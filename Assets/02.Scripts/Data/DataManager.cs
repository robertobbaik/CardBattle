using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private Dictionary<int, CardData> _cardDataById;

    public Dictionary<int, CardData> CardDataById => _cardDataById;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    private void LoadData()
    {
        LoadCardData();
    }

    private void LoadCardData()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Data/CardData");
        CardData.Table table = JsonUtility.FromJson<CardData.Table>(textAsset.text);
        _cardDataById = table.ToDictionary();
    }
}
