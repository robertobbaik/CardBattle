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

    public void EndTurn()
    {
        Debug.Log(string.Format("Turn End - Current: {0}", _currentTurnOwner));

        if (_currentTurnOwner == TurnOwner.Player)
        {
            StartEnemyTurn();
            return;
        }

        StartPlayerTurn();
    }
}
