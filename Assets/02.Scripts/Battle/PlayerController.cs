using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private const int InitialOpenCardCount = 3;
    private const int TotalCardCount = 6;

    public static PlayerController Instance { get; private set; }

    [SerializeField] private List<Transform> _cardParents = new List<Transform>(6);

    private List<int> _currentDec = new List<int>();
    private List<BaseCard> _cards = new List<BaseCard>(TotalCardCount);
    private Queue<BaseCard> _waitingCardQueue = new Queue<BaseCard>();
    private Coroutine _openCardsCoroutine;

    public List<int> CurrentDec => _currentDec;
    public List<BaseCard> Cards => _cards;
    public Queue<BaseCard> WaitingCardQueue => _waitingCardQueue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void InitializeDec()
    {
        _currentDec = new List<int>(UserInfoManager.Instance.CurrentDec);
        _cards.Clear();
        _waitingCardQueue.Clear();
    }

    public void CreateCards(List<int> selectedCardIds)
    {
        _cards.Clear();
        _waitingCardQueue.Clear();

        int cardIndex = 0;
        foreach (int cardId in selectedCardIds)
        {
            BaseCard card = CreateCard(cardId, cardIndex);
            if (card != null)
            {
                _cards.Add(card);
                _waitingCardQueue.Enqueue(card);

                cardIndex++;
            }
        }

        StartOpenCards();
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
        card.SetOwner(CardOwner.Player);
        card.SetSlotIndex(cardIndex);
        card.Initialize();

        return card;
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

    private Transform GetCardParent(int cardIndex)
    {
        if (cardIndex >= _cardParents.Count)
        {
            return transform;
        }

        return _cardParents[cardIndex] == null ? transform : _cardParents[cardIndex];
    }

    private void StartOpenCards()
    {
        if (_openCardsCoroutine != null)
        {
            StopCoroutine(_openCardsCoroutine);
        }

        _openCardsCoroutine = StartCoroutine(OpenCards());
    }

    private IEnumerator OpenCards()
    {
        for (int i = 0; i < InitialOpenCardCount; i++)
        {
            BaseCard nextCard = _waitingCardQueue.Dequeue();
            nextCard.FlipToFront();
            yield return new WaitForSeconds(nextCard.FlipDuration);
        }

        _openCardsCoroutine = null;
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
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
