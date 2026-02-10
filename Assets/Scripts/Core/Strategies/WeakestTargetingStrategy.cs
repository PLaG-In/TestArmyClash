using System.Collections.Generic;
using System.Linq;

namespace ArmyClash.Core
{

    /// <summary>
    /// Targets the weakest enemy (lowest HP)
    /// </summary>
    public class WeakestTargetingStrategy : ITargetingStrategy
    {
        public Unit FindTarget(Unit attacker, List<Unit> enemies)
        {
            return enemies.OrderBy(e => e.CurrentHP).FirstOrDefault();
        }
    }
}