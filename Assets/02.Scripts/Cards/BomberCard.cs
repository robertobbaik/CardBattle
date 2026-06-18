using System.Collections.Generic;
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

        ResolveBomberAttack(target);
        MarkAsActed();
    }

    public override void ReflectDamage(BaseCard target, int targetHpBeforeDamage)
    {
    }

    private void ResolveBomberAttack(BaseCard target)
    {
        List<BaseCard> enemyCards = GetEnemyBattlefieldCards();
        int mainDamage = Mathf.FloorToInt(Hp * 0.5f);
        int splashDamage = Mathf.FloorToInt(Hp * 0.3f);

        mainDamage = Mathf.Max(1, mainDamage);
        splashDamage = Mathf.Max(1, splashDamage);

        target.TakeEffectDamage(mainDamage, this);

        for (int i = 0; i < enemyCards.Count; i++)
        {
            BaseCard card = enemyCards[i];
            if (card == null || card == target || !card.IsAlive || !card.IsOpen)
            {
                continue;
            }

            card.TakeEffectDamage(splashDamage, this);
        }
    }

    private List<BaseCard> GetEnemyBattlefieldCards()
    {
        if (Owner == CardOwner.Player && EnemyController.Instance != null)
        {
            return EnemyController.Instance.Cards;
        }

        if (Owner == CardOwner.Enemy && PlayerController.Instance != null)
        {
            return PlayerController.Instance.Cards;
        }

        return new List<BaseCard>();
    }
}
