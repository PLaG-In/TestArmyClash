using System.Collections.Generic;
using UnityEngine;
using Zenject;
using ArmyClash.Core;
using ArmyClash.Utilities;

namespace ArmyClash.View
{
    /// <summary>
    /// Manages all visual representations of units using prefabs and object pooling
    /// Uses ObjectPool from Utilities
    /// </summary>
    public class BattleViewManager : MonoBehaviour
    {
        private readonly Dictionary<Unit, UnitView> _unitViews = new Dictionary<Unit, UnitView>();
        
        private BattleManager _battleManager;
        private GameConfig _config;
        private UnitPrefabConfig _prefabConfig;
        private DiContainer _container;

        // Object pools for each unit type using existing ObjectPool
        private ObjectPool<UnitView> _cubePool;
        private ObjectPool<UnitView> _spherePool;

        [Inject]
        public void Construct(
            BattleManager battleManager, 
            GameConfig config, 
            UnitPrefabConfig prefabConfig,
            DiContainer container)
        {
            _battleManager = battleManager;
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
            // Subscribe to BattleManager events
            _battleManager.OnUnitSpawned += CreateUnitView;
            _battleManager.OnBattleCleared += ClearAllViews;
        }

        private void InitializeObjectPools()
        {
            // Create pool for Cubes using existing ObjectPool from Utilities
            GameObject cubePrefab = _prefabConfig.GetPrefabForShape(UnitShape.Cube);
            if (cubePrefab != null)
            {
                UnitView cubeView = cubePrefab.GetComponent<UnitView>();
                if (cubeView == null)
                {
                    cubeView = cubePrefab.AddComponent<UnitView>();
                }
                
                Transform cubeParent = new GameObject("CubePool").transform;
                cubeParent.SetParent(transform);
                
                _cubePool = new ObjectPool<UnitView>(
                    cubeView, 
                    cubeParent, 
                    initialSize: 10, 
                    maxSize: Constants.MAX_UNITS_PER_TEAM
                );
            }

            // Create pool for Spheres
            GameObject spherePrefab = _prefabConfig.GetPrefabForShape(UnitShape.Sphere);
            if (spherePrefab != null)
            {
                UnitView sphereView = spherePrefab.GetComponent<UnitView>();
                if (sphereView == null)
                {
                    sphereView = spherePrefab.AddComponent<UnitView>();
                }
                
                Transform sphereParent = new GameObject("SpherePool").transform;
                sphereParent.SetParent(transform);
                
                _spherePool = new ObjectPool<UnitView>(
                    sphereView, 
                    sphereParent, 
                    initialSize: 10, 
                    maxSize: Constants.MAX_UNITS_PER_TEAM
                );
            }

            Debug.Log($"[BattleViewManager] Initialized object pools");
        }

        private void Update()
        {
            // Update unit view positions and rotations from models
            foreach (var kvp in _unitViews)
            {
                Unit unit = kvp.Key;
                UnitView view = kvp.Value;

                if (unit != null && view != null && unit.IsAlive)
                {
                    // Sync view position with model
                    view.transform.position = unit.Position;
                    
                    // Rotate to face target
                    if (unit.Target != null && unit.Target.IsAlive)
                    {
                        Vector3 targetPos = unit.Target.Position;
                        Vector3 currentPos = unit.Position;
                        Vector3 direction = (targetPos - currentPos).normalized;
                        
                        if (direction != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(direction);
                            view.transform.rotation = Quaternion.Slerp(
                                view.transform.rotation, 
                                targetRotation, 
                                Time.deltaTime * 5f
                            );
                        }
                    }
                }
            }
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
                    if (view == null)
                    {
                        view = go.AddComponent<UnitView>();
                    }
                }
            }

            if (view == null)
            {
                Debug.LogError($"[BattleViewManager] Failed to create view for {shape}");
                return;
            }

            // Setup view
            view.gameObject.name = $"{unit.Team}_{shape}_{unit.Color}_{unit.Size}";
            
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
                    // Return to appropriate pool
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
                        view.gameObject.SafeDestroy(); // Using Extensions.SafeDestroy
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
                    // Return to pool or destroy using SafeDestroy from Extensions
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
                        kvp.Value.gameObject.SafeDestroy(); // Using Extensions
                    }
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

            // Clear object pools
            _cubePool?.Clear();
            _spherePool?.Clear();
        }

        /// <summary>
        /// Get view for a specific unit
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

        /// <summary>
        /// Get pool statistics (for debugging)
        /// </summary>
        public void LogPoolStats()
        {
            if (_cubePool != null)
            {
                var cubeStats = _cubePool.GetStats();
                Debug.Log($"[CubePool] {cubeStats}");
            }
            
            if (_spherePool != null)
            {
                var sphereStats = _spherePool.GetStats();
                Debug.Log($"[SpherePool] {sphereStats}");
            }
        }
    }
}
