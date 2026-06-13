using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private List<int> _currentDec = new List<int>();

    public List<int> CurrentDec => _currentDec;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _currentDec = new List<int>(UserInfoManager.Instance.CurrentDec);
    }
}
