using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private const int EnemyDecCardCount = 6;
    private const int InitialOpenCardCount = 3;

    public static EnemyController Instance { get; private set; }

    [SerializeField] private List<Transform> _cardParents = new List<Transform>(EnemyDecCardCount);

    private EnemyState _currentState = EnemyState.None;
    private List<int> _currentDec = new List<int>(EnemyDecCardCount);
    private List<BaseCard> _cards = new List<BaseCard>(EnemyDecCardCount);
    private Queue<BaseCard> _waitingCardQueue = new Queue<BaseCard>(EnemyDecCardCount);
    private Coroutine _openCardsCoroutine;

    public EnemyState CurrentState => _currentState;
    public List<BaseCard> Cards => _cards;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Initialize()
    {
        ChangeState(EnemyState.Idle);
    }

    public List<int> GetDec()
    {
        if (_currentDec == null || _currentDec.Count == 0)
        {
            _currentDec = CreateRandomDec();
        }

        return _currentDec;
    }

    public void CreateCardsInRandomOrder(Action onComplete)
    {
        List<int> randomOrderedDec = new List<int>(GetDec());
        ShuffleDec(randomOrderedDec);

        _cards.Clear();
        _waitingCardQueue.Clear();

        int cardIndex = 0;
        foreach (int cardId in randomOrderedDec)
        {
            BaseCard card = CreateCard(cardId, cardIndex);
            if (card != null)
            {
                _cards.Add(card);
                _waitingCardQueue.Enqueue(card);

                cardIndex++;
            }
        }

        StartOpenCards(onComplete);
    }

    public void ChangeState(EnemyState nextState)
    {
        if (_currentState == nextState)
        {
            return;
        }

        _currentState = nextState;
        EnterState(nextState);
    }

    public List<BaseCard> GetAttackTargetCards()
    {
        List<BaseCard> targetCards = new List<BaseCard>(3);

        for (int i = 0; i < InitialOpenCardCount; i++)
        {
            if (_cards[i] == null)
            {
                continue;
            }

            targetCards.Add(_cards[i]);
        }

        return targetCards;
    }

    public void OnCardDestroyed(BaseCard destroyedCard)
    {
        if (destroyedCard == null)
        {
            return;
        }

        int slotIndex = destroyedCard.SlotIndex;
        if (slotIndex < 0 || slotIndex >= _cards.Count)
        {
            return;
        }

        if (_cards[slotIndex] == destroyedCard)
        {
            _cards[slotIndex] = null;
        }

        FillEmptySlot(slotIndex);
    }

    private void Update()
    {
        TickState(_currentState);
    }

    private void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Think:
                break;
            case EnemyState.Act:
                break;
            case EnemyState.End:
                break;
        }
    }

    private void TickState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Think:
                break;
            case EnemyState.Act:
                break;
            case EnemyState.End:
                break;
        }
    }

    private List<int> CreateRandomDec()
    {
        List<CardData> cardDataList = new List<CardData>(DataManager.Instance.CardDataById.Values);
        HashSet<int> selectedCardTypes = new HashSet<int>();
        List<int> dec = new List<int>(EnemyDecCardCount);

        Shuffle(cardDataList);

        foreach (CardData cardData in cardDataList)
        {
            if (!selectedCardTypes.Add(cardData.cardType))
            {
                continue;
            }

            dec.Add(cardData.cardId);
            if (dec.Count >= EnemyDecCardCount)
            {
                break;
            }
        }

        return dec;
    }

    private void Shuffle(List<CardData> cardDataList)
    {
        for (int i = 0; i < cardDataList.Count - 1; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, cardDataList.Count);
            CardData currentCardData = cardDataList[i];
            cardDataList[i] = cardDataList[randomIndex];
            cardDataList[randomIndex] = currentCardData;
        }
    }

    private void ShuffleDec(List<int> dec)
    {
        for (int i = 0; i < dec.Count - 1; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, dec.Count);
            int currentCardId = dec[i];
            dec[i] = dec[randomIndex];
            dec[randomIndex] = currentCardId;
        }
    }

    private BaseCard CreateCard(int cardId, int cardIndex)
    {
        CardData cardData = DataManager.Instance.CardDataById[cardId];
        string resourceName = string.Concat((CardType)cardData.cardType, GlobalString.Card);
        GameObject prefab = ResourceManager.Instance.LoadGameObject(GlobalString.ResourceCardsPath, resourceName);
        if (prefab == null)
        {
            return null;
        }

        Transform parent = GetCardParent(cardIndex);
        GameObject cardObject = Instantiate(prefab, parent);
        BaseCard card = cardObject.GetComponent<BaseCard>();
        if (card == null)
        {
            Debug.LogError(string.Format(GlobalString.CardComponentNotFoundMessage, resourceName));
            return null;
        }

        card.cardId = cardId;
        card.SetOwner(CardOwner.Enemy);
        card.SetSlotIndex(cardIndex);
        card.Initialize();

        return card;
    }

    private Transform GetCardParent(int cardIndex)
    {
        if (cardIndex >= _cardParents.Count)
        {
            return transform;
        }

        return _cardParents[cardIndex] == null ? transform : _cardParents[cardIndex];
    }

    private void StartOpenCards(Action onComplete)
    {
        if (_openCardsCoroutine != null)
        {
            StopCoroutine(_openCardsCoroutine);
        }

        _openCardsCoroutine = StartCoroutine(OpenCards(onComplete));
    }

    private IEnumerator OpenCards(Action onComplete)
    {
        for (int i = 0; i < InitialOpenCardCount; i++)
        {
            BaseCard nextCard = _waitingCardQueue.Dequeue();
            nextCard.FlipToFront();
            yield return new WaitForSeconds(nextCard.FlipDuration);
        }

        _openCardsCoroutine = null;
        onComplete?.Invoke();
    }

    private void FillEmptySlot(int slotIndex)
    {
        if (_waitingCardQueue.Count == 0)
        {
            return;
        }

        BaseCard nextCard = _waitingCardQueue.Dequeue();
        if (nextCard == null)
        {
            return;
        }

        int hiddenSlotIndex = nextCard.SlotIndex;
        if (hiddenSlotIndex >= 0 && hiddenSlotIndex < _cards.Count && _cards[hiddenSlotIndex] == nextCard)
        {
            _cards[hiddenSlotIndex] = null;
        }

        _cards[slotIndex] = nextCard;
        nextCard.transform.SetParent(GetCardParent(slotIndex), false);
        nextCard.transform.localPosition = Vector3.zero;
        nextCard.transform.localRotation = Quaternion.identity;
        nextCard.SetSlotIndex(slotIndex);
        nextCard.FlipToFront();
    }

    private void OnDestroy()
    {
        if (_openCardsCoroutine != null)
        {
            StopCoroutine(_openCardsCoroutine);
            _openCardsCoroutine = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
