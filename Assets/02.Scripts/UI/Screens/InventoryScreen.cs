using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScreen : BaseScreen
{
    [SerializeField] private List<InventoryItem> _currentDec = new List<InventoryItem>();
    [SerializeField] private List<CollectionItem> _collectionItems = new List<CollectionItem>();
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _cancelButton;

    private CardData _selectedCardData;
    private bool _isEquipMode;

    private void Awake()
    {
        _equipButton.onClick.AddListener(OnClickEquip);
        _cancelButton.onClick.AddListener(OnClickCancel);
    }

    public override void Initialize()
    {
        InitializeCurrentDec();
        InitializeCollectionItems();
        InitializeDefaultCardInfo();
        SetEquipMode(false);
    }

    public override void Show()
    {
        InitializeCurrentDec();
        SetEquipMode(false);
        base.Show();
    }

    private void InitializeCurrentDec()
    {
        List<int> currentDec = UserInfoManager.Instance.CurrentDec;
        for (int i = 0; i < _currentDec.Count; i++)
        {
            InventoryItem item = _currentDec[i];
            if (item == null)
            {
                continue;
            }

            if (i >= currentDec.Count)
            {
                item.gameObject.SetActive(false);
                continue;
            }

            CardData cardData = DataManager.Instance.CardDataById[currentDec[i]];
            item.gameObject.SetActive(true);
            item.Initialize(cardData);
            item.SetClickCallback(OnClickInventoryItem);
        }
    }

    private void InitializeCollectionItems()
    {
        List<CardData> cardDataList = GetSortedCardDataList();
        for (int i = 0; i < _collectionItems.Count; i++)
        {
            CollectionItem item = _collectionItems[i];
            if (item == null)
            {
                continue;
            }

            if (i >= cardDataList.Count)
            {
                item.gameObject.SetActive(false);
                continue;
            }

            item.gameObject.SetActive(true);
            item.Initialize(cardDataList[i]);
            item.SetClickCallback(OnClickCollectionItem);
        }
    }

    private void InitializeDefaultCardInfo()
    {
        List<CardData> cardDataList = GetSortedCardDataList();
        if (cardDataList.Count == 0)
        {
            return;
        }

        SetCardInfo(cardDataList[0]);
    }

    private void OnClickCollectionItem(CollectionItem collectionItem)
    {
        SetCardInfo(collectionItem.CardData);
    }

    private void OnClickInventoryItem(InventoryItem inventoryItem)
    {
        if (!_isEquipMode)
        {
            SetCardInfo(inventoryItem.CardData);
            return;
        }

        int index = _currentDec.IndexOf(inventoryItem);
        if (index < 0 || _selectedCardData == null)
        {
            return;
        }

        UserInfoManager.Instance.SetCurrentDecCard(index, _selectedCardData.cardId);
        InitializeCurrentDec();
        SetEquipMode(false);
    }

    private void OnClickEquip()
    {
        SetEquipMode(true);
    }

    private void OnClickCancel()
    {
        SetEquipMode(false);
    }

    private void SetCardInfo(CardData cardData)
    {
        _selectedCardData = cardData;
        _iconImage.sprite = DataManager.Instance.GetCardIcon(cardData.cardId);
        _nameText.text = DataManager.Instance.GetCardName(cardData.cardTextKey);
        _descriptionText.text = DataManager.Instance.GetCardLogicDescription(cardData.cardTextKey);
    }

    private void SetEquipMode(bool isEquipMode)
    {
        _isEquipMode = isEquipMode;

        for (int i = 0; i < _currentDec.Count; i++)
        {
            InventoryItem item = _currentDec[i];
            if (item == null)
            {
                continue;
            }

            item.SetPointerActive(isEquipMode);
        }
    }

    private List<CardData> GetSortedCardDataList()
    {
        return DataManager.Instance.CardDataById.Values.OrderBy(cardData => cardData.cardId).ToList();
    }

    private void OnDestroy()
    {
        _equipButton.onClick.RemoveListener(OnClickEquip);
        _cancelButton.onClick.RemoveListener(OnClickCancel);
    }
}
