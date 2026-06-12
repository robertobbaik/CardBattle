using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

public sealed class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField]
    [SerializedDictionary("Screen Type", "Screen")]
    private SerializedDictionary<LobbyScreenType, BaseScreen> _screens;

    [SerializeField]
    [SerializedDictionary("Screen Type", "Button")]
    private SerializedDictionary<LobbyScreenType, Button> _screenButtons;

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

    public void OnClickScreen(LobbyScreenType screenType)
    {
        if (_screens == null)
        {
            return;
        }

        foreach (var screen in _screens)
        {
            if (screen.Value == null)
            {
                continue;
            }

            if (screen.Key == screenType)
            {
                screen.Value.Show();
                continue;
            }

            screen.Value.Hide();
        }
    }
}
