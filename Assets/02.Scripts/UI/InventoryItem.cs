using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _pointer;

    private CardData _cardData;
    private CardType _cardType;
    private Action<InventoryItem> _clickCallback;
    private Sequence _pointerSequence;
    private Vector3 _pointerBaseLocalPosition;
    private bool _hasPointerBasePosition;

    public Button Button => _button;
    public Image Icon => _icon;
    public CardData CardData => _cardData;
    public Image Pointer => _pointer;
    public CardType CardType => _cardType;

    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
    }

    public void Initialize(CardData cardData)
    {
        _cardData = cardData;
        _cardType = (CardType)cardData.cardType;
        _icon.sprite = DataManager.Instance.GetCardIcon(cardData.cardId);
        HidePointer();
    }

    public void SetClickCallback(Action<InventoryItem> clickCallback)
    {
        _clickCallback = clickCallback;
    }

    public void ShowPointer()
    {
        InitializePointerBasePosition();
        _pointer.gameObject.SetActive(true);

        _pointerSequence?.Kill();
        _pointer.transform.localPosition = _pointerBaseLocalPosition;
        _pointerSequence = DOTween.Sequence();
        _pointerSequence.Append(_pointer.transform.DOLocalMoveY(_pointerBaseLocalPosition.y + 8f, 0.45f));
        _pointerSequence.Append(_pointer.transform.DOLocalMoveY(_pointerBaseLocalPosition.y, 0.45f));
        _pointerSequence.SetEase(Ease.InOutSine);
        _pointerSequence.SetLoops(-1);
    }

    public void HidePointer()
    {
        InitializePointerBasePosition();
        _pointerSequence?.Kill();
        _pointerSequence = null;
        _pointer.transform.localPosition = _pointerBaseLocalPosition;
        _pointer.gameObject.SetActive(false);
    }

    public void SetPointerActive(bool isActive)
    {
        if (isActive)
        {
            ShowPointer();
            return;
        }

        HidePointer();
    }

    private void OnClick()
    {
        _clickCallback?.Invoke(this);
    }

    private void OnDestroy()
    {
        _pointerSequence?.Kill();
        _button.onClick.RemoveListener(OnClick);
    }

    private void InitializePointerBasePosition()
    {
        if (_hasPointerBasePosition)
        {
            return;
        }

        _pointerBaseLocalPosition = _pointer.transform.localPosition;
        _hasPointerBasePosition = true;
    }
}
