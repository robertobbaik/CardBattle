using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly Color AttackTargetHighlightColor = new Color(1f, 0f, 0f, 1f);

    public static GameManager Instance { get; private set; }

    [SerializeField] private StartPanel _startPanel;
    [SerializeField] private CardSelectPanel _selectPanel;
    [SerializeField] private ActionPanel _actionPanel;

    private BaseCard _focusedActionCard;
    private List<BaseCard> _attackTargetCards = new List<BaseCard>(3);
    private bool _isAttackSelectionActive;

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

        EnemyController.Instance.Initialize();
        TurnManager.Instance.Initialize();
        PlayerController.Instance.InitializeDec();

        if (_startPanel == null)
        {
            StartCardSelection();
            return;
        }

        _startPanel.PlayStartSequence(StartCardSelection);
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
        EnemyController.Instance.CreateCardsInRandomOrder(StartBattle);
    }

    private void StartBattle()
    {
        TurnManager.Instance.StartPlayerTurn();
    }

    public void OnClickBattleCard(BaseCard card)
    {
        if (card == null)
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

        FocusActionCard(card);
    }

    public void BeginAttackSelection()
    {
        if (_focusedActionCard == null)
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
        if (clickedCard.Owner != CardOwner.Enemy)
        {
            return;
        }

        if (!IsAttackTargetCard(clickedCard))
        {
            return;
        }

        if (_focusedActionCard != null)
        {
            _focusedActionCard.Attack(clickedCard);
        }

        CancelActionFocus();
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
