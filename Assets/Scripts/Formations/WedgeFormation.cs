using UnityEngine;

namespace ArmyClash.Core.Formations
{
    /// <summary>
    /// Wedge formation - triangle/spearhead shape pointing forward
    /// Bonus: High speed and attack for breakthrough tactics
    /// </summary>
    public class WedgeFormation : IFormation
    {
        public string Name => "Wedge Formation";
        public string Description => "+25 SPEED, +10 ATK - Pierce enemy lines";

        private readonly float _spacing;

        public WedgeFormation(float spacing = 1.5f)
        {
            _spacing = spacing;
        }

        public Vector3 CalculatePosition(int unitIndex, int totalUnits, Vector3 centerPoint)
        {
            // Triangle/wedge shape pointing forward
            // Using triangular number sequence to arrange units
            int row = Mathf.FloorToInt(Mathf.Sqrt(unitIndex * 2));
            int col = unitIndex - (row * (row + 1)) / 2;

            float offsetX = row * _spacing;
            float offsetY = (col - row / 2f) * _spacing;

            return centerPoint + new Vector3(offsetX, offsetY, 0);
        }

        public UnitStats GetFormationBonus()
        {
            return new UnitStats(hp: 0, atk: 10, speed: 25, atkSpeed: 0);
        }
    }
}
