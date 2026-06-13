using UnityEngine;

public abstract class BaseCard : MonoBehaviour
{
    public int cardId;
    public int cardType;
    public int maxHp;
    public int startHp;
    public float damageMultiplier;
    public float counterDamageMultiplier;
    public float adjacentDamageMultiplier;
    public int maxAdjacentTargets;
    public int turnStartHealAmount;
    public int damageReduction;
    public int flatDamageBonus;
    public bool hasTurnStartEffect;
    public bool ignoresCounter;
    public bool appliesWeaken;
    public bool appliesInspired;
    public bool hasDeathTrigger;
    public int executeThresholdHp;
    public int selfDamageOnAttack;
    public int weakenAmount;
    public int weakenDurationTurns;
    public int deathDamage;

    private int _currentHp;

    public int Hp => _currentHp;
    public int AttackPower => _currentHp + flatDamageBonus;

    public abstract CardType CardType { get; }

    public abstract void Attack(BaseCard target);

    public abstract void Destroy();

    public void Initialize(CardData cardData)
    {
        cardId = cardData.cardId;
        cardType = cardData.cardType;
        maxHp = cardData.maxHp;
        startHp = cardData.startHp;
        damageMultiplier = cardData.damageMultiplier;
        counterDamageMultiplier = cardData.counterDamageMultiplier;
        adjacentDamageMultiplier = cardData.adjacentDamageMultiplier;
        maxAdjacentTargets = cardData.maxAdjacentTargets;
        turnStartHealAmount = cardData.turnStartHealAmount;
        damageReduction = cardData.damageReduction;
        flatDamageBonus = cardData.flatDamageBonus;
        hasTurnStartEffect = cardData.hasTurnStartEffect;
        ignoresCounter = cardData.ignoresCounter;
        appliesWeaken = cardData.appliesWeaken;
        appliesInspired = cardData.appliesInspired;
        hasDeathTrigger = cardData.hasDeathTrigger;
        executeThresholdHp = cardData.executeThresholdHp;
        selfDamageOnAttack = cardData.selfDamageOnAttack;
        weakenAmount = cardData.weakenAmount;
        weakenDurationTurns = cardData.weakenDurationTurns;
        deathDamage = cardData.deathDamage;

        _currentHp = startHp;
    }

    public void TakeDamage(int damage)
    {
        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            Destroy();
        }
    }
}
