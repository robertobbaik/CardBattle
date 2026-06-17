using System.Collections.Generic;
using UnityEngine;

public class ShamanCard : BaseCard
{
    public override CardType CardType => CardType.Shaman;

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

        int damage = GetAttackDamage() / 2;
        if (damage < 1)
        {
            damage = 1;
        }

        target.TakeDamage(damage, this);
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
        List<BaseCard> cards = GetEnemyBattlefieldCards();
        for (int i = 0; i < cards.Count; i++)
        {
            BaseCard card = cards[i];
            if (card == null || !card.IsAlive || !card.IsOpen)
            {
                continue;
            }

            card.ReduceHealth(2, this);
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

        target.ApplyWeaken();
        MarkAsActed();
    }

    private List<BaseCard> GetEnemyBattlefieldCards()
    {
        if (Owner == CardOwner.Player && EnemyController.Instance != null)
        {
            return EnemyController.Instance.Cards;
        }

        if (Owner == CardOwner.Enemy && PlayerController.Instance != null)
        {
            return PlayerController.Instance.Cards;
        }

        return new List<BaseCard>();
    }
}
