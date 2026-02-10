using System;

namespace ArmyClash.Core
{
    /// <summary>
    /// Immutable structure representing unit statistics
    /// Uses operator overloading for easy stat modification
    /// </summary>
    [Serializable]
    public struct UnitStats
    {
        public float HP;
        public float ATK;
        public float Speed;
        public float AtkSpeed;

        public UnitStats(float hp, float atk, float speed, float atkSpeed)
        {
            HP = hp;
            ATK = atk;
            Speed = speed;
            AtkSpeed = atkSpeed;
        }

        /// <summary>
        /// Add two stat sets together (used for applying modifiers)
        /// Example: baseStats + shapeModifier + colorModifier
        /// </summary>
        public static UnitStats operator +(UnitStats a, UnitStats b)
        {
            return new UnitStats(
                a.HP + b.HP,
                a.ATK + b.ATK,
                a.Speed + b.Speed,
                a.AtkSpeed + b.AtkSpeed
            );
        }

        /// <summary>
        /// Multiply stats by a scalar (useful for percentage modifiers)
        /// Example: stats * 1.5f for 150% bonus
        /// </summary>
        public static UnitStats operator *(UnitStats stats, float multiplier)
        {
            return new UnitStats(
                stats.HP * multiplier,
                stats.ATK * multiplier,
                stats.Speed * multiplier,
                stats.AtkSpeed * multiplier
            );
        }

        public override string ToString()
        {
            return $"HP: {HP}, ATK: {ATK}, Speed: {Speed}, AtkSpd: {AtkSpeed}";
        }
    }
}
