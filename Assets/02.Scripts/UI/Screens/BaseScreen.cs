using UnityEngine;

public abstract class BaseScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;

    public bool IsVisible => _canvasGroup.alpha > 0f;

    public abstract void Initialize();

    public virtual void Show()
    {
        SetVisible(1f, true);
    }

    public virtual void Hide()
    {
        SetVisible(0f, false);
    }

    protected void SetVisible(float alpha, bool canInteract)
    {
        _canvasGroup.alpha = alpha;
        _canvasGroup.interactable = canInteract;
        _canvasGroup.blocksRaycasts = canInteract;
    }
}
