using UnityEngine;

namespace ArmyClash.Core
{
    // ─── Bonus type ──────────────────────────────────────────────────────────────

    public enum BonusType
    {
        AttackBoost,    // +ATK
        SpeedBoost,     // +Speed
        HealthRestore,  // +HP (heal)
        Shield,         // временная неуязвимость
        Berserker,      // +ATK, -Speed
    }

    // ─── Runtime bonus effect ────────────────────────────────────────────────────

    [System.Serializable]
    public struct BonusEffect
    {
        public BonusType Type;
        public float Value;      // сколько даёт
        public float Duration;   // 0 = мгновенный, >0 = временный

        public BonusEffect(BonusType type, float value, float duration = 0)
        {
            Type = type;
            Value = value;
            Duration = duration;
        }
    }

    // ─── ScriptableObject config ─────────────────────────────────────────────────

    [CreateAssetMenu(fileName = "BonusConfig", menuName = "ArmyClash/Bonus Config")]
    public class BonusConfig : ScriptableObject
    {
        [Header("Spawner Settings")]
        [Tooltip("Seconds between bonus spawns")]
        public float spawnInterval = 8f;

        [Tooltip("Maximum bonuses alive at once")]
        public int maxBonuses = 4;

        [Tooltip("Seconds before a bonus despawns if uncollected")]
        public float bonusLifetime = 15f;

        [Header("Bonus Definitions")]
        public BonusEntry[] bonuses = new BonusEntry[]
        {
            new BonusEntry(BonusType.AttackBoost,   Color.red,    20f,  8f,  1f),
            new BonusEntry(BonusType.SpeedBoost,    Color.cyan,   5f,   8f,  1f),
            new BonusEntry(BonusType.HealthRestore, Color.green,  50f,  0f,  1f),
            new BonusEntry(BonusType.Shield,        Color.white,  0f,   5f,  1f),
            new BonusEntry(BonusType.Berserker,     Color.yellow, 40f,  8f,  0.5f),
        };

        [Header("Spawn Area (must fit battlefield)")]
        public Vector2 spawnAreaX = new Vector2(-18f, 18f);
        public Vector2 spawnAreaZ = new Vector2(-12f, 12f);

        // Find entry by type
        public BonusEntry GetEntry(BonusType type)
        {
            foreach (var e in bonuses)
                if (e.type == type) return e;
            return bonuses[0];
        }

        // Pick a random entry weighted by their weight field
        public BonusEntry GetRandom()
        {
            float total = 0;
            foreach (var e in bonuses) total += e.weight;
            float r = Random.Range(0, total);
            float acc = 0;
            foreach (var e in bonuses)
            {
                acc += e.weight;
                if (r <= acc) return e;
            }
            return bonuses[0];
        }
    }

    // ─── A single bonus definition ───────────────────────────────────────────────

    [System.Serializable]
    public class BonusEntry
    {
        public BonusType type;
        public Color color;
        public float value;        // effect magnitude
        public float duration;     // 0 = instant
        public float weight = 1f;  // spawn weight

        public BonusEntry() { }

        public BonusEntry(BonusType type, Color color, float value, float duration, float weight)
        {
            this.type = type;
            this.color = color;
            this.value = value;
            this.duration = duration;
            this.weight = weight;
        }

        public BonusEffect ToEffect() => new BonusEffect(type, value, duration);
    }
}