using UnityEngine;
using System.Collections.Generic;

public class UserInfoManager : MonoBehaviour
{
    public static UserInfoManager Instance { get; private set; }

    [SerializeField] private List<int> _currentDec = new List<int>();

    public List<int> CurrentDec => _currentDec;
    public List<int> Dec => _currentDec;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadCurrentDec();
    }

    private void LoadCurrentDec()
    {
        string currentDecJson = PlayerPrefs.GetString(GlobalString.CurrentDecKey);

        if (string.IsNullOrEmpty(currentDecJson))
        {
            _currentDec = new List<int> { 1, 2, 3, 4, 5, 6 };
            return;
        }

        CurrentDecData currentDecData = JsonUtility.FromJson<CurrentDecData>(currentDecJson);
        _currentDec = currentDecData.cardIds;
    }

    public void SetCurrentDecCard(int index, int cardId)
    {
        if (index < 0 || index >= _currentDec.Count)
        {
            return;
        }

        int duplicateIndex = GetCurrentDecIndex(cardId);
        if (duplicateIndex >= 0)
        {
            int currentCardId = _currentDec[index];
            _currentDec[index] = cardId;
            _currentDec[duplicateIndex] = currentCardId;
            SaveCurrentDec();
            return;
        }

        _currentDec[index] = cardId;
        SaveCurrentDec();
    }

    private int GetCurrentDecIndex(int cardId)
    {
        for (int i = 0; i < _currentDec.Count; i++)
        {
            if (_currentDec[i] == cardId)
            {
                return i;
            }
        }

        return -1;
    }

    private void SaveCurrentDec()
    {
        CurrentDecData currentDecData = new CurrentDecData();
        currentDecData.cardIds = new List<int>(_currentDec);
        string currentDecJson = JsonUtility.ToJson(currentDecData);
        PlayerPrefs.SetString(GlobalString.CurrentDecKey, currentDecJson);
        PlayerPrefs.Save();
    }

    private class CurrentDecData
    {
        public List<int> cardIds = new List<int>();
    }
}
