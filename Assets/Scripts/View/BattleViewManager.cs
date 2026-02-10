using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ArmyClash.Core;

namespace ArmyClash.View
{
    /// <summary>
    /// Manages all visual representations of units
    /// Bridges the gap between Core layer (BattleManager) and View layer
    /// </summary>
    public class BattleViewManager : MonoBehaviour
    {
        private readonly Dictionary<Unit, UnitView> _unitViews = new Dictionary<Unit, UnitView>();
        
        private BattleManager _battleManager;
        private GameConfig _config;
        private DiContainer _container;

        [Inject]
        public void Construct(BattleManager battleManager, GameConfig config, DiContainer container)
        {
            _battleManager = battleManager;
            _config = config;
            _container = container;
        }

        private void Start()
        {
            // Subscribe to BattleManager events
            _battleManager.OnUnitSpawned += CreateUnitView;
            _battleManager.OnBattleCleared += ClearAllViews;
        }

        private void Update()
        {
            // Update unit view positions from models
            foreach (var kvp in _unitViews)
            {
                Unit unit = kvp.Key;
                UnitView view = kvp.Value;

                if (unit != null && view != null && unit.IsAlive)
                {
                    // Sync view position with model
                    view.transform.position = unit.Position;
                    
                    // Optionally: rotate to face target
                    if (unit.Target != null && unit.Target.IsAlive)
                    {
                        Vector3 targetPos = unit.Target.Position;
                        Vector3 direction = (targetPos - unit.Position).normalized;
                        if (direction != Vector3.zero)
                        {
                            view.transform.rotation = Quaternion.LookRotation(direction);
                        }
                    }
                }
            }
        }

        private void CreateUnitView(Unit unit, Vector3 position, UnitShape shape)
        {
            // Create primitive based on shape
            GameObject go = shape == UnitShape.Cube
                ? GameObject.CreatePrimitive(PrimitiveType.Cube)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            go.transform.position = position;
            go.name = $"{unit.Team}_{shape}_{unit.Color}_{unit.Size}";

            // Add UnitView component
            UnitView view = go.AddComponent<UnitView>();
            
            // Inject dependencies
            _container.Inject(view);
            
            // Initialize view with unit model
            view.Initialize(unit);

            // Store reference
            _unitViews[unit] = view;

            // Subscribe to unit death
            unit.OnDeath += OnUnitDied;
        }

        private void OnUnitDied(Unit unit)
        {
            if (_unitViews.TryGetValue(unit, out UnitView view))
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
                _unitViews.Remove(unit);
            }
        }

        private void ClearAllViews()
        {
            foreach (var kvp in _unitViews)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _unitViews.Clear();
        }

        private void OnDestroy()
        {
            if (_battleManager != null)
            {
                _battleManager.OnUnitSpawned -= CreateUnitView;
                _battleManager.OnBattleCleared -= ClearAllViews;
            }

            ClearAllViews();
        }

        /// <summary>
        /// Get view for a specific unit (useful for selection, etc.)
        /// </summary>
        public UnitView GetViewForUnit(Unit unit)
        {
            _unitViews.TryGetValue(unit, out UnitView view);
            return view;
        }

        /// <summary>
        /// Get all active unit views
        /// </summary>
        public IEnumerable<UnitView> GetAllViews()
        {
            return _unitViews.Values;
        }
    }
}
