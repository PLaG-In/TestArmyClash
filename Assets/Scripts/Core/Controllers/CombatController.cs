using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using ArmyClash.Utilities;

namespace ArmyClash.Core
{
    /// <summary>
    /// Handles combat logic, targeting, and battle flow
    /// Uses Constants from Utilities
    /// </summary>
    public class CombatController : ITickable
    {
        private readonly GameConfig _config;
        private readonly List<Unit> _allUnits = new List<Unit>();
        private readonly ITargetingStrategy _targetingStrategy;
        private int _frameCount = 0;

        public CombatController(GameConfig config, ITargetingStrategy targetingStrategy)
        {
            _config = config;
            _targetingStrategy = targetingStrategy;
        }

        public void RegisterUnit(Unit unit)
        {
            _allUnits.Add(unit);
            unit.OnDeath += OnUnitDeath;
        }

        public void UnregisterUnit(Unit unit)
        {
            _allUnits.Remove(unit);
            unit.OnDeath -= OnUnitDeath;
        }

        public void Tick()
        {
            _frameCount++;
            float currentTime = Time.time;

            foreach (var unit in _allUnits.Where(u => u.IsAlive).ToList())
            {
                // Update target selection periodically (using Constants.TARGET_UPDATE_INTERVAL)
                if (_frameCount % Constants.TARGET_UPDATE_INTERVAL == 0)
                {
                    if (unit.Target == null || !unit.Target.IsAlive)
                    {
                        unit.Target = _targetingStrategy.FindTarget(unit, GetEnemies(unit.Team));
                    }
                }

                // Attack if in range (using Constants.MELEE_RANGE)
                if (unit.Target != null)
                {
                    float distance = Vector3.Distance(unit.Position, unit.Target.Position);
                    
                    if (distance <= Constants.MELEE_RANGE)
                    {
                        if (unit.CanAttack(currentTime))
                        {
                            unit.Attack(unit.Target, currentTime);
                        }
                    }
                }
            }
        }

        private List<Unit> GetEnemies(Team team)
        {
            Team enemyTeam = team == Team.Team1 ? Team.Team2 : Team.Team1;
            return _allUnits.Where(u => u.Team == enemyTeam && u.IsAlive).ToList();
        }

        private void OnUnitDeath(Unit unit)
        {
            UnregisterUnit(unit);
        }

        public List<Unit> GetUnitsOfTeam(Team team)
        {
            return _allUnits.Where(u => u.Team == team && u.IsAlive).ToList();
        }

        public void Clear()
        {
            foreach (var unit in _allUnits.ToList())
            {
                unit.Dispose();
            }
            _allUnits.Clear();
        }
    }
}
