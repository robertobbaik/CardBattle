using System;
using System.Collections.Generic;

[Serializable]
public class CardData
{
    public int cardId;
    public int cardType;
    public int cardTextKey;
    public int maxHp;
    public int startHp;
    public List<float> uniqueValue;

    [Serializable]
    public class Table
    {
        public List<CardData> items = new List<CardData>();

        public Dictionary<int, CardData> ToDictionary()
        {
            Dictionary<int, CardData> dataById = new Dictionary<int, CardData>(items.Count);

            foreach (CardData data in items)
            {
                dataById.Add(data.cardId, data);
            }

            return dataById;
        }
    }
}

[Serializable]
public class CardTextData
{
    public int cardTextKey;
    public string koreanName;
    public string koreanDescription;
    public string englishName;
    public string englishDescription;
    public string koreanLogicDescription;
    public string englishLogicDescription;

    [Serializable]
    public class Table
    {
        public List<CardTextData> items = new List<CardTextData>();

        public Dictionary<int, CardTextData> ToDictionary()
        {
            Dictionary<int, CardTextData> dataById = new Dictionary<int, CardTextData>(items.Count);

            foreach (CardTextData data in items)
            {
                dataById.Add(data.cardTextKey, data);
            }

            return dataById;
        }
    }

    public string GetName(string languageCode)
    {
        return NormalizeLanguageCode(languageCode) == GlobalString.LanguageKo ? koreanName : englishName;
    }

    public string GetDescription(string languageCode)
    {
        return NormalizeLanguageCode(languageCode) == GlobalString.LanguageKo ? koreanDescription : englishDescription;
    }

    public string GetLogicDescription(string languageCode)
    {
        return NormalizeLanguageCode(languageCode) == GlobalString.LanguageKo ? koreanLogicDescription : englishLogicDescription;
    }

    private static string NormalizeLanguageCode(string languageCode)
    {
        return string.Equals(languageCode, GlobalString.LanguageKo, StringComparison.OrdinalIgnoreCase) ? GlobalString.LanguageKo : GlobalString.LanguageEn;
    }
}
