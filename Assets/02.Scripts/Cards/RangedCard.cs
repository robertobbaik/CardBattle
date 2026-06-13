public class RangedCard : BaseCard
{
    public override CardType CardType => CardType.Ranged;

    public override void Attack(BaseCard target)
    {
        target.TakeDamage(AttackPower);
    }

    public override void Destroy()
    {
        UnityEngine.Object.Destroy(gameObject);
    }
}
