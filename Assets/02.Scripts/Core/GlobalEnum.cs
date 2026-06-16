public enum SceneType
{
    LoadingScene = 0,
    LobbyScene = 1,
    GameLoading = 2,
    GameScene = 3
}

public enum LobbyScreenType
{
    Main,
    Shop,
    Inventory,
    Collection
}

public enum CardType
{
    Normal = 1,
    Ranged = 2,
    Peerless = 3,
    Healer = 4,
    Guardian = 5,
    Assassin = 6,
    Berserker = 7,
    Shaman = 8,
    Commander = 9,
    Bomber = 10
}

public enum TurnOwner
{
    None,
    Player,
    Enemy
}

public enum CardOwner
{
    None,
    Player,
    Enemy
}

public enum EnemyState
{
    None,
    Idle,
    Think,
    Act,
    End
}
