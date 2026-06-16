using System;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectPanel : MonoBehaviour
{
    public static CardSelectPanel Instance { get; private set; }

    [SerializeField] private List<CardItem> _cardItems = new List<CardItem>(6);

    private List<int> _dec = new List<int>();
    private Queue<int> _selectedCardIdQueue = new Queue<int>();
    private int _selectedCardCount;
    private int _selectionTargetCount;
    private bool _isSelectionComplete;
    private Action<List<int>> _selectionCompleteCallback;

    public bool IsSelectionComplete => _isSelectionComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Show(List<int> dec, Action<List<int>> onComplete)
    {
        gameObject.SetActive(true);
        _selectionCompleteCallback = onComplete;
        Initialize(dec);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Initialize(List<int> dec)
    {
        _dec = new List<int>(dec);
        _selectedCardIdQueue.Clear();
        _selectedCardCount = 0;
        _selectionTargetCount = 0;
        _isSelectionComplete = false;

        InitializeCardItems();
    }

    public void OnClickCardItem(CardItem cardItem)
    {
        if (cardItem.IsSelected)
        {
            if (cardItem.Order == _selectedCardCount)
            {
                cardItem.SetOrder(0);
                _selectedCardCount--;
                UpdateSelectedCardIdQueue();
            }

            return;
        }

        if (_isSelectionComplete)
        {
            return;
        }

        if (_selectedCardCount >= _selectionTargetCount || cardItem.CardIndex >= _dec.Count)
        {
            return;
        }

        _selectedCardCount++;
        cardItem.SetOrder(_selectedCardCount);
        UpdateSelectedCardIdQueue();
    }

    private void InitializeCardItems()
    {
        for (int i = 0; i < _cardItems.Count; i++)
        {
            CardItem cardItem = _cardItems[i];
            if (cardItem == null)
            {
                continue;
            }

            if (i >= _dec.Count)
            {
                cardItem.gameObject.SetActive(false);
                continue;
            }

            CardData cardData = DataManager.Instance.CardDataById[_dec[i]];
            cardItem.gameObject.SetActive(true);
            cardItem.Initialize(cardData, i);
            _selectionTargetCount++;
        }
    }

    private void UpdateSelectedCardIdQueue()
    {
        _selectedCardIdQueue.Clear();
        _isSelectionComplete = false;

        if (_selectedCardCount < _selectionTargetCount)
        {
            return;
        }

        for (int order = 1; order <= _selectedCardCount; order++)
        {
            CardItem cardItem = GetCardItemByOrder(order);
            if (cardItem == null || cardItem.CardIndex >= _dec.Count)
            {
                _selectedCardIdQueue.Clear();
                return;
            }

            _selectedCardIdQueue.Enqueue(_dec[cardItem.CardIndex]);
        }

        CompleteSelection();
    }

    private void CompleteSelection()
    {
        _isSelectionComplete = true;

        Action<List<int>> completeCallback = _selectionCompleteCallback;
        _selectionCompleteCallback = null;

        completeCallback?.Invoke(new List<int>(_selectedCardIdQueue));
    }

    private CardItem GetCardItemByOrder(int order)
    {
        for (int i = 0; i < _cardItems.Count; i++)
        {
            CardItem cardItem = _cardItems[i];
            if (cardItem != null && cardItem.Order == order)
            {
                return cardItem;
            }
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
}
