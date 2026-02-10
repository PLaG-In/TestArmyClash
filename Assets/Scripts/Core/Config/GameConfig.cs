using UnityEngine;

namespace ArmyClash.Core
{
    /// <summary>
    /// ScriptableObject configuration for base unit stats
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "ArmyClash/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Base Stats")]
        public UnitStats baseStats = new UnitStats(100, 10, 10, 1);

        [Header("Shape Modifiers")]
        public UnitStats cubeModifier = new UnitStats(100, 10, 0, 0);
        public UnitStats sphereModifier = new UnitStats(50, 20, 0, 0);

        [Header("Size Modifiers")]
        public UnitStats bigModifier = new UnitStats(50, 0, 0, 0);
        public UnitStats smallModifier = new UnitStats(-50, 0, 0, 0);

        [Header("Color Modifiers")]
        public UnitStats blueModifier = new UnitStats(0, -15, 10, 4);
        public UnitStats greenModifier = new UnitStats(-50, 20, -5, 0);
        public UnitStats redModifier = new UnitStats(200, 40, -9, 0);

        [Header("Battle Settings")]
        public int unitsPerArmy = 20;
        public float armySpacing = 2f;
        public float unitSpacing = 1.5f;

        /// <summary>
        /// Calculate final stats based on unit properties
        /// </summary>
        public UnitStats CalculateStats(UnitShape shape, UnitSize size, UnitColor color)
        {
            UnitStats final = baseStats;

            // Apply shape modifier
            final += shape switch
            {
                UnitShape.Cube => cubeModifier,
                UnitShape.Sphere => sphereModifier,
                _ => new UnitStats(0, 0, 0, 0)
            };

            // Apply size modifier
            final += size switch
            {
                UnitSize.Big => bigModifier,
                UnitSize.Small => smallModifier,
                _ => new UnitStats(0, 0, 0, 0)
            };

            // Apply color modifier
            final += color switch
            {
                UnitColor.Blue => blueModifier,
                UnitColor.Green => greenModifier,
                UnitColor.Red => redModifier,
                _ => new UnitStats(0, 0, 0, 0)
            };

            return final;
        }

        public Color GetColorValue(UnitColor unitColor)
        {
            return unitColor switch
            {
                UnitColor.Blue => Color.blue,
                UnitColor.Green => Color.green,
                UnitColor.Red => Color.red,
                _ => Color.white
            };
        }

        public float GetSizeScale(UnitSize size)
        {
            return size switch
            {
                UnitSize.Big => 1.5f,
                UnitSize.Small => 0.75f,
                _ => 1f
            };
        }
    }
}
