using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public abstract class BaseCard : MonoBehaviour
{
    public int cardId;
    public CardType cardType;
    public int cardTextKey;
    public string cardName;
    public string cardDescription;
    public int maxHp;
    public int startHp;
    public List<float> uniqueValue = new List<float>();

    [SerializeField] private GameObject _back;
    [SerializeField] private GameObject _front;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private float _flipDuration = 0.4f;

    public GameObject Back => _back;
    public GameObject Front => _front;
    public Image IconImage => _iconImage;
    public TextMeshProUGUI NameText => _nameText;
    public TextMeshProUGUI DescriptionText => _descriptionText;
    public TextMeshProUGUI HpText => _hpText;
    public float FlipDuration
    {
        get => _flipDuration;
        set => _flipDuration = value;
    }

    private int _currentHp;
    private Sequence _flipSequence;

    public int Hp => _currentHp;
    public int AttackPower => _currentHp;

    public abstract CardType CardType { get; }

    public abstract void Attack(BaseCard target);

    public abstract void Destroy();

    public void Initialize()
    {
        CardData cardData = DataManager.Instance.CardDataById[cardId];
        cardType = (CardType)cardData.cardType;
        cardTextKey = cardData.cardTextKey;
        cardName = DataManager.Instance.GetCardName(cardTextKey);
        cardDescription = DataManager.Instance.GetCardDescription(cardTextKey);
        maxHp = cardData.maxHp;
        startHp = cardData.startHp;
        uniqueValue = new List<float>(cardData.uniqueValue);

        _currentHp = startHp;
        _iconImage.sprite = DataManager.Instance.GetCardIcon(cardType);
        _nameText.text = cardName;
        _descriptionText.text = cardDescription;
        _hpText.text = _currentHp.ToString();
        SetBackState();
    }

    public void FlipToFront()
    {
        _flipSequence?.Kill();

        transform.localScale = Vector3.one;
        _back.SetActive(true);
        _front.SetActive(false);

        float halfDuration = _flipDuration * 0.5f;
        _flipSequence = DOTween.Sequence();
        _flipSequence.Append(transform.DOScaleX(0f, halfDuration));
        _flipSequence.AppendCallback(SwitchToFront);
        _flipSequence.Append(transform.DOScaleX(1f, halfDuration));
    }

    public void SetBackState()
    {
        _flipSequence?.Kill();

        transform.localScale = Vector3.one;
        _back.SetActive(true);
        _front.SetActive(false);
    }

    private void SwitchToFront()
    {
        _back.SetActive(false);
        _front.SetActive(true);
    }

    public void TakeDamage(int damage)
    {
        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            Destroy();
        }
    }

    protected virtual void OnDestroy()
    {
        _flipSequence?.Kill();
    }
}
