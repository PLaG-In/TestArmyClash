using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Handles unit movement towards targets
    /// Works purely with Unit models, communicates position updates via Unit.Position
    /// </summary>
    public class MovementController : ITickable
    {
        private readonly List<Unit> _units = new List<Unit>();
        private const float MELEE_RANGE = 1f;

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
            foreach (var unit in _units.ToList())
            {
                if (!unit.IsAlive) continue;

                // Move towards target if we have one
                if (unit.Target != null && unit.Target.IsAlive)
                {
                    Vector3 targetPos = unit.Target.Position;
                    Vector3 currentPos = unit.Position;
                    
                    float distance = Vector3.Distance(currentPos, targetPos);

                    // Move if not in melee range
                    if (distance > MELEE_RANGE)
                    {
                        Vector3 direction = (targetPos - currentPos).normalized;
                        float moveSpeed = unit.Stats.Speed;
                        
                        Vector3 newPos = currentPos + direction * moveSpeed * Time.deltaTime;
                        unit.Position = newPos;
                        
                        // Note: View layer should subscribe to position changes
                        // or poll unit.Position every frame
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
