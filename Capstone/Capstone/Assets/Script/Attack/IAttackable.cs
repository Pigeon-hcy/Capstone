using UnityEngine;

public interface IAttackable
    {
        bool TakeDamage(int amount, DamageType type, Vector2? hitPoint);
        bool IsAlive { get; }
    }
