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

    [SerializeField] private CardOwner _owner = CardOwner.None;
    [SerializeField] private GameObject _back;
    [SerializeField] private GameObject _front;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private float _flipDuration = 0.4f;

    private Button _button;
    private Image _highlightImage;
    private int _currentHp;
    private int _slotIndex = -1;
    private int _nextAttackModifier;
    private bool _hasActedThisTurn;
    private Sequence _flipSequence;
    private Sequence _damageShakeSequence;
    private Color _defaultHighlightColor = Color.white;

    public CardOwner Owner => _owner;
    public GameObject Back => _back;
    public GameObject Front => _front;
    public Image IconImage => _iconImage;
    public TextMeshProUGUI NameText => _nameText;
    public TextMeshProUGUI DescriptionText => _descriptionText;
    public TextMeshProUGUI HpText => _hpText;
    public GameObject Highlight => _highlight;
    public float FlipDuration
    {
        get => _flipDuration;
        set => _flipDuration = value;
    }

    public int Hp => _currentHp;
    public int AttackPower => _currentHp;
    public int SlotIndex => _slotIndex;
    public int MaxHp => maxHp;
    public bool HasActedThisTurn => _hasActedThisTurn;

    public virtual bool CanAttack => _currentHp > 0 && !_hasActedThisTurn;

    public virtual bool CanUseSkill => false;

    public abstract CardType CardType { get; }

    public abstract void Attack(BaseCard target);

    public abstract void Destroy();

    protected virtual void Awake()
    {
        _button = GetComponent<Button>();

        if (_highlight != null)
        {
            _highlightImage = _highlight.GetComponent<Image>();
            if (_highlightImage != null)
            {
                _defaultHighlightColor = _highlightImage.color;
            }
        }
    }

    protected virtual void OnEnable()
    {
        if (_button == null)
        {
            return;
        }

        _button.onClick.AddListener(OnClickCard);
    }

    protected virtual void OnDisable()
    {
        if (_button == null)
        {
            return;
        }

        _button.onClick.RemoveListener(OnClickCard);
    }

    public void SetOwner(CardOwner owner)
    {
        _owner = owner;
    }

    public void SetSlotIndex(int slotIndex)
    {
        _slotIndex = slotIndex;
    }

    public virtual void ResetTurnAction()
    {
        _hasActedThisTurn = false;
    }

    public void MarkAsActed()
    {
        _hasActedThisTurn = true;
    }

    public virtual void OnTurnStart()
    {
    }

    public virtual void UseSkill(BaseCard target = null)
    {
    }

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
        _nextAttackModifier = 0;
        _hasActedThisTurn = false;
        _iconImage.sprite = DataManager.Instance.GetCardIcon(cardType);
        _nameText.text = cardName;
        _descriptionText.text = cardDescription;
        _hpText.text = _currentHp.ToString();
        SetHighlightActive(false);
        SetBackState();
    }

    public virtual void ReflectAttack(BaseCard attacker)
    {
        if (attacker == null)
        {
            Debug.Log($"{name} reflect attack skipped. attacker is null.");
            return;
        }

        Debug.Log($"{name} reflect attack. counter owner: {_owner}, attacker owner: {attacker.Owner}");
        Attack(attacker);
    }

    protected int GetAttackDamage()
    {
        int attackDamage = _currentHp + _nextAttackModifier;
        _nextAttackModifier = 0;

        if (attackDamage < 1)
        {
            attackDamage = 1;
        }

        return attackDamage;
    }

    protected void AttackWithCounter(BaseCard target, int damage)
    {
        if (target == null)
        {
            return;
        }

        target.TakeDamage(damage, this);

        if (!ShouldReceiveCounter(target))
        {
            return;
        }

        int counterDamage = target.Hp;
        if (counterDamage <= 0)
        {
            return;
        }

        TakeDamage(counterDamage, target);
    }

    protected BaseCard GetRandomAdjacentEnemyCard(BaseCard target)
    {
        if (target == null)
        {
            return null;
        }

        if (target.Owner == CardOwner.Player)
        {
            if (PlayerController.Instance == null)
            {
                return null;
            }

            return PlayerController.Instance.GetRandomAdjacentAliveCard(target.SlotIndex);
        }

        if (target.Owner == CardOwner.Enemy)
        {
            if (EnemyController.Instance == null)
            {
                return null;
            }

            return EnemyController.Instance.GetRandomAdjacentAliveCard(target.SlotIndex);
        }

        return null;
    }

    protected virtual bool ShouldReceiveCounter(BaseCard target)
    {
        return true;
    }

    public void ApplyInspired()
    {
        _nextAttackModifier += 2;
    }

    public void ApplyWeaken()
    {
        _nextAttackModifier -= 2;
    }

    public void SetHighlightActive(bool isActive)
    {
        if (_highlight == null)
        {
            return;
        }

        _highlight.SetActive(isActive);
    }

    public void SetHighlightColor(Color color)
    {
        if (_highlightImage == null)
        {
            return;
        }

        _highlightImage.color = color;
    }

    public void ResetHighlightColor()
    {
        SetHighlightColor(_defaultHighlightColor);
    }

    public void ShowHighlight()
    {
        SetHighlightActive(true);
    }

    public void HideHighlight()
    {
        SetHighlightActive(false);
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

    public void TakeDamage(int damage, BaseCard source = null)
    {
        if (damage <= 0)
        {
            return;
        }

        damage -= GetIncomingDamageReduction();
        if (damage < 1)
        {
            damage = 1;
        }

        PlayDamageShake();
        PlayDamageFX();
        _currentHp -= damage;
        _hpText.text = _currentHp.ToString();

        if (_currentHp <= 0)
        {
            OnDeath(source);
            NotifyDestroyed();
            Destroy();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        PlayHealFX();
        _currentHp += amount;

        if (_currentHp > maxHp)
        {
            _currentHp = maxHp;
        }

        _hpText.text = _currentHp.ToString();
    }

    private void PlayDamageFX()
    {
        if (FXManager.Instance == null)
        {
            return;
        }

        FXManager.Instance.PlayDamageFX(transform, Vector3.zero);
    }

    private void PlayDamageShake()
    {
        _damageShakeSequence?.Kill();

        Vector3 baseLocalPosition = transform.localPosition;
        Vector3 shakeOffset = new Vector3(14f, 0f, 0f);
        float shakeDuration = 0.28f;
        float stepDuration = shakeDuration * 0.25f;

        _damageShakeSequence = DOTween.Sequence();
        _damageShakeSequence.Append(transform.DOLocalMove(baseLocalPosition + shakeOffset, stepDuration));
        _damageShakeSequence.Append(transform.DOLocalMove(baseLocalPosition - shakeOffset, stepDuration));
        _damageShakeSequence.Append(transform.DOLocalMove(baseLocalPosition + new Vector3(8f, 0f, 0f), stepDuration));
        _damageShakeSequence.Append(transform.DOLocalMove(baseLocalPosition, stepDuration));
    }

    private void PlayHealFX()
    {
        if (FXManager.Instance == null)
        {
            return;
        }

        FXManager.Instance.PlayHealFX(transform, Vector3.zero);
    }

    private int GetIncomingDamageReduction()
    {
        if (_owner == CardOwner.Player && PlayerController.Instance != null)
        {
            return PlayerController.Instance.GetGuardDamageReduction();
        }

        if (_owner == CardOwner.Enemy && EnemyController.Instance != null)
        {
            return EnemyController.Instance.GetGuardDamageReduction();
        }

        return 0;
    }

    private void NotifyDestroyed()
    {
        if (_owner == CardOwner.Player)
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnCardDestroyed(this);
            }

            return;
        }

        if (_owner == CardOwner.Enemy && EnemyController.Instance != null)
        {
            EnemyController.Instance.OnCardDestroyed(this);
        }
    }

    protected virtual void OnDeath(BaseCard killer)
    {
    }

    private void OnClickCard()
    {
        TurnOwner currentTurnOwner = TurnOwner.None;
        if (TurnManager.Instance != null)
        {
            currentTurnOwner = TurnManager.Instance.CurrentTurnOwner;
        }

        Debug.Log(GetClickReactionLog(currentTurnOwner));

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnClickBattleCard(this);
        }
    }

    private string GetClickReactionLog(TurnOwner currentTurnOwner)
    {
        string cardLabel = string.IsNullOrEmpty(cardName) ? name : cardName;
        string reaction = GetClickReaction(currentTurnOwner);
        return $"Card Click - card: {cardLabel}, owner: {_owner}, turn: {currentTurnOwner}, reaction: {reaction}";
    }

    private string GetClickReaction(TurnOwner currentTurnOwner)
    {
        if (currentTurnOwner == TurnOwner.None)
        {
            return "turn is not started";
        }

        if (_owner == CardOwner.None)
        {
            return "card owner is not assigned";
        }

        if (currentTurnOwner == TurnOwner.Player && _owner == CardOwner.Player)
        {
            return "player turn / player card";
        }

        if (currentTurnOwner == TurnOwner.Player && _owner == CardOwner.Enemy)
        {
            return "player turn / enemy card";
        }

        if (currentTurnOwner == TurnOwner.Enemy && _owner == CardOwner.Player)
        {
            return "enemy turn / player card";
        }

        return "enemy turn / enemy card";
    }

    protected virtual void OnDestroy()
    {
        _flipSequence?.Kill();
        _damageShakeSequence?.Kill();
    }
}
