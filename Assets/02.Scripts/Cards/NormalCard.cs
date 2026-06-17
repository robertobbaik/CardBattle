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

}
