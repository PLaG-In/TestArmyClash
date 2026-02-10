using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Service that manages the entire battle lifecycle
    /// Uses events to communicate with View layer (no direct dependencies)
    /// </summary>
    public class BattleManager : IInitializable, IDisposable
    {
        public event Action<Team> OnBattleEnded;
        public event Action<Unit, Vector3, UnitShape> OnUnitSpawned;
        public event Action OnBattleCleared;
        
        private readonly GameConfig _config;
        private readonly IUnitFactory _unitFactory;
        private readonly CombatController _combatController;
        private readonly MovementController _movementController;

        private readonly List<Unit> _team1Units = new List<Unit>();
        private readonly List<Unit> _team2Units = new List<Unit>();

        private bool _battleActive;

        public BattleManager(
            GameConfig config,
            IUnitFactory unitFactory,
            CombatController combatController,
            MovementController movementController)
        {
            _config = config;
            _unitFactory = unitFactory;
            _combatController = combatController;
            _movementController = movementController;
        }

        public void Initialize()
        {
            // Initialization if needed
        }

        public void StartBattle()
        {
            ClearBattle();
            SpawnArmies();
            _battleActive = true;
        }

        public void RandomizeArmies()
        {
            if (_battleActive)
            {
                ClearBattle();
            }
            SpawnArmies();
        }

        private void SpawnArmies()
        {
            // Spawn Team 1 (left side)
            for (int i = 0; i < _config.unitsPerArmy; i++)
            {
                Unit unit = _unitFactory.CreateRandomUnit(Team.Team1);
                Vector3 position = CalculateSpawnPosition(Team.Team1, i);
                SpawnUnit(unit, position);
                _team1Units.Add(unit);
            }

            // Spawn Team 2 (right side)
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
            int rows = 4; // 5x4 formation for 20 units
            int cols = 5;
            
            int row = index / cols;
            int col = index % cols;

            float xOffset = team == Team.Team1 ? -_config.armySpacing : _config.armySpacing;
            float x = xOffset;
            float z = (row - rows / 2f) * _config.unitSpacing;
            float y = (col - cols / 2f) * _config.unitSpacing;

            return new Vector3(x, y, z);
        }

        private void SpawnUnit(Unit unit, Vector3 position)
        {
            unit.Position = position;
            
            _combatController.RegisterUnit(unit);
            _movementController.RegisterUnit(unit);
            
            unit.OnDeath += OnUnitDied;
            
            // Notify View layer to create visual representation
            OnUnitSpawned?.Invoke(unit, position, unit.Shape);
        }

        private void OnUnitDied(Unit unit)
        {
            _team1Units.Remove(unit);
            _team2Units.Remove(unit);
            _movementController.UnregisterUnit(unit);

            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            if (!_battleActive) return;

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
            _battleActive = false;
            OnBattleEnded?.Invoke(winner);
            Debug.Log($"Battle ended! Winner: {winner}");
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
            _movementController.Clear();
            _battleActive = false;
            
            // Notify View layer to destroy all visuals
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
