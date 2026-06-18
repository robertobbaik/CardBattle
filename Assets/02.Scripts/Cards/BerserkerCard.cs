using UnityEngine;

public class BerserkerCard : BaseCard
{
    public override CardType CardType => CardType.Berserker;

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

        int attackDamage = Mathf.FloorToInt(Hp * 1.2f);
        if (attackDamage < 1)
        {
            attackDamage = 1;
        }

        target.TakeDamage(attackDamage, this);
        TakeSelfDamage();
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

    private void TakeSelfDamage()
    {
        if (!IsAlive)
        {
            return;
        }

        int selfDamage = Mathf.FloorToInt(MaxHp * 0.2f);
        if (selfDamage < 1)
        {
            selfDamage = 1;
        }

        TakeEffectDamage(selfDamage, this);
    }

}
