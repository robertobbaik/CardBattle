using UnityEngine;

public class GuardianCard : BaseCard
{
    public override CardType CardType => CardType.Guardian;

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

        int damage = Mathf.FloorToInt(Hp * 0.8f);
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

        int reflectedDamage = Mathf.FloorToInt(targetHpBeforeDamage * 1.2f);
        if (reflectedDamage <= 0)
        {
            return;
        }

        TakeReflectDamage(reflectedDamage, target);
    }

}
