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
    [SerializeField] private bool _logAiDecision;

    private EnemyState _currentState = EnemyState.None;
    private List<int> _currentDec = new List<int>(EnemyDecCardCount);
    private List<BaseCard> _cards = new List<BaseCard>(EnemyDecCardCount);
    private Queue<BaseCard> _waitingCardQueue = new Queue<BaseCard>(EnemyDecCardCount);
    private Coroutine _openCardsCoroutine;
    private Coroutine _actionSequenceCoroutine;
    private int _guardDamageReduction;

    private struct EnemyActionPlan
    {
        public BaseCard Attacker;
        public BaseCard Target;
        public int Score;
        public bool IsValid;
    }

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

    public void CreateCards(Action onComplete, bool suppressOpponentRevealDebuffs = false)
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

        StartOpenCards(onComplete, suppressOpponentRevealDebuffs);
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

        EnemyActionPlan actionPlan = GetBestActionPlan();
        BaseCard attacker = actionPlan.IsValid ? actionPlan.Attacker : null;
        BaseCard target = actionPlan.IsValid ? actionPlan.Target : null;

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

    private EnemyActionPlan GetBestActionPlan()
    {
        if (PlayerController.Instance == null)
        {
            return default;
        }

        List<BaseCard> playerCards = PlayerController.Instance.Cards;
        EnemyActionPlan selectedPlan = default;
        int bestScore = int.MinValue;
        int bestScoreTieCount = 0;

        for (int attackerIndex = 0; attackerIndex < _cards.Count; attackerIndex++)
        {
            BaseCard attacker = _cards[attackerIndex];
            if (attacker == null || !attacker.IsAlive || !attacker.IsOpen || !attacker.CanAttack)
            {
                continue;
            }

            for (int targetIndex = 0; targetIndex < playerCards.Count; targetIndex++)
            {
                BaseCard target = playerCards[targetIndex];
                if (target == null || !target.IsAlive || !target.IsOpen)
                {
                    continue;
                }

                int score = EvaluateActionScore(attacker, target);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestScoreTieCount = 1;
                    selectedPlan = CreateActionPlan(attacker, target, score);
                    continue;
                }

                if (score == bestScore)
                {
                    bestScoreTieCount++;
                    if (UnityEngine.Random.Range(0, bestScoreTieCount) == 0)
                    {
                        selectedPlan = CreateActionPlan(attacker, target, score);
                    }
                }
            }
        }

        if (selectedPlan.IsValid && _logAiDecision)
        {
            Debug.Log(string.Format("Enemy AI Action - attacker: {0}, target: {1}, score: {2}", GetCardLabel(selectedPlan.Attacker), GetCardLabel(selectedPlan.Target), selectedPlan.Score));
        }

        return selectedPlan;
    }

    private EnemyActionPlan CreateActionPlan(BaseCard attacker, BaseCard target, int score)
    {
        return new EnemyActionPlan
        {
            Attacker = attacker,
            Target = target,
            Score = score,
            IsValid = true
        };
    }

    private int EvaluateActionScore(BaseCard attacker, BaseCard target)
    {
        int attackDamage = EstimateAttackDamage(attacker);
        int reflectedDamage = EstimateReflectedDamage(attacker, target, attackDamage);
        int selfDamage = EstimateSelfDamage(attacker);
        int targetPriority = GetTargetPriority(target);
        int attackerPriority = GetAttackerPriority(attacker);
        int splashValue = EstimateSplashValue(attacker, target, attackDamage);
        bool canKillTarget = attackDamage >= target.Hp;
        bool attackerLikelyDies = attacker.Hp <= reflectedDamage + selfDamage;

        int score = 0;
        score += canKillTarget ? 10000 : 0;
        score += targetPriority * 100;
        score += attackerPriority * 100;
        score += Mathf.Max(0, 100 - target.Hp) * 50;
        score += splashValue * 35;
        score -= reflectedDamage * 80;
        score -= selfDamage * 50;
        score -= attackerLikelyDies ? 2500 : 0;
        score += Mathf.Max(0, attackDamage - target.Hp) * 20;

        return score;
    }

    private int EstimateAttackDamage(BaseCard attacker)
    {
        if (attacker == null)
        {
            return 0;
        }

        switch (attacker.CardType)
        {
            case CardType.Guardian:
                return Mathf.Max(1, Mathf.FloorToInt(attacker.Hp * 0.8f));
            case CardType.Berserker:
                return Mathf.Max(1, Mathf.FloorToInt(attacker.Hp * 1.2f));
            case CardType.Bomber:
                return Mathf.Max(1, Mathf.FloorToInt(attacker.Hp * 0.5f));
            default:
                return Mathf.Max(1, attacker.Hp);
        }
    }

    private int EstimateReflectedDamage(BaseCard attacker, BaseCard target, int attackDamage)
    {
        if (attacker == null || target == null)
        {
            return 0;
        }

        if (attacker.CardType == CardType.Ranged || attacker.CardType == CardType.Bomber)
        {
            return 0;
        }

        if (target.Hp - attackDamage <= 0 && attacker.CardType == CardType.Assassin)
        {
            return 0;
        }

        switch (attacker.CardType)
        {
            case CardType.Assassin:
                return Mathf.FloorToInt(target.Hp * 1.5f);
            case CardType.Guardian:
            case CardType.Peerless:
                return Mathf.FloorToInt(target.Hp * 1.2f);
            default:
                return target.Hp;
        }
    }

    private int EstimateSelfDamage(BaseCard attacker)
    {
        if (attacker == null || attacker.CardType != CardType.Berserker)
        {
            return 0;
        }

        return Mathf.Max(1, Mathf.FloorToInt(attacker.MaxHp * 0.2f));
    }

    private int GetTargetPriority(BaseCard target)
    {
        if (target == null)
        {
            return 0;
        }

        switch (target.CardType)
        {
            case CardType.Healer:
            case CardType.Commander:
            case CardType.Shaman:
                return 5;
            case CardType.Assassin:
            case CardType.Peerless:
            case CardType.Bomber:
                return 4;
            case CardType.Berserker:
            case CardType.Ranged:
                return 3;
            case CardType.Guardian:
                return 2;
            default:
                return 1;
        }
    }

    private int GetAttackerPriority(BaseCard attacker)
    {
        if (attacker == null)
        {
            return 0;
        }

        switch (attacker.CardType)
        {
            case CardType.Ranged:
                return 6;
            case CardType.Bomber:
                return 4;
            case CardType.Peerless:
                return 3;
            default:
                return 0;
        }
    }

    private int EstimateSplashValue(BaseCard attacker, BaseCard target, int attackDamage)
    {
        if (attacker == null || target == null || PlayerController.Instance == null)
        {
            return 0;
        }

        if (attacker.CardType == CardType.Bomber)
        {
            int splashDamage = Mathf.Max(1, Mathf.FloorToInt(attacker.Hp * 0.3f));
            return CountDamagedCards(PlayerController.Instance.Cards, target, splashDamage) * splashDamage;
        }

        if (attacker.CardType == CardType.Peerless)
        {
            int splashDamage = Mathf.FloorToInt(attackDamage * 0.5f);
            if (splashDamage <= 0)
            {
                return 0;
            }

            return CountAdjacentAliveCards(PlayerController.Instance.Cards, target.SlotIndex) * splashDamage;
        }

        return 0;
    }

    private int CountDamagedCards(List<BaseCard> cards, BaseCard excludedCard, int damage)
    {
        if (cards == null || damage <= 0)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            BaseCard card = cards[i];
            if (card == null || card == excludedCard || !card.IsAlive || !card.IsOpen)
            {
                continue;
            }

            count++;
        }

        return count;
    }

    private int CountAdjacentAliveCards(List<BaseCard> cards, int slotIndex)
    {
        if (cards == null)
        {
            return 0;
        }

        int count = 0;
        if (IsAliveOpenCardAt(cards, slotIndex - 1))
        {
            count++;
        }

        if (IsAliveOpenCardAt(cards, slotIndex + 1))
        {
            count++;
        }

        return count;
    }

    private bool IsAliveOpenCardAt(List<BaseCard> cards, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cards.Count)
        {
            return false;
        }

        BaseCard card = cards[slotIndex];
        return card != null && card.IsAlive && card.IsOpen;
    }

    private string GetCardLabel(BaseCard card)
    {
        if (card == null)
        {
            return "None";
        }

        return string.Format("{0}[Slot:{1}, HP:{2}, Type:{3}]", string.IsNullOrEmpty(card.cardName) ? card.name : card.cardName, card.SlotIndex, card.Hp, card.CardType);
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

    private void StartOpenCards(Action onComplete, bool suppressOpponentRevealDebuffs)
    {
        if (_openCardsCoroutine != null)
        {
            StopCoroutine(_openCardsCoroutine);
        }

        _openCardsCoroutine = StartCoroutine(OpenCards(onComplete, suppressOpponentRevealDebuffs));
    }

    private IEnumerator OpenCards(Action onComplete, bool suppressOpponentRevealDebuffs)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetOpponentRevealDebuffsSuppressed(CardOwner.Enemy, suppressOpponentRevealDebuffs);
            GameManager.Instance.BeginInitialRevealEffectQueue();
        }

        for (int i = 0; i < InitialOpenCardCount; i++)
        {
            BaseCard nextCard = _waitingCardQueue.Dequeue();
            nextCard.FlipToFront();
            yield return new WaitForSeconds(nextCard.FlipDuration);
        }

        if (GameManager.Instance != null)
        {
            yield return GameManager.Instance.FlushInitialRevealEffectQueue();
            GameManager.Instance.SetOpponentRevealDebuffsSuppressed(CardOwner.Enemy, false);
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
