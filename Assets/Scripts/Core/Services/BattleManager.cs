using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Service that manages the entire battle lifecycle
    /// Movement is now autonomous in UnitView
    /// </summary>
    public class BattleManager : IInitializable, IDisposable
    {
        public event Action<Team> OnBattleEnded;
        public event Action<Unit, Vector3, UnitShape> OnUnitSpawned;
        public event Action OnBattleCleared;

        private readonly GameConfig _config;
        private readonly IUnitFactory _unitFactory;
        private readonly CombatController _combatController;
        private readonly BattleState _battleState;

        private readonly List<Unit> _team1Units = new();
        private readonly List<Unit> _team2Units = new();

        public BattleManager(
            GameConfig config,
            IUnitFactory unitFactory,
            CombatController combatController,
            BattleState battleState)
        {
            _config = config;
            _unitFactory = unitFactory;
            _combatController = combatController;
            _battleState = battleState;
        }

        public void Initialize()
        {
        }

        public void PrepareArmies()
        {
            ClearBattle();
            SpawnArmies();
            _battleState.Reset();
            Debug.Log("[BattleManager] Armies prepared");
        }

        public void StartBattle()
        {
            if (_team1Units.Count == 0 || _team2Units.Count == 0)
            {
                PrepareArmies();
            }

            _battleState.StartBattle();
        }

        public void RandomizeArmies()
        {
            ClearBattle();
            PrepareArmies();
        }

        private void SpawnArmies()
        {
            for (int i = 0; i < _config.unitsPerArmy; i++)
            {
                Unit unit = _unitFactory.CreateRandomUnit(Team.Team1);
                Vector3 position = CalculateSpawnPosition(Team.Team1, i);
                SpawnUnit(unit, position);
                _team1Units.Add(unit);
            }

            for (int i = 0; i < _config.unitsPerArmy; i++)
            {
                Unit unit = _unitFactory.CreateRandomUnit(Team.Team2);
                Vector3 position = CalculateSpawnPosition(Team.Team2, i);
                SpawnUnit(unit, position);
                _team2Units.Add(unit);
            }
        }

        private Vector3 CalculateSpawnPosition(Team team, int index)
        {
            int rows = 4;
            int cols = 5;

            int row = index / cols;
            int col = index % cols;

            float baseX = team == Team.Team1 ? -_config.armySpacing : _config.armySpacing;
            float x = baseX + (col - cols / 2f) * _config.unitSpacing;
            float z = (row - rows / 2f) * _config.unitSpacing;
            float y = 0.5f;

            return new Vector3(x, y, z);
        }

        private void SpawnUnit(Unit unit, Vector3 position)
        {
            unit.Position = position;

            _combatController.RegisterUnit(unit);

            unit.OnDeath += OnUnitDied;

            OnUnitSpawned?.Invoke(unit, position, unit.Shape);
        }

        private void OnUnitDied(Unit unit)
        {
            _team1Units.Remove(unit);
            _team2Units.Remove(unit);
            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            if (!_battleState.IsBattleActive) return;

            if (_team1Units.Count == 0)
            {
                EndBattle(Team.Team2);
            }
            else if (_team2Units.Count == 0)
            {
                EndBattle(Team.Team1);
            }
        }

        private void EndBattle(Team winner)
        {
            _battleState.StopBattle();
            OnBattleEnded?.Invoke(winner);
            Debug.Log($"[BattleManager] Battle ended! Winner: {winner}");
        }

        public void ClearBattle()
        {
            foreach (var unit in _team1Units)
            {
                unit.Dispose();
            }
            foreach (var unit in _team2Units)
            {
                unit.Dispose();
            }

            _team1Units.Clear();
            _team2Units.Clear();
            _combatController.Clear();
            _battleState.Reset();

            OnBattleCleared?.Invoke();
        }

        public List<Unit> GetUnitsOfTeam(Team team)
        {
            return team == Team.Team1 ? _team1Units : _team2Units;
        }

        public void Dispose()
        {
            ClearBattle();
        }
    }
}
