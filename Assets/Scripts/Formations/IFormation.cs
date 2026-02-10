using UnityEngine;

namespace ArmyClash.Core.Formations
{
    /// <summary>
    /// Interface for formation strategies
    /// Allows different army formations with unique positioning and bonuses
    /// </summary>
    public interface IFormation
    {
        /// <summary>
        /// Formation display name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Formation description for UI
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Calculate position for a unit in this formation
        /// </summary>
        /// <param name="unitIndex">Index of the unit (0 to totalUnits-1)</param>
        /// <param name="totalUnits">Total number of units in the formation</param>
        /// <param name="centerPoint">Center point of the formation</param>
        /// <returns>World position for the unit</returns>
        Vector3 CalculatePosition(int unitIndex, int totalUnits, Vector3 centerPoint);

        /// <summary>
        /// Get stat bonus/penalty for this formation
        /// </summary>
        /// <returns>Stat modifiers to apply to all units in formation</returns>
        UnitStats GetFormationBonus();
    }
}
