public class HealerCard : BaseCard
{
    public override CardType CardType => CardType.Healer;

    public override void Attack(BaseCard target)
    {
        target.TakeDamage(AttackPower);
    }

    public override void Destroy()
    {
        UnityEngine.Object.Destroy(gameObject);
    }
}
