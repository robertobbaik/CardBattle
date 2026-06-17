using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardItem : MonoBehaviour
{
    [SerializeField] private int _order;
    [SerializeField] private int _cardIndex;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private GameObject _selectedFrame;
    [SerializeField] private TextMeshProUGUI _orderText;

    public int Order => _order;
    public int CardIndex => _cardIndex;
    public bool IsSelected => _order > 0;

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        _button.onClick.AddListener(OnClick);
    }

    public void Initialize(CardData cardData)
    {
        _nameText.text = DataManager.Instance.GetCardName(cardData.cardTextKey);
        _descriptionText.text = DataManager.Instance.GetCardDescription(cardData.cardTextKey);
        _hpText.text = string.Format("{0}/{1}", cardData.startHp, cardData.startHp);
        _iconImage.sprite = DataManager.Instance.GetCardIcon(cardData.cardId);
        SetOrder(0);
    }

    public void Initialize(CardData cardData, int cardIndex)
    {
        _cardIndex = cardIndex;
        Initialize(cardData);
    }

    public void SetOrder(int order)
    {
        _order = order;
        _orderText.text = _order > 0 ? _order.ToString() : GlobalString.Empty;
        SetSelected(_order > 0);
    }

    public void SetSelected(bool isSelected)
    {
        _selectedFrame.SetActive(isSelected);
    }

    private void OnClick()
    {
        if (CardSelectPanel.Instance != null)
        {
            CardSelectPanel.Instance.OnClickCardItem(this);
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }
}
