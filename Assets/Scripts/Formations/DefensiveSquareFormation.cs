using UnityEngine;

namespace ArmyClash.Core.Formations
{
    /// <summary>
    /// Defensive square formation - units form a protective square
    /// Bonus: High HP, slower movement
    /// </summary>
    public class DefensiveSquareFormation : IFormation
    {
        public string Name => "Defensive Square";
        public string Description => "+30 HP, -15 SPEED - Solid defensive position";

        private readonly float _spacing;

        public DefensiveSquareFormation(float spacing = 1.5f)
        {
            _spacing = spacing;
        }

        public Vector3 CalculatePosition(int unitIndex, int totalUnits, Vector3 centerPoint)
        {
            // Create square formation
            int sideLength = Mathf.CeilToInt(Mathf.Sqrt(totalUnits));
            int row = unitIndex / sideLength;
            int col = unitIndex % sideLength;

            float offsetY = (row - sideLength / 2f) * _spacing;
            float offsetZ = (col - sideLength / 2f) * _spacing;

            return centerPoint + new Vector3(0, offsetY, offsetZ);
        }

        public UnitStats GetFormationBonus()
        {
            return new UnitStats(hp: 30, atk: 0, speed: -15, atkSpeed: 0);
        }
    }
}
