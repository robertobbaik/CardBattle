using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static EnemyController Instance { get; private set; }

    private EnemyState _currentState = EnemyState.None;

    public EnemyState CurrentState => _currentState;

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
        ChangeState(EnemyState.Idle);
    }

    public void ChangeState(EnemyState nextState)
    {
        if (_currentState == nextState)
        {
            return;
        }

        _currentState = nextState;
        EnterState(nextState);
    }

    private void Update()
    {
        TickState(_currentState);
    }

    private void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Think:
                break;
            case EnemyState.Act:
                break;
            case EnemyState.End:
                break;
        }
    }

    private void TickState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Think:
                break;
            case EnemyState.Act:
                break;
            case EnemyState.End:
                break;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
