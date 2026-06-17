using UnityEngine;

public class NormalCard : BaseCard
{
    public override CardType CardType => CardType.Normal;

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

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
