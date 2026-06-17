using UnityEngine;

public class RangedCard : BaseCard
{
    public override CardType CardType => CardType.Ranged;

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

    protected override bool ShouldReceiveCounter(BaseCard target)
    {
        return false;
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
