using UnityEngine;

public class BomberCard : BaseCard
{
    public override CardType CardType => CardType.Bomber;

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

        target.TakeDamage(3, this);

        BaseCard adjacentCard = GetRandomAdjacentEnemyCard(target);
        if (adjacentCard != null)
        {
            adjacentCard.TakeDamage(1, this);
        }

        MarkAsActed();
    }

    protected override bool ShouldReceiveCounter(BaseCard target)
    {
        return false;
    }

    protected override void OnDeath(BaseCard killer)
    {
        if (killer == null)
        {
            return;
        }

        if (killer.Hp <= 0)
        {
            return;
        }

        killer.TakeDamage(2, this);
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
