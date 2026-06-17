using UnityEngine;

public class PeerlessCard : BaseCard
{
    public override CardType CardType => CardType.Peerless;

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

        AttackWithCounter(target, GetAttackDamage());
        MarkAsActed();
    }

    public override bool CanUseSkill => true;

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

        int mainDamage = GetAttackDamage();
        target.TakeDamage(mainDamage, this);

        BaseCard adjacentCard = GetRandomAdjacentEnemyCard(target);
        if (adjacentCard != null)
        {
            int splashDamage = mainDamage / 2;
            if (splashDamage < 1)
            {
                splashDamage = 1;
            }

            adjacentCard.TakeDamage(splashDamage, this);
        }

        MarkAsActed();
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
