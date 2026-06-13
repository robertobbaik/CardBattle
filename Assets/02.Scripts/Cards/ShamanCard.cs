public class ShamanCard : BaseCard
{
    public override CardType CardType => CardType.Shaman;

    public override void Attack(BaseCard target)
    {
        target.TakeDamage(AttackPower);
    }

    public override void Destroy()
    {
        UnityEngine.Object.Destroy(gameObject);
    }
}
