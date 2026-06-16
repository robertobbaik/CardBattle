using UnityEngine;

public class HealerCard : BaseCard
{
    public override CardType CardType => CardType.Healer;

    public override void Attack(BaseCard target)
    {
        if (target == null)
        {
            return;
        }

        target.Heal(AttackPower);
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
