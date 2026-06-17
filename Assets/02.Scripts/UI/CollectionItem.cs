using System;
using UnityEngine;
using UnityEngine.UI;

public class CollectionItem : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;

    private CardData _cardData;
    private CardType _cardType;
    private Action<CollectionItem> _clickCallback;

    public Button Button => _button;
    public Image Icon => _icon;
    public CardData CardData => _cardData;
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
    }

    public void SetClickCallback(Action<CollectionItem> clickCallback)
    {
        _clickCallback = clickCallback;
    }

    private void OnClick()
    {
        _clickCallback?.Invoke(this);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }
}
