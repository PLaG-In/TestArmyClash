using System.Collections.Generic;
using UnityEngine;

namespace ArmyClash.Core
{

    /// <summary>
    /// Targets the nearest enemy
    /// </summary>
    public class NearestTargetingStrategy : ITargetingStrategy
    {
        public Unit FindTarget(Unit attacker, List<Unit> enemies)
        {
            if (enemies.Count == 0) return null;

            Unit nearest = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(attacker.Position, enemy.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }
    }
}