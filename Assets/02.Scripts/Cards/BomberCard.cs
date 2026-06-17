using UnityEngine;

public class BomberCard : BaseCard
{
    public override CardType CardType => CardType.Bomber;

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
    }

    public override bool CanUseSkill => true;

    public override void UseSkill(BaseCard target = null)
    {
        if (HasActedThisTurn)
        {
            return;
        }

        ExplodeOnFieldCards();
        MarkAsActed();
    }

    protected override void OnDeath(BaseCard killer)
    {
        if (killer == null)
        {
            return;
        }

        if (killer.Hp <= 0)
        {
            return;
        }

        killer.TakeDamage(2, this);
    }

    private void ExplodeOnFieldCards()
    {
        BaseCard[] fieldCards = GetFieldCards();
        if (fieldCards == null || fieldCards.Length == 0)
        {
            return;
        }

        int explosionDamage = Mathf.FloorToInt(Hp * 0.5f);
        if (explosionDamage < 1)
        {
            explosionDamage = 1;
        }

        for (int i = 0; i < fieldCards.Length; i++)
        {
            BaseCard card = fieldCards[i];
            if (card == null || card == this || !card.IsAlive || !card.IsOpen)
            {
                continue;
            }

            card.TakeDamage(explosionDamage, null);
        }
    }

    private BaseCard[] GetFieldCards()
    {
        if (Owner == CardOwner.Player && PlayerController.Instance != null)
        {
            return PlayerController.Instance.Cards.ToArray();
        }

        if (Owner == CardOwner.Enemy && EnemyController.Instance != null)
        {
            return EnemyController.Instance.Cards.ToArray();
        }

        return null;
    }
}
