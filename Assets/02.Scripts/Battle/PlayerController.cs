using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private const int InitialOpenCardCount = 3;

    public static PlayerController Instance { get; private set; }

    [SerializeField] private List<Transform> _cardParents = new List<Transform>(6);

    private List<int> _currentDec = new List<int>();
    private List<BaseCard> _cards = new List<BaseCard>();
    private Coroutine _openCardsCoroutine;

    public List<int> CurrentDec => _currentDec;
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
        _currentDec = new List<int>(UserInfoManager.Instance.CurrentDec);
        _cards.Clear();

        for (int i = 0; i < _currentDec.Count; i++)
        {
            BaseCard card = CreateCard(_currentDec[i], i);
            if (card != null)
            {
                _cards.Add(card);
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
        int openCardCount = Mathf.Min(InitialOpenCardCount, _cards.Count);

        for (int i = 0; i < openCardCount; i++)
        {
            _cards[i].FlipToFront();
            yield return new WaitForSeconds(_cards[i].FlipDuration);
        }

        _openCardsCoroutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
