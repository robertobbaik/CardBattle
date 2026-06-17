using UnityEngine;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private bool _isInitialized;

    [SerializeField]
    [SerializedDictionary(GlobalString.LobbyScreenTypeLabel, GlobalString.LobbyScreenLabel)]
    private SerializedDictionary<LobbyScreenType, BaseScreen> _screens;

    [SerializeField]
    [SerializedDictionary(GlobalString.LobbyScreenTypeLabel, GlobalString.LobbyButtonLabel)]
    private SerializedDictionary<LobbyScreenType, Button> _screenButtons;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Initialize();
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
        if (_isInitialized)
        {
            return;
        }

        foreach (var screenButton in _screenButtons)
        {
            LobbyScreenType screenType = screenButton.Key;
            Button button = screenButton.Value;

            button.onClick.AddListener(() => OnClickScreen(screenType));
        }

        foreach (var screen in _screens)
        {
            screen.Value.gameObject.SetActive(true);
            screen.Value.Initialize();
        }

        OnClickScreen(LobbyScreenType.Main);
        _isInitialized = true;
    }

    public void OnClickScreen(LobbyScreenType screenType)
    {
        foreach (var screen in _screens)
        {
            if (screen.Key == screenType)
            {
                screen.Value.Show();
                continue;
            }

            screen.Value.Hide();
        }
    }
}
