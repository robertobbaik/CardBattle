using UnityEngine;

public class AssassinCard : BaseCard
{
    public override CardType CardType => CardType.Assassin;

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

        if (target.Hp <= 0)
        {
            return;
        }

        int reflectedDamage = Mathf.FloorToInt(targetHpBeforeDamage * 1.5f);
        if (reflectedDamage <= 0)
        {
            return;
        }

        TakeReflectDamage(reflectedDamage, target);
    }

}
