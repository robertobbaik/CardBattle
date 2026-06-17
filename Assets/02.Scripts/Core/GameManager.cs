using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly Color AttackTargetHighlightColor = new Color(1f, 0f, 0f, 1f);

    public static GameManager Instance { get; private set; }

    [SerializeField] private StartPanel _startPanel;
    [SerializeField] private CardSelectPanel _selectPanel;
    [SerializeField] private ActionPanel _actionPanel;

    private SequenceState _sequenceState = SequenceState.None;
    private BaseCard _focusedActionCard;
    private List<BaseCard> _attackTargetCards = new List<BaseCard>(3);
    private bool _isAttackSelectionActive;
    private bool _battleEnded;
    private Coroutine _returnToLobbyCoroutine;

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
        _battleEnded = false;

        EnemyController.Instance.Initialize();
        TurnManager.Instance.Initialize();
        PlayerController.Instance.InitializeDec();
        BeginSequenceState();

        if (_startPanel == null)
        {
            EndSequenceState();
            StartCardSelection();
            return;
        }

        _startPanel.PlayStartSequence(OnStartSequenceComplete);
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
        PlayerController.Instance.CreateCards(selectedCardIds);
        BeginSequenceState();
        EnemyController.Instance.CreateCards(StartBattle);
    }

    private void StartBattle()
    {
        EndSequenceState();
        TurnManager.Instance.StartPlayerTurn();
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

        if (card.SlotIndex >= 3)
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

    public void BeginEnemyActionSequence()
    {
        if (_battleEnded || TurnManager.Instance == null || EnemyController.Instance == null)
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
            TurnManager.Instance.EndTurn();
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

        if (CheckBattleResult())
        {
            return;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.EndTurn();
        }

        if (!CheckBattleResult())
        {
            BeginEnemyActionSequence();
        }
    }

    private void ResolveEnemyAttackAfterLine(BaseCard attacker, BaseCard target, Action onComplete)
    {
        if (attacker != null && target != null)
        {
            attacker.Attack(target);
        }

        CheckBattleResult();
        onComplete?.Invoke();
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

        bool playerHasCards = PlayerController.Instance != null && PlayerController.Instance.HasAliveBattlefieldCards();
        bool enemyHasCards = EnemyController.Instance != null && EnemyController.Instance.HasAliveBattlefieldCards();

        if (playerHasCards && enemyHasCards)
        {
            return false;
        }

        string winner = "Draw";
        if (playerHasCards && !enemyHasCards)
        {
            winner = "Player";
        }
        else if (!playerHasCards && enemyHasCards)
        {
            winner = "Enemy";
        }

        BeginGameOver(winner);
        return true;
    }

    private void BeginGameOver(string winner)
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

        Debug.Log(string.Format("GameOver - Winner: {0}", winner));

        if (_returnToLobbyCoroutine != null)
        {
            StopCoroutine(_returnToLobbyCoroutine);
        }

        _returnToLobbyCoroutine = StartCoroutine(ReturnToLobbyCoroutine());
    }

    private IEnumerator ReturnToLobbyCoroutine()
    {
        yield return new WaitForSeconds(1.25f);

        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadScene(SceneType.LobbyScene);
        }
    }
}
