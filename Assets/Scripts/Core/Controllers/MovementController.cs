using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using ArmyClash.Utilities;

namespace ArmyClash.Core
{
    /// <summary>
    /// Handles unit movement towards targets
    /// </summary>
    public class MovementController : ITickable
    {
        private readonly List<Unit> _units = new List<Unit>();
        private readonly BattleState _battleState;

        public MovementController(BattleState battleState)
        {
            _battleState = battleState;
        }

        public void RegisterUnit(Unit unit)
        {
            if (!_units.Contains(unit))
            {
                _units.Add(unit);
            }
        }

        public void UnregisterUnit(Unit unit)
        {
            _units.Remove(unit);
        }

        public void Tick()
        {
            // Check BattleState instead of BattleManager
            if (!_battleState.IsBattleActive)
                return;

            foreach (var unit in _units.ToList())
            {
                if (!unit.IsAlive) continue;

                if (unit.Target != null && unit.Target.IsAlive)
                {
                    Vector3 targetPos = unit.Target.Position;
                    Vector3 currentPos = unit.Position;
                    
                    float distance = currentPos.Distance2D(targetPos);

                    if (distance > Constants.MELEE_RANGE)
                    {
                        Vector3 direction = targetPos - currentPos;
                        direction.Normalize();
                        
                        float moveSpeed = unit.Stats.Speed;
                        Vector3 newPos = currentPos + moveSpeed * Time.deltaTime * direction;
                        
                        unit.Position = newPos;
                    }
                }
            }
        }

        public void Clear()
        {
            _units.Clear();
        }
    }
}
