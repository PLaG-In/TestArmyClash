using System.Collections.Generic;
using System.Linq;

namespace ArmyClash.Core
{
    /// <summary>
    /// Targets the strongest enemy (highest ATK)
    /// </summary>
    public class StrongestTargetingStrategy : ITargetingStrategy
    {
        public Unit FindTarget(Unit attacker, List<Unit> enemies)
        {
            return enemies.OrderByDescending(e => e.Stats.ATK).FirstOrDefault();
        }
    }
}