using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static readonly Color AttackTargetHighlightColor = new Color(1f, 0f, 0f, 1f);
    private const float TurnPanelDuration = 2f;
    private const float GameOverReturnDelay = 3f;
    private const float InitialRevealSideInterval = 0.8f;

    public static GameManager Instance { get; private set; }

    [SerializeField] private StartPanel _startPanel;
    [SerializeField] private CardSelectPanel _selectPanel;
    [SerializeField] private ActionPanel _actionPanel;
    [SerializeField] private GameObject _turnPanel;
    [SerializeField] private Image _yourTurn;
    [SerializeField] private Image _enemyTurn;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Image _victory;
    [SerializeField] private Image _draw;
    [SerializeField] private Image _defeat;

    private SequenceState _sequenceState = SequenceState.None;
    private BaseCard _focusedActionCard;
    private List<BaseCard> _attackTargetCards = new List<BaseCard>(3);
    private bool _isAttackSelectionActive;
    private bool _battleEnded;
    private Coroutine _returnToLobbyCoroutine;
    private Coroutine _waitCardSequencesCoroutine;
    private Coroutine _startBattleCoroutine;
    private Coroutine _initialRevealSideIntervalCoroutine;
    private CardOwner _opponentRevealDebuffsSuppressedOwner = CardOwner.None;
    private List<BaseCard> _queuedInitialRevealEffectCards = new List<BaseCard>(3);
    private bool _isQueuingInitialRevealEffects;
    private TurnOwner _firstTurnOwner = TurnOwner.Player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HideActionPanel();
        HideTurnPanel();
        HideGameOverPanel();
        _battleEnded = false;

        EnemyController.Instance.Initialize();
        TurnManager.Instance.Initialize();
        PlayerController.Instance.InitializeDec();
        _firstTurnOwner = GetRandomFirstTurnOwner();
        BeginSequenceState();

        if (_startPanel == null)
        {
            EndSequenceState();
            StartCardSelection();
            return;
        }

        _startPanel.PlayStartSequence(_firstTurnOwner, OnStartSequenceComplete);
    }

    private void OnStartSequenceComplete()
    {
        EndSequenceState();
        StartCardSelection();
    }

    private void StartCardSelection()
    {
        if (_selectPanel == null)
        {
            StartBattle();
            return;
        }

        _selectPanel.Show(PlayerController.Instance.CurrentDec, OnCardSelectionComplete);
    }

    private void OnCardSelectionComplete(List<int> selectedCardIds)
    {
        _selectPanel.Hide();
        BeginSequenceState();
        StartInitialCardReveal(selectedCardIds, _firstTurnOwner);
    }

    private TurnOwner GetRandomFirstTurnOwner()
    {
        return UnityEngine.Random.Range(0, 2) == 0 ? TurnOwner.Player : TurnOwner.Enemy;
    }

    private void StartInitialCardReveal(List<int> selectedCardIds, TurnOwner firstTurnOwner)
    {
        if (firstTurnOwner == TurnOwner.Enemy)
        {
            EnemyController.Instance.CreateCards(delegate
            {
                StartSecondInitialRevealAfterInterval(delegate
                {
                    PlayerController.Instance.CreateCards(selectedCardIds, false, StartBattle);
                });
            }, true);
            return;
        }

        PlayerController.Instance.CreateCards(selectedCardIds, true, delegate
        {
            StartSecondInitialRevealAfterInterval(delegate
            {
                EnemyController.Instance.CreateCards(StartBattle, false);
            });
        });
    }

    private void StartSecondInitialRevealAfterInterval(Action startSecondReveal)
    {
        if (_initialRevealSideIntervalCoroutine != null)
        {
            StopCoroutine(_initialRevealSideIntervalCoroutine);
        }

        _initialRevealSideIntervalCoroutine = StartCoroutine(SecondInitialRevealIntervalCoroutine(startSecondReveal));
    }

    private IEnumerator SecondInitialRevealIntervalCoroutine(Action startSecondReveal)
    {
        if (InitialRevealSideInterval > 0f)
        {
            yield return new WaitForSeconds(InitialRevealSideInterval);
        }

        _initialRevealSideIntervalCoroutine = null;
        startSecondReveal?.Invoke();
    }

    private void StartBattle()
    {
        if (_startBattleCoroutine != null)
        {
            StopCoroutine(_startBattleCoroutine);
        }

        _startBattleCoroutine = StartCoroutine(StartBattleCoroutine());
    }

    private IEnumerator StartBattleCoroutine()
    {
        BeginSequenceState();

        while (HasActiveBattlefieldCardSequences())
        {
            yield return null;
        }

        yield return ShowTurnTransition(_firstTurnOwner);

        if (!_battleEnded && TurnManager.Instance != null)
        {
            if (_firstTurnOwner == TurnOwner.Enemy)
            {
                TurnManager.Instance.StartEnemyTurn();
                BeginEnemyActionSequence();
                _startBattleCoroutine = null;
                yield break;
            }
            else
            {
                TurnManager.Instance.StartPlayerTurn();
            }
        }

        _startBattleCoroutine = null;
        EndSequenceState();
    }

    public void OnClickBattleCard(BaseCard card)
    {
        if (card == null || IsSequenceStateActive() || _battleEnded)
        {
            return;
        }

        if (_isAttackSelectionActive)
        {
            TryResolveAttack(card);
            return;
        }

        if (TurnManager.Instance == null || TurnManager.Instance.CurrentTurnOwner != TurnOwner.Player)
        {
            return;
        }

        if (!card.IsAlive || !card.IsOpen)
        {
            return;
        }

        if (card.Owner != CardOwner.Player)
        {
            return;
        }

        if (!card.CanAttack)
        {
            return;
        }

        FocusActionCard(card);
    }

    public void BeginAttackSelection()
    {
        if (_focusedActionCard == null || IsSequenceStateActive() || _battleEnded)
        {
            return;
        }

        if (!_focusedActionCard.CanAttack)
        {
            return;
        }

        if (TurnManager.Instance == null || TurnManager.Instance.CurrentTurnOwner != TurnOwner.Player)
        {
            return;
        }

        ClearAttackTargetHighlights();

        List<BaseCard> targetCards = EnemyController.Instance.GetAttackTargetCards();
        if (targetCards.Count == 0)
        {
            return;
        }

        _attackTargetCards = targetCards;
        _isAttackSelectionActive = true;

        for (int i = 0; i < _attackTargetCards.Count; i++)
        {
            BaseCard targetCard = _attackTargetCards[i];
            targetCard.SetHighlightColor(AttackTargetHighlightColor);
            targetCard.ShowHighlight();
        }
    }

    public void CancelActionFocus()
    {
        if (IsSequenceStateActive() || _battleEnded)
        {
            return;
        }

        ClearAttackTargetHighlights();

        if (_focusedActionCard != null)
        {
            _focusedActionCard.ResetHighlightColor();
            _focusedActionCard.HideHighlight();
            _focusedActionCard = null;
        }

        SetActionPanelActive(false);
    }

    public void SetActionPanelActive(bool isActive)
    {
        if (IsSequenceStateActive() || _battleEnded)
        {
            return;
        }

        if (isActive)
        {
            ShowActionPanel();
            return;
        }

        HideActionPanel();
    }

    private void FocusActionCard(BaseCard card)
    {
        ClearAttackTargetHighlights();

        if (_focusedActionCard != null && _focusedActionCard != card)
        {
            _focusedActionCard.ResetHighlightColor();
            _focusedActionCard.HideHighlight();
        }

        _focusedActionCard = card;
        _focusedActionCard.ResetHighlightColor();
        _focusedActionCard.ShowHighlight();
        SetActionPanelActive(true);
    }

    private void TryResolveAttack(BaseCard clickedCard)
    {
        if (_focusedActionCard == null || !_focusedActionCard.CanAttack)
        {
            return;
        }

        if (clickedCard.Owner != CardOwner.Enemy)
        {
            return;
        }

        if (!IsAttackTargetCard(clickedCard))
        {
            return;
        }

        BaseCard attacker = _focusedActionCard;
        CancelActionFocus();
        BeginPlayerAttackSequence(attacker, clickedCard);
    }

    private bool IsAttackTargetCard(BaseCard clickedCard)
    {
        for (int i = 0; i < _attackTargetCards.Count; i++)
        {
            if (_attackTargetCards[i] == clickedCard)
            {
                return true;
            }
        }

        return false;
    }

    private void ClearAttackTargetHighlights()
    {
        _isAttackSelectionActive = false;

        for (int i = 0; i < _attackTargetCards.Count; i++)
        {
            BaseCard targetCard = _attackTargetCards[i];
            if (targetCard == null)
            {
                continue;
            }

            targetCard.ResetHighlightColor();
            targetCard.HideHighlight();
        }

        _attackTargetCards.Clear();
    }

    private void ShowActionPanel()
    {
        if (_actionPanel == null)
        {
            return;
        }

        _actionPanel.Show();
    }

    private void HideActionPanel()
    {
        if (_actionPanel == null)
        {
            return;
        }

        _actionPanel.Hide();
    }

    public void BeginSequenceState()
    {
        _sequenceState = SequenceState.Sequence;
    }

    public void EndSequenceState()
    {
        _sequenceState = SequenceState.None;
    }

    public bool IsSequenceStateActive()
    {
        return _sequenceState == SequenceState.Sequence;
    }

    public void SetOpponentRevealDebuffsSuppressed(CardOwner owner, bool isSuppressed)
    {
        _opponentRevealDebuffsSuppressedOwner = isSuppressed ? owner : CardOwner.None;
    }

    public bool IsOpponentRevealDebuffSuppressed(CardOwner owner)
    {
        return _opponentRevealDebuffsSuppressedOwner == owner;
    }

    public void BeginInitialRevealEffectQueue()
    {
        _queuedInitialRevealEffectCards.Clear();
        _isQueuingInitialRevealEffects = true;
    }

    public bool TryQueueInitialRevealEffect(BaseCard card)
    {
        if (!_isQueuingInitialRevealEffects)
        {
            return false;
        }

        if (card != null)
        {
            _queuedInitialRevealEffectCards.Add(card);
        }

        return true;
    }

    public IEnumerator FlushInitialRevealEffectQueue()
    {
        if (!_isQueuingInitialRevealEffects)
        {
            yield break;
        }

        _isQueuingInitialRevealEffects = false;

        for (int i = 0; i < _queuedInitialRevealEffectCards.Count; i++)
        {
            BaseCard card = _queuedInitialRevealEffectCards[i];
            if (card == null)
            {
                continue;
            }

            card.ResolveQueuedEnterFieldEffect();

            while (HasActiveBattlefieldCardSequences())
            {
                yield return null;
            }
        }

        _queuedInitialRevealEffectCards.Clear();
    }

    public IEnumerator ShowTurnTransition(TurnOwner turnOwner)
    {
        ShowTurnPanel(turnOwner);
        yield return new WaitForSeconds(TurnPanelDuration);
        HideTurnPanel();
    }

    public void BeginEnemyActionSequence()
    {
        if (_battleEnded || TurnManager.Instance == null || EnemyController.Instance == null)
        {
            return;
        }

        if (TurnManager.Instance.CurrentTurnOwner != TurnOwner.Enemy || TurnManager.Instance.IsTransitioningTurn)
        {
            return;
        }

        if (!IsSequenceStateActive())
        {
            BeginSequenceState();
        }

        EnemyController.Instance.StartActionSequence(OnEnemyActionSequenceComplete);
    }

    private void OnEnemyActionSequenceComplete()
    {
        if (_battleEnded)
        {
            EndSequenceState();
            return;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.EndTurn(delegate
            {
                EndSequenceState();
            });
            return;
        }

        EndSequenceState();
    }

    private void BeginPlayerAttackSequence(BaseCard attacker, BaseCard target)
    {
        BeginAttackSequence(attacker, target, delegate
        {
            ResolvePlayerAttackAfterLine(attacker, target);
        });
    }

    public void BeginEnemyAttackSequence(BaseCard attacker, BaseCard target, Action onComplete)
    {
        BeginAttackSequence(attacker, target, delegate
        {
            ResolveEnemyAttackAfterLine(attacker, target, onComplete);
        });
    }

    private void BeginAttackSequence(BaseCard attacker, BaseCard target, Action onLineComplete)
    {
        if (attacker == null || target == null)
        {
            return;
        }

        BeginSequenceState();

        Vector3 attackerPosition = attacker.transform.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 centerPosition = (attackerPosition + targetPosition) * 0.5f;
        float angle = GetAttackAngle(attackerPosition, targetPosition);
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        Transform fxParent = GetFxParent(attacker, target);

        if (FXManager.Instance == null)
        {
            onLineComplete?.Invoke();
            return;
        }

        FXManager.Instance.PlayLiningFX(fxParent, centerPosition, rotation, delegate
        {
            FXManager.Instance.PlayDoubleSlashFX(fxParent, targetPosition, rotation, delegate
            {
                onLineComplete?.Invoke();
            });
        });
    }

    private void ResolvePlayerAttackAfterLine(BaseCard attacker, BaseCard target)
    {
        if (attacker != null && target != null)
        {
            attacker.Attack(target);
        }

        StartWaitCardSequences(delegate
        {
            if (CheckBattleResult())
            {
                return;
            }

            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.EndTurn();
            }
        });
    }

    private void ResolveEnemyAttackAfterLine(BaseCard attacker, BaseCard target, Action onComplete)
    {
        if (attacker != null && target != null)
        {
            attacker.Attack(target);
        }

        StartWaitCardSequences(delegate
        {
            CheckBattleResult();
            onComplete?.Invoke();
        });
    }

    private void StartWaitCardSequences(Action onComplete)
    {
        if (_waitCardSequencesCoroutine != null)
        {
            StopCoroutine(_waitCardSequencesCoroutine);
        }

        _waitCardSequencesCoroutine = StartCoroutine(WaitCardSequencesCoroutine(onComplete));
    }

    private IEnumerator WaitCardSequencesCoroutine(Action onComplete)
    {
        while (HasActiveBattlefieldCardSequences())
        {
            yield return null;
        }

        _waitCardSequencesCoroutine = null;
        onComplete?.Invoke();
    }

    private bool HasActiveBattlefieldCardSequences()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.HasActiveBattlefieldCardSequences())
        {
            return true;
        }

        if (EnemyController.Instance != null && EnemyController.Instance.HasActiveBattlefieldCardSequences())
        {
            return true;
        }

        return false;
    }

    private static float GetAttackAngle(Vector3 fromPosition, Vector3 toPosition)
    {
        Vector3 direction = toPosition - fromPosition;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private static Transform GetFxParent(BaseCard attacker, BaseCard target)
    {
        if (attacker != null && attacker.transform != null && attacker.transform.root != null)
        {
            return attacker.transform.root;
        }

        if (target != null && target.transform != null && target.transform.root != null)
        {
            return target.transform.root;
        }

        return null;
    }

    private void OnDestroy()
    {
        if (_startBattleCoroutine != null)
        {
            StopCoroutine(_startBattleCoroutine);
            _startBattleCoroutine = null;
        }

        if (_waitCardSequencesCoroutine != null)
        {
            StopCoroutine(_waitCardSequencesCoroutine);
            _waitCardSequencesCoroutine = null;
        }

        if (_initialRevealSideIntervalCoroutine != null)
        {
            StopCoroutine(_initialRevealSideIntervalCoroutine);
            _initialRevealSideIntervalCoroutine = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool CheckBattleResult()
    {
        if (_battleEnded)
        {
            return true;
        }

        if (PlayerController.Instance != null && PlayerController.Instance.HasDestroyingBattlefieldCards())
        {
            return false;
        }

        if (EnemyController.Instance != null && EnemyController.Instance.HasDestroyingBattlefieldCards())
        {
            return false;
        }

        bool playerHasCards = PlayerController.Instance != null && PlayerController.Instance.HasAliveBattlefieldCards();
        bool enemyHasCards = EnemyController.Instance != null && EnemyController.Instance.HasAliveBattlefieldCards();

        if (playerHasCards && enemyHasCards)
        {
            return false;
        }

        BattleResult result = BattleResult.Draw;
        if (playerHasCards && !enemyHasCards)
        {
            result = BattleResult.Victory;
        }
        else if (!playerHasCards && enemyHasCards)
        {
            result = BattleResult.Defeat;
        }

        BeginGameOver(result);
        return true;
    }

    private void BeginGameOver(BattleResult result)
    {
        if (_battleEnded)
        {
            return;
        }

        _battleEnded = true;
        BeginSequenceState();
        HideActionPanel();
        ClearAttackTargetHighlights();

        if (_focusedActionCard != null)
        {
            _focusedActionCard.ResetHighlightColor();
            _focusedActionCard.HideHighlight();
            _focusedActionCard = null;
        }

        Debug.Log(string.Format("GameOver - Result: {0}", result));
        ShowGameOverPanel(result);

        if (_returnToLobbyCoroutine != null)
        {
            StopCoroutine(_returnToLobbyCoroutine);
        }

        _returnToLobbyCoroutine = StartCoroutine(ReturnToLobbyCoroutine());
    }

    private IEnumerator ReturnToLobbyCoroutine()
    {
        yield return new WaitForSeconds(GameOverReturnDelay);

        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadScene(SceneType.LobbyScene);
        }
    }

    private void ShowTurnPanel(TurnOwner turnOwner)
    {
        SetImageActive(_yourTurn, turnOwner == TurnOwner.Player);
        SetImageActive(_enemyTurn, turnOwner == TurnOwner.Enemy);

        if (_turnPanel != null)
        {
            _turnPanel.SetActive(true);
        }
    }

    private void HideTurnPanel()
    {
        SetImageActive(_yourTurn, false);
        SetImageActive(_enemyTurn, false);

        if (_turnPanel != null)
        {
            _turnPanel.SetActive(false);
        }
    }

    private void ShowGameOverPanel(BattleResult result)
    {
        SetImageActive(_victory, result == BattleResult.Victory);
        SetImageActive(_draw, result == BattleResult.Draw);
        SetImageActive(_defeat, result == BattleResult.Defeat);

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
    }

    private void HideGameOverPanel()
    {
        SetImageActive(_victory, false);
        SetImageActive(_draw, false);
        SetImageActive(_defeat, false);

        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(false);
        }
    }

    private static void SetImageActive(Image image, bool isActive)
    {
        if (image != null)
        {
            image.gameObject.SetActive(isActive);
        }
    }

    private enum BattleResult
    {
        Victory,
        Draw,
        Defeat
    }
}
