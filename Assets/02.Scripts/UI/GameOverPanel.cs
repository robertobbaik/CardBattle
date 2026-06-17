using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private Image _victory;
    [SerializeField] private Image _draw;
    [SerializeField] private Image _defeat;

    public void ShowVictory()
    {
        Show(_victory);
    }

    public void ShowDraw()
    {
        Show(_draw);
    }

    public void ShowDefeat()
    {
        Show(_defeat);
    }

    public void Hide()
    {
        SetImageActive(_victory, false);
        SetImageActive(_draw, false);
        SetImageActive(_defeat, false);
        gameObject.SetActive(false);
    }

    private void Show(Image activeImage)
    {
        SetImageActive(_victory, activeImage == _victory);
        SetImageActive(_draw, activeImage == _draw);
        SetImageActive(_defeat, activeImage == _defeat);
        gameObject.SetActive(true);
    }

    private static void SetImageActive(Image image, bool isActive)
    {
        if (image != null)
        {
            image.gameObject.SetActive(isActive);
        }
    }
}
