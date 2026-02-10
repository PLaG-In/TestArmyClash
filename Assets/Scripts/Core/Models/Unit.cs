using System;
using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Core unit model - handles health, stats, and state
    /// </summary>
    public class Unit : IDisposable
    {
        public event Action<Unit> OnDeath;
        public event Action<float> OnHealthChanged;

        public UnitShape Shape { get; private set; }
        public UnitSize Size { get; private set; }
        public UnitColor Color { get; private set; }
        public Team Team { get; private set; }

        public UnitStats Stats { get; private set; }
        public float CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;

        public Unit Target { get; set; }
        public Vector3 Position { get; set; }

        private float _lastAttackTime;

        public Unit(UnitShape shape, UnitSize size, UnitColor color, Team team, UnitStats stats)
        {
            Shape = shape;
            Size = size;
            Color = color;
            Team = team;
            Stats = stats;
            CurrentHP = stats.HP;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;

            CurrentHP = Mathf.Max(0, CurrentHP - damage);
            OnHealthChanged?.Invoke(CurrentHP);

            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        public bool CanAttack(float currentTime)
        {
            // Attack speed determines delay between attacks (higher = slower)
            float attackDelay = Stats.AtkSpeed;
            return currentTime - _lastAttackTime >= attackDelay;
        }

        public void Attack(Unit target, float currentTime)
        {
            if (!CanAttack(currentTime)) return;
            
            target.TakeDamage(Stats.ATK);
            _lastAttackTime = currentTime;
        }

        private void Die()
        {
            OnDeath?.Invoke(this);
        }

        public void Dispose()
        {
            OnDeath = null;
            OnHealthChanged = null;
        }

        public class Factory : PlaceholderFactory<UnitShape, UnitSize, UnitColor, Team, Unit>
        {
        }
    }
}
