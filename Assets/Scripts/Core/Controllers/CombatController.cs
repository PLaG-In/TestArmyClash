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
        private readonly HashSet<Unit> _team1Units = new HashSet<Unit>();
        private readonly HashSet<Unit> _team2Units = new HashSet<Unit>();
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
            if (unit.Team == Team.Team1) _team1Units.Add(unit);
            else _team2Units.Add(unit);

            unit.OnDeath += OnUnitDeath;
        }

        public void UnregisterUnit(Unit unit)
        {
            _allUnits.Remove(unit);
            _team1Units.Remove(unit);
            _team2Units.Remove(unit);
            unit.OnDeath -= OnUnitDeath;
        }

        public void Tick()
        {
            if (!_battleState.IsBattleActive)
                return;

            _frameCount++;

            if (_frameCount % Constants.TARGET_UPDATE_INTERVAL == 0)
            {
                UpdateTargets(_team1Units);
                UpdateTargets(_team2Units);
            }
        }

        private void UpdateTargets(IEnumerable<Unit> units)
        {
            foreach (var unit in units)
            {
                if (unit.IsAlive && unit.Target is not { IsAlive: true })
                {
                    unit.Target = _targetingStrategy.FindTarget(unit, GetEnemies(unit.Team).ToList());
                }
            }
        }

        private IEnumerable<Unit> GetEnemies(Team team)
        {
            return team == Team.Team1 ? _team2Units : _team1Units;
        }

        private void OnUnitDeath(Unit unit)
        {
            UnregisterUnit(unit);
        }

        public List<Unit> GetUnitsOfTeam(Team team)
        {
            return team == Team.Team1 ? _team1Units.ToList() : _team2Units.ToList();
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
