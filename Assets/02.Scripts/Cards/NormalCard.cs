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

        target.TakeDamage(AttackPower);

        int selfDamage = Mathf.Max(target.Hp, 0);
        if (selfDamage > 0)
        {
            TakeDamage(selfDamage);
        }
    }

    public override void Destroy()
    {
        Object.Destroy(gameObject);
    }
}
