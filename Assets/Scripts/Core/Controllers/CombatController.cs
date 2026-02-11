using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using ArmyClash.Utilities;

namespace ArmyClash.Core
{
    /// <summary>
    /// Handles combat logic, targeting, and battle flow
    /// Uses BattleState to check if battle is active
    /// </summary>
    public class CombatController : ITickable
    {
        public event Action<Unit, Unit, float> OnUnitAttacked;
        
        private readonly GameConfig _config;
        private readonly List<Unit> _allUnits = new List<Unit>();
        private readonly ITargetingStrategy _targetingStrategy;
        private readonly BattleState _battleState;
        private int _frameCount = 0;

        public CombatController(GameConfig config, ITargetingStrategy targetingStrategy, BattleState battleState)
        {
            _config = config;
            _targetingStrategy = targetingStrategy;
            _battleState = battleState;
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
            if (!_battleState.IsBattleActive)
                return;

            _frameCount++;

            // Only update targeting
            foreach (var unit in _allUnits.Where(u => u.IsAlive).ToList())
            {
                // Update target selection periodically
                if (_frameCount % Constants.TARGET_UPDATE_INTERVAL == 0)
                {
                    if (unit.Target == null || !unit.Target.IsAlive)
                    {
                        unit.Target = _targetingStrategy.FindTarget(unit, GetEnemies(unit.Team));
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
