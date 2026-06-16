using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    [SerializeField] private Button _attackButton;
    [SerializeField] private Button _cancelButton;

    private void Awake()
    {
        if (_attackButton != null)
        {
            _attackButton.onClick.AddListener(OnClickAttack);
        }

        if (_cancelButton == null)
        {
            return;
        }

        _cancelButton.onClick.AddListener(OnClickCancel);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnClickAttack()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.BeginAttackSelection();
    }

    private void OnClickCancel()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.CancelActionFocus();
    }

    private void OnDestroy()
    {
        if (_attackButton != null)
        {
            _attackButton.onClick.RemoveListener(OnClickAttack);
        }

        if (_cancelButton == null)
        {
            return;
        }

        _cancelButton.onClick.RemoveListener(OnClickCancel);
    }
}
