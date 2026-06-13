using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    private TurnOwner _currentTurnOwner = TurnOwner.None;
    private int _turnCount;

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
        _currentTurnOwner = TurnOwner.None;
        _turnCount = 0;
    }

    public void StartPlayerTurn()
    {
        _currentTurnOwner = TurnOwner.Player;
        _turnCount++;
    }

    public void StartEnemyTurn()
    {
        _currentTurnOwner = TurnOwner.Enemy;
    }

    public void EndTurn()
    {
        if (_currentTurnOwner == TurnOwner.Player)
        {
            StartEnemyTurn();
            return;
        }

        StartPlayerTurn();
    }
}
