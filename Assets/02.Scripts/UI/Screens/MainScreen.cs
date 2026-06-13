using UnityEngine;
using UnityEngine.UI;

public class MainScreen : BaseScreen
{
    [SerializeField] private Button _gameStartButton;

    private bool _isGameStartRequested;

    private void Awake()
    {
        _gameStartButton.onClick.AddListener(OnClickGameStart);
    }

    private void OnDestroy()
    {
        _gameStartButton.onClick.RemoveListener(OnClickGameStart);
    }

    private void OnClickGameStart()
    {
        if (_isGameStartRequested)
        {
            return;
        }

        _isGameStartRequested = true;
        _gameStartButton.interactable = false;

        SceneManager.Instance.LoadScene(SceneType.Game);
    }
}
