using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private Dictionary<int, CardData> _cardDataById;
    private Dictionary<int, CardTextData> _cardTextDataByKey;
    private Dictionary<CardType, Sprite> _cardIconByType;

    public Dictionary<int, CardData> CardDataById => _cardDataById;
    public Dictionary<int, CardTextData> CardTextDataByKey => _cardTextDataByKey;
    public Dictionary<CardType, Sprite> CardIconByType => _cardIconByType;
    public string LanguageCode { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LanguageCode = Application.systemLanguage == SystemLanguage.Korean ? GlobalString.LanguageKo : GlobalString.LanguageEn;
        LoadData();
    }

    private void LoadData()
    {
        LoadCardData();
        LoadCardTextData();
        LoadCardIcons();
    }

    private void LoadCardData()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(GlobalString.ResourceCardDataPath);
        CardData.Table table = JsonUtility.FromJson<CardData.Table>(textAsset.text);
        _cardDataById = table.ToDictionary();
    }

    private void LoadCardTextData()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(GlobalString.ResourceCardTextDataPath);
        CardTextData.Table table = JsonUtility.FromJson<CardTextData.Table>(textAsset.text);
        _cardTextDataByKey = table.ToDictionary();
    }

    public CardTextData GetCardTextData(int cardTextKey)
    {
        return _cardTextDataByKey[cardTextKey];
    }

    public string GetCardName(int cardTextKey)
    {
        return GetCardTextData(cardTextKey).GetName(LanguageCode);
    }

    public string GetCardDescription(int cardTextKey)
    {
        return GetCardTextData(cardTextKey).GetDescription(LanguageCode);
    }

    private void LoadCardIcons()
    {
        _cardIconByType = new Dictionary<CardType, Sprite>();

        foreach (CardData cardData in _cardDataById.Values)
        {
            CardType cardType = (CardType)cardData.cardType;
            if (_cardIconByType.ContainsKey(cardType))
            {
                continue;
            }

            string resourceName = string.Concat(cardType.ToString().ToLowerInvariant(), GlobalString.CardIconFileSuffix);
            string resourcePath = string.Concat(GlobalString.ResourceCardIconsPath, GlobalString.Slash, resourceName);
            Sprite icon = Resources.Load<Sprite>(resourcePath);

            if (icon == null)
            {
                Debug.LogError(string.Format(GlobalString.ResourceNotFoundMessage, resourcePath));
                continue;
            }

            _cardIconByType.Add(cardType, icon);
        }
    }

    public Sprite GetCardIcon(CardType cardType)
    {
        return _cardIconByType[cardType];
    }
}
