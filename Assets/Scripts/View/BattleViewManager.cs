using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ArmyClash.Core;
using ArmyClash.Utilities;

namespace ArmyClash.View
{
    /// <summary>
    /// Manages all visual representations of units using prefabs and object pooling
    /// Integrates physics, audio, and visual effects
    /// </summary>
    public class BattleViewManager : MonoBehaviour
    {
        private readonly Dictionary<Unit, UnitView> _unitViews = new Dictionary<Unit, UnitView>();
        
        private BattleManager _battleManager;
        private CombatController _combatController;
        private GameConfig _config;
        private UnitPrefabConfig _prefabConfig;
        private DiContainer _container;

        // Object pools for each unit type
        private ObjectPool<UnitView> _cubePool;
        private ObjectPool<UnitView> _spherePool;

        [Inject]
        public void Construct(
            BattleManager battleManager,
            CombatController combatController,
            GameConfig config, 
            UnitPrefabConfig prefabConfig,
            DiContainer container)
        {
            _battleManager = battleManager;
            _combatController = combatController;
            _config = config;
            _prefabConfig = prefabConfig;
            _container = container;
        }

        private void Awake()
        {
            InitializeObjectPools();
        }

        private void Start()
        {
            _battleManager.OnUnitSpawned += CreateUnitView;
            _battleManager.OnBattleCleared += ClearAllViews;
            
            // Subscribe to combat events for visual feedback
            if (_combatController != null)
            {
                _combatController.OnUnitAttacked += OnUnitAttacked;
            }
        }

        private void InitializeObjectPools()
        {
            // Create pool for Cubes
            GameObject cubePrefab = _prefabConfig.GetPrefabForShape(UnitShape.Cube);
            if (cubePrefab != null)
            {
                Transform cubeParent = new GameObject("CubePool").transform;
                cubeParent.SetParent(transform);
                
                UnitView cubeView = cubePrefab.GetComponent<UnitView>();
                
                _cubePool = new ObjectPool<UnitView>(
                    cubeView, 
                    cubeParent, 
                    initialSize: 0, 
                    maxSize: Constants.MAX_UNITS_PER_TEAM
                );
            }

            // Create pool for Spheres
            GameObject spherePrefab = _prefabConfig.GetPrefabForShape(UnitShape.Sphere);
            if (spherePrefab != null)
            {
                Transform sphereParent = new GameObject("SpherePool").transform;
                sphereParent.SetParent(transform);
                
                UnitView sphereView = spherePrefab.GetComponent<UnitView>();
                if (sphereView == null)
                {
                    sphereView = spherePrefab.AddComponent<UnitView>();
                }
                
                _spherePool = new ObjectPool<UnitView>(
                    sphereView, 
                    sphereParent, 
                    initialSize: 0, 
                    maxSize: Constants.MAX_UNITS_PER_TEAM
                );
            }

            Debug.Log($"[BattleViewManager] Initialized object pools");
        }

        private void CreateUnitView(Unit unit, Vector3 position, UnitShape shape)
        {
            UnitView view = null;

            // Get from appropriate pool
            if (shape == UnitShape.Cube && _cubePool != null)
            {
                view = _cubePool.Get(position, Quaternion.identity);
            }
            else if (shape == UnitShape.Sphere && _spherePool != null)
            {
                view = _spherePool.Get(position, Quaternion.identity);
            }
            else
            {
                // Fallback: create from prefab directly
                Debug.LogWarning($"[BattleViewManager] No pool for shape {shape}, creating directly");
                GameObject prefab = _prefabConfig.GetPrefabForShape(shape);
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab, position, Quaternion.identity);
                    view = go.GetComponent<UnitView>();
                }
            }

            if (view == null)
            {
                Debug.LogError($"[BattleViewManager] Failed to create view for {shape}");
                return;
            }

            view.gameObject.name = $"{unit.Team}_{shape}_{unit.Color}_{unit.Size}";
            
            // Set layer and tag for physics
            view.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_UNIT);
            view.gameObject.tag = unit.Team == Team.Team1 ? Constants.TAG_TEAM1 : Constants.TAG_TEAM2;
            
            _container.Inject(view);
            view.Initialize(unit);
            _unitViews[unit] = view;

            unit.OnDeath += OnUnitDied;
        }

        private void OnUnitAttacked(Unit attacker, Unit target, float damage)
        {
            if (_unitViews.TryGetValue(attacker, out UnitView attackerView))
            {
                attackerView.PlayAttack();
            }

            if (_unitViews.TryGetValue(target, out UnitView targetView))
            {
                targetView.PlayHitReaction();
            }
        }

        private void OnUnitDied(Unit unit)
        {
            unit.OnDeath -= OnUnitDied;

            if (_unitViews.TryGetValue(unit, out UnitView view))
            {
                if (view != null)
                {
                    if (unit.Shape == UnitShape.Cube && _cubePool != null)
                    {
                        _cubePool.Return(view);
                    }
                    else if (unit.Shape == UnitShape.Sphere && _spherePool != null)
                    {
                        _spherePool.Return(view);
                    }
                    else
                    {
                        view.gameObject.SafeDestroy();
                    }
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
                    if (kvp.Key.Shape == UnitShape.Cube && _cubePool != null)
                    {
                        _cubePool.Return(kvp.Value);
                    }
                    else if (kvp.Key.Shape == UnitShape.Sphere && _spherePool != null)
                    {
                        _spherePool.Return(kvp.Value);
                    }
                    else
                    {
                        kvp.Value.gameObject.SafeDestroy();
                    }
                }
            }
            _unitViews.Clear();
        }

        private void OnDestroy()
        {
            _battleManager.OnUnitSpawned -= CreateUnitView;
            _battleManager.OnBattleCleared -= ClearAllViews;
            _combatController.OnUnitAttacked -= OnUnitAttacked;

            ClearAllViews();

            _cubePool?.Clear();
            _spherePool?.Clear();
        }

        public UnitView GetViewForUnit(Unit unit)
        {
            _unitViews.TryGetValue(unit, out UnitView view);
            return view;
        }

        public IEnumerable<UnitView> GetAllViews()
        {
            return _unitViews.Values;
        }

        public void LogPoolStats()
        {
            if (_cubePool != null)
            {
                Debug.Log($"[CubePool] {_cubePool.GetStats()}");
            }
            
            if (_spherePool != null)
            {
                Debug.Log($"[SpherePool] {_spherePool.GetStats()}");
            }
        }
    }
}
