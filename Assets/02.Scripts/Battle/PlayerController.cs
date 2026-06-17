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
    private int _guardDamageReduction;

    public List<int> CurrentDec => _currentDec;
    public List<BaseCard> Cards => _cards;
    public Queue<BaseCard> WaitingCardQueue => _waitingCardQueue;
    public int GuardDamageReduction => _guardDamageReduction;

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

    public void OnTurnStart()
    {
        ClearGuard();

        for (int i = 0; i < _cards.Count; i++)
        {
            BaseCard card = _cards[i];
            if (card == null || !card.IsAlive || !card.IsOpen)
            {
                continue;
            }

            card.ResetTurnAction();
            card.OnTurnStart();
        }
    }

    public void ApplyGuard()
    {
        _guardDamageReduction = 1;
    }

    public int GetGuardDamageReduction()
    {
        return _guardDamageReduction;
    }

    public BaseCard GetCardAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _cards.Count)
        {
            return null;
        }

        return _cards[slotIndex];
    }

    public BaseCard GetRandomAdjacentAliveCard(int slotIndex)
    {
        List<BaseCard> candidates = GetAdjacentAliveCards(slotIndex);
        if (candidates.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
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

        FillEmptySlot(destroyedCard);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckBattleResult();
        }
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

    private void FillEmptySlot(BaseCard destroyedCard)
    {
        if (_waitingCardQueue.Count == 0)
        {
            return;
        }

        int slotIndex = destroyedCard.SlotIndex;
        BaseCard nextCard = _waitingCardQueue.Dequeue();
        if (nextCard == null)
        {
            return;
        }

        int hiddenSlotIndex = nextCard.SlotIndex;
        if (hiddenSlotIndex >= 0 && hiddenSlotIndex < _cards.Count)
        {
            _cards[hiddenSlotIndex] = destroyedCard;
        }

        _cards[slotIndex] = nextCard;
        destroyedCard.SetSlotIndex(hiddenSlotIndex);
        destroyedCard.transform.SetParent(GetCardParent(hiddenSlotIndex), false);
        destroyedCard.transform.localPosition = Vector3.zero;
        destroyedCard.transform.localRotation = Quaternion.identity;

        nextCard.transform.SetParent(GetCardParent(slotIndex), false);
        nextCard.transform.localPosition = Vector3.zero;
        nextCard.transform.localRotation = Quaternion.identity;
        nextCard.SetSlotIndex(slotIndex);
        nextCard.FlipToFront();
    }

    private void ClearGuard()
    {
        _guardDamageReduction = 0;
    }

    private List<BaseCard> GetAdjacentAliveCards(int slotIndex)
    {
        List<BaseCard> candidates = new List<BaseCard>(2);

        AddAdjacentCandidate(candidates, slotIndex - 1);
        AddAdjacentCandidate(candidates, slotIndex + 1);

        return candidates;
    }

    private void AddAdjacentCandidate(List<BaseCard> candidates, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _cards.Count)
        {
            return;
        }

        BaseCard card = _cards[slotIndex];
        if (card == null || !card.IsAlive || !card.IsOpen)
        {
            return;
        }

        candidates.Add(card);
    }

    public bool HasAliveBattlefieldCards()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            BaseCard card = _cards[i];
            if (card != null && card.IsAlive && (card.IsOpen || card.IsOpening))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasDestroyingBattlefieldCards()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            BaseCard card = _cards[i];
            if (card != null && card.IsAlive && (card.IsOpen || card.IsOpening) && card.IsDestroying)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
