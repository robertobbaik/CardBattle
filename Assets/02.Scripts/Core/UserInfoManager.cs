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

    private class CurrentDecData
    {
        public List<int> cardIds = new List<int>();
    }
}
