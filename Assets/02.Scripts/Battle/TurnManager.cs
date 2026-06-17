using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private GameObject _turnPanel;
    [SerializeField] private TextMeshProUGUI _turnText;

    private TurnOwner _currentTurnOwner = TurnOwner.None;
    private int _turnCount;
    private Coroutine _turnTransitionCoroutine;
    private bool _isTransitioningTurn;

    public TurnOwner CurrentTurnOwner => _currentTurnOwner;
    public int TurnCount => _turnCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Initialize()
    {
        if (_turnTransitionCoroutine != null)
        {
            StopCoroutine(_turnTransitionCoroutine);
            _turnTransitionCoroutine = null;
        }

        _currentTurnOwner = TurnOwner.None;
        _turnCount = 0;
        _isTransitioningTurn = false;
        SetTurnPanelActive(false);
        Debug.Log("Turn Initialize");
    }

    public void StartPlayerTurn()
    {
        _currentTurnOwner = TurnOwner.Player;
        _turnCount++;
        Debug.Log(string.Format("Turn Start - Player, TurnCount: {0}", _turnCount));

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnTurnStart();
        }
    }

    public void StartEnemyTurn()
    {
        _currentTurnOwner = TurnOwner.Enemy;
        Debug.Log("Turn Start - Enemy");

        if (EnemyController.Instance != null)
        {
            EnemyController.Instance.OnTurnStart();
        }
    }

    public void EndTurn(Action onTurnChangedComplete = null)
    {
        Debug.Log(string.Format("Turn End - Current: {0}", _currentTurnOwner));

        if (_isTransitioningTurn)
        {
            Debug.Log("Turn Transition - skipped because transition is already running");
            return;
        }

        if (_turnTransitionCoroutine != null)
        {
            StopCoroutine(_turnTransitionCoroutine);
        }

        _turnTransitionCoroutine = StartCoroutine(TurnTransitionCoroutine(onTurnChangedComplete));
    }

    private IEnumerator TurnTransitionCoroutine(Action onTurnChangedComplete)
    {
        _isTransitioningTurn = true;

        TurnOwner nextTurnOwner = TurnOwner.None;
        if (_currentTurnOwner == TurnOwner.Player)
        {
            nextTurnOwner = TurnOwner.Enemy;
        }
        else if (_currentTurnOwner == TurnOwner.Enemy)
        {
            nextTurnOwner = TurnOwner.Player;
        }

        Debug.Log(string.Format("Turn Transition Start - Current: {0}, Next: {1}", _currentTurnOwner, nextTurnOwner));
        ShowTurnPanel(nextTurnOwner);
        Debug.Log("Turn Transition Wait - 1s");
        yield return new WaitForSeconds(1f);
        SetTurnPanelActive(false);
        Debug.Log(string.Format("Turn Transition End - Next: {0}", nextTurnOwner));

        if (nextTurnOwner == TurnOwner.Enemy)
        {
            StartEnemyTurn();
        }
        else if (nextTurnOwner == TurnOwner.Player)
        {
            StartPlayerTurn();
        }

        _isTransitioningTurn = false;
        _turnTransitionCoroutine = null;
        onTurnChangedComplete?.Invoke();
    }

    private void ShowTurnPanel(TurnOwner turnOwner)
    {
        if (_turnText != null)
        {
            _turnText.text = GetTurnText(turnOwner);
        }

        SetTurnPanelActive(true);
    }

    private void SetTurnPanelActive(bool isActive)
    {
        if (_turnPanel == null)
        {
            return;
        }

        _turnPanel.SetActive(isActive);
    }

    private static string GetTurnText(TurnOwner turnOwner)
    {
        if (turnOwner == TurnOwner.Player)
        {
            return "플레이어 턴";
        }

        if (turnOwner == TurnOwner.Enemy)
        {
            return "적 플레이어 턴";
        }

        return string.Empty;
    }
}
