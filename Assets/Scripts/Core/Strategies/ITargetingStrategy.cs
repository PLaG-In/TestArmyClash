using System.Collections.Generic;

namespace ArmyClash.Core
{
    /// <summary>
    /// Strategy pattern for different targeting behaviors
    /// </summary>
    public interface ITargetingStrategy
    {
        Unit FindTarget(Unit attacker, List<Unit> enemies);
    }
}