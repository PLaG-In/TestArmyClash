using UnityEngine;

namespace ArmyClash.Core
{
    /// <summary>
    /// Configuration for unit prefabs
    /// Maps unit shapes to their visual prefabs
    /// </summary>
    [CreateAssetMenu(fileName = "UnitPrefabConfig", menuName = "ArmyClash/Unit Prefab Config")]
    public class UnitPrefabConfig : ScriptableObject
    {
        [Header("Unit Prefabs")]
        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private GameObject _spherePrefab;

        [Header("Effect Prefabs")]
        [SerializeField] private GameObject _healthBarPrefab;

        /// <summary>
        /// Get prefab for specific shape
        /// </summary>
        public GameObject GetPrefabForShape(UnitShape shape)
        {
            return shape switch
            {
                UnitShape.Cube => _cubePrefab,
                UnitShape.Sphere => _spherePrefab,
                _ => null
            };
        }

        public GameObject HealthBarPrefab => _healthBarPrefab;

        /// <summary>
        /// Validate configuration
        /// </summary>
        private void OnValidate()
        {
            if (_cubePrefab == null)
                Debug.LogWarning("[UnitPrefabConfig] Cube prefab is not assigned!");
            
            if (_spherePrefab == null)
                Debug.LogWarning("[UnitPrefabConfig] Sphere prefab is not assigned!");
        }
    }
}
