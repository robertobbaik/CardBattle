using System.Collections.Generic;
using UnityEngine;

public class CommanderCard : BaseCard
{
    public override CardType CardType => CardType.Commander;

    public override void Attack(BaseCard target)
    {
        if (target == null)
        {
            return;
        }

        if (!CanAttack)
        {
            return;
        }

        target.TakeDamage(GetAttackDamage(), this);
        MarkAsActed();
    }

    public override void ReflectDamage(BaseCard target, int targetHpBeforeDamage)
    {
        if (target == null)
        {
            return;
        }

        TakeReflectDamage(targetHpBeforeDamage, target);
    }

    public override bool CanUseSkill => true;

    protected override void OnEnterField()
    {
        List<BaseCard> cards = GetAlliedBattlefieldCards();
        for (int i = 0; i < cards.Count; i++)
        {
            BaseCard card = cards[i];
            if (card == null || !card.IsAlive || !card.IsOpen)
            {
                continue;
            }

            card.AddHealth(2);
        }
    }

    public override void UseSkill(BaseCard target = null)
    {
        if (target == null)
        {
            return;
        }

        if (HasActedThisTurn)
        {
            return;
        }

        target.ApplyInspired();
        MarkAsActed();
    }

    private List<BaseCard> GetAlliedBattlefieldCards()
    {
        if (Owner == CardOwner.Player && PlayerController.Instance != null)
        {
            return PlayerController.Instance.Cards;
        }

        if (Owner == CardOwner.Enemy && EnemyController.Instance != null)
        {
            return EnemyController.Instance.Cards;
        }

        return new List<BaseCard>();
    }
}
