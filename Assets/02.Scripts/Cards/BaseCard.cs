using UnityEngine;

public abstract class BaseCard : MonoBehaviour
{
    [SerializeField] private int _hp;
    [SerializeField] private int _attackPower;

    public int Hp => _hp;
    public int AttackPower => _attackPower;

    public abstract CardType CardType { get; }

    public abstract void Attack(BaseCard target);

    public abstract void Destroy();

    public void TakeDamage(int damage)
    {
        _hp -= damage;

        if (_hp <= 0)
        {
            Destroy();
        }
    }
}
