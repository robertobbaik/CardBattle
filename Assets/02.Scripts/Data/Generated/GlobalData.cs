using System;
using System.Collections.Generic;

[Serializable]
public class CardData
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

    [Serializable]
    public class Table
    {
        public List<CardData> items = new List<CardData>();

        public Dictionary<int, CardData> ToDictionary()
        {
            Dictionary<int, CardData> dataById = new Dictionary<int, CardData>(items.Count);

            foreach (CardData data in items)
            {
                dataById.Add(data.cardId, data);
            }

            return dataById;
        }
    }
}
