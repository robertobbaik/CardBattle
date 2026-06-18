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
    private Coroutine _actionSequenceCoroutine;
    private int _guardDamageReduction;

    public EnemyState CurrentState => _currentState;
    public List<BaseCard> Cards => _cards;
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

    public void Initialize()
    {
        _currentDec.Clear();
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

    private List<int> CreateRandomDec()
    {
        List<int> cardIds = new List<int>(DataManager.Instance.CardDataById.Keys);
        ShuffleDec(cardIds);

        int decCount = Mathf.Min(EnemyDecCardCount, cardIds.Count);
        List<int> randomDec = cardIds.GetRange(0, decCount);

        if (randomDec.Count < EnemyDecCardCount)
        {
            Debug.LogWarning(string.Format("Enemy Dec has only {0} cards because card data is smaller than required count {1}.", randomDec.Count, EnemyDecCardCount));
        }

        Debug.Log(string.Format("Enemy Dec Generated - {0}", string.Join(",", randomDec)));
        return randomDec;
    }

    public void CreateCards(Action onComplete)
    {
        List<int> orderedDec = new List<int>(GetDec());
        ShuffleDec(orderedDec);

        _cards.Clear();
        _waitingCardQueue.Clear();

        int cardIndex = 0;
        foreach (int cardId in orderedDec)
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

    private void ShuffleDec(List<int> dec)
    {
        if (dec == null || dec.Count < 2)
        {
            return;
        }

        for (int i = dec.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            if (randomIndex == i)
            {
                continue;
            }

            int temp = dec[i];
            dec[i] = dec[randomIndex];
            dec[randomIndex] = temp;
        }

        Debug.Log(string.Format("Enemy Dec Shuffled - {0}", string.Join(",", dec)));
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

        for (int i = 0; i < _cards.Count; i++)
        {
            BaseCard card = _cards[i];
            if (card == null || !card.IsAlive || !card.IsOpen)
            {
                continue;
            }

            targetCards.Add(card);
        }

        return targetCards;
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

        int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[randomIndex];
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

    public void StartActionSequence(Action onComplete)
    {
        if (_actionSequenceCoroutine != null)
        {
            StopCoroutine(_actionSequenceCoroutine);
        }

        _actionSequenceCoroutine = StartCoroutine(ActionSequence(onComplete));
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

    private IEnumerator ActionSequence(Action onComplete)
    {
        ChangeState(EnemyState.Think);
        float thinkDuration = UnityEngine.Random.Range(1f, 2f);
        yield return new WaitForSeconds(thinkDuration);

        ChangeState(EnemyState.Act);

        BaseCard attacker = GetActionCard();
        BaseCard target = GetPlayerTargetCard();

        if (attacker != null && target != null)
        {
            bool attackSequenceComplete = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.BeginEnemyAttackSequence(attacker, target, delegate
                {
                    attackSequenceComplete = true;
                });
            }
            else
            {
                attacker.Attack(target);
                attackSequenceComplete = true;
            }

            while (!attackSequenceComplete)
            {
                yield return null;
            }
        }

        ChangeState(EnemyState.End);
        _actionSequenceCoroutine = null;
        ChangeState(EnemyState.Idle);
        onComplete?.Invoke();
    }

    private BaseCard GetActionCard()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            BaseCard card = _cards[i];
            if (card != null && card.IsAlive && card.IsOpen)
            {
                return card;
            }
        }

        return null;
    }

    private BaseCard GetPlayerTargetCard()
    {
        if (PlayerController.Instance == null)
        {
            return null;
        }

        List<BaseCard> playerCards = PlayerController.Instance.Cards;
        for (int i = 0; i < playerCards.Count; i++)
        {
            BaseCard card = playerCards[i];
            if (card != null && card.IsAlive && card.IsOpen)
            {
                return card;
            }
        }

        return null;
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

    private void OnDestroy()
    {
        if (_openCardsCoroutine != null)
        {
            StopCoroutine(_openCardsCoroutine);
            _openCardsCoroutine = null;
        }

        if (_actionSequenceCoroutine != null)
        {
            StopCoroutine(_actionSequenceCoroutine);
            _actionSequenceCoroutine = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
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

    public bool HasActiveBattlefieldCardSequences()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            BaseCard card = _cards[i];
            if (card != null && card.IsSequencePlaying)
            {
                return true;
            }
        }

        return false;
    }
}
