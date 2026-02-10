using UnityEngine;

namespace ArmyClash.Core.Formations
{
    /// <summary>
    /// Offensive line formation - all units in a straight horizontal line
    /// Bonus: High attack, lower HP
    /// </summary>
    public class OffensiveLineFormation : IFormation
    {
        public string Name => "Offensive Line";
        public string Description => "+20 ATK, -10 HP - Aggressive frontal assault";

        private readonly float _spacing;

        public OffensiveLineFormation(float spacing = 1.5f)
        {
            _spacing = spacing;
        }

        public Vector3 CalculatePosition(int unitIndex, int totalUnits, Vector3 centerPoint)
        {
            // Single horizontal line
            float offset = (unitIndex - totalUnits / 2f) * _spacing;
            return centerPoint + new Vector3(0, offset, 0);
        }

        public UnitStats GetFormationBonus()
        {
            return new UnitStats(hp: -10, atk: 20, speed: 0, atkSpeed: 0);
        }
    }
}
