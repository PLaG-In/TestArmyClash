using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Core unit model - handles health, stats, buffs, and state.
    /// </summary>
    public class Unit : IDisposable
    {
        public event Action<Unit> OnDeath;
        public event Action<float> OnHealthChanged;
        public event Action<BonusEffect> OnBuffApplied;

        public UnitShape Shape { get; private set; }
        public UnitSize Size { get; private set; }
        public UnitColor Color { get; private set; }
        public Team Team { get; private set; }

        // Base stats (never change)
        public UnitStats BaseStats { get; private set; }

        // Current effective stats (base + active buffs)
        public UnitStats Stats => _currentStats;

        public float CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public bool IsShielded => _shieldEndTime > Time.time;

        public Unit Target { get; set; }
        public Vector3 Position { get; set; }

        private float _lastAttackTime;
        private UnitStats _currentStats;

        // ─── Active buffs ─────────────────────────────────────────────────────────

        private struct ActiveBuff
        {
            public BonusType Type;
            public float Value;
            public float EndTime;
        }

        private readonly List<ActiveBuff> _buffs = new List<ActiveBuff>();
        private float _shieldEndTime;

        // ─── Constructor ─────────────────────────────────────────────────────────

        public Unit(UnitShape shape, UnitSize size, UnitColor color, Team team, UnitStats stats)
        {
            Shape = shape;
            Size = size;
            Color = color;
            Team = team;
            BaseStats = stats;
            _currentStats = stats;
            CurrentHP = stats.HP;
        }

        // ─── Tick (called by CombatController each frame) ─────────────────────────

        public void Tick(float time)
        {
            ExpireBuffs(time);
        }

        // ─── Damage / Heal ────────────────────────────────────────────────────────

        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            if (IsShielded) return;   // blocked by shield

            CurrentHP = Mathf.Max(0, CurrentHP - damage);
            OnHealthChanged?.Invoke(CurrentHP);

            if (CurrentHP <= 0) Die();
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHP = Mathf.Min(_currentStats.HP, CurrentHP + amount);
            OnHealthChanged?.Invoke(CurrentHP);
        }

        // ─── Attack ───────────────────────────────────────────────────────────────

        public bool CanAttack(float currentTime)
        {
            return currentTime - _lastAttackTime >= _currentStats.AtkSpeed;
        }

        public void Attack(Unit target, float currentTime)
        {
            if (!CanAttack(currentTime)) return;
            target.TakeDamage(_currentStats.ATK);
            _lastAttackTime = currentTime;
        }

        // ─── Buffs ────────────────────────────────────────────────────────────────

        public void ApplyBuff(BonusEffect effect)
        {
            if (!IsAlive) return;

            if (effect.Duration <= 0)
            {
                // Instant effect
                ApplyInstant(effect);
            }
            else
            {
                // Timed buff
                _buffs.Add(new ActiveBuff
                {
                    Type = effect.Type,
                    Value = effect.Value,
                    EndTime = Time.time + effect.Duration
                });

                if (effect.Type == BonusType.Shield)
                {
                    _shieldEndTime = Time.time + effect.Duration;
                }

                RebuildStats();
            }

            OnBuffApplied?.Invoke(effect);
        }

        private void ApplyInstant(BonusEffect effect)
        {
            switch (effect.Type)
            {
                case BonusType.HealthRestore:
                    Heal(effect.Value);
                    break;
            }
        }

        private void ExpireBuffs(float time)
        {
            bool changed = false;
            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                if (time >= _buffs[i].EndTime)
                {
                    _buffs.RemoveAt(i);
                    changed = true;
                }
            }
            if (changed) RebuildStats();
        }
        
        private void RebuildStats()
        {
            UnitStats s = BaseStats;

            foreach (var buff in _buffs)
            {
                switch (buff.Type)
                {
                    case BonusType.AttackBoost:
                        s = new UnitStats(s.HP, s.ATK + buff.Value, s.Speed, s.AtkSpeed);
                        break;

                    case BonusType.SpeedBoost:
                        s = new UnitStats(s.HP, s.ATK, s.Speed + buff.Value, s.AtkSpeed);
                        break;

                    case BonusType.Berserker:
                        // +ATK, -Speed
                        s = new UnitStats(s.HP, s.ATK + buff.Value, s.Speed * 0.6f, s.AtkSpeed);
                        break;

                    case BonusType.Shield:
                        // No stat change
                        break;
                }
            }

            _currentStats = s;
        }

        // ─── Internals ────────────────────────────────────────────────────────────

        private void Die()
        {
            OnDeath?.Invoke(this);
        }

        public void Dispose()
        {
            OnDeath = null;
            OnHealthChanged = null;
            OnBuffApplied = null;
            _buffs.Clear();
        }

        public class Factory : PlaceholderFactory<UnitShape, UnitSize, UnitColor, Team, Unit>
        {
        }
    }
}