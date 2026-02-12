using System.Collections.Generic;
using UnityEngine;

namespace ArmyClash.Utilities
{
    /// <summary>
    /// Generic object pool for reusing GameObjects
    /// Improves performance by avoiding frequent Instantiate/Destroy calls
    /// </summary>
    /// <typeparam name="T">Component type to pool</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _availableObjects = new Queue<T>();
        private readonly List<T> _allObjects = new List<T>();
        private readonly int _initialSize;
        private readonly int _maxSize;

        /// <summary>
        /// Create a new object pool
        /// </summary>
        /// <param name="prefab">Prefab to pool</param>
        /// <param name="parent">Parent transform for pooled objects</param>
        /// <param name="initialSize">Number of objects to pre-instantiate</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
        public ObjectPool(T prefab, Transform parent = null, int initialSize = 10, int maxSize = 100)
        {
            _prefab = prefab;
            _parent = parent;
            _initialSize = initialSize;
            _maxSize = maxSize;

            // Pre-warm the pool
            for (int i = 0; i < _initialSize; i++)
            {
                CreateNewObject();
            }
        }

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        public T Get()
        {
            T obj;

            if (_availableObjects.Count > 0)
            {
                // Reuse existing object
                obj = _availableObjects.Dequeue();
            }
            else
            {
                // Create new object if under max size
                if (_maxSize == 0 || _allObjects.Count < _maxSize)
                {
                    obj = CreateNewObject();
                }
                else
                {
                    // Pool is at max capacity, reuse oldest
                    Debug.LogWarning($"ObjectPool<{typeof(T).Name}> at max capacity ({_maxSize}). Reusing oldest object.");
                    obj = _allObjects[0];
                }
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            obj.gameObject.SetActive(false);
            
            if (_parent != null)
            {
                obj.transform.SetParent(_parent);
            }

            if (!_availableObjects.Contains(obj))
            {
                _availableObjects.Enqueue(obj);
            }
        }

        /// <summary>
        /// Get or create an object at a specific position and rotation
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = Get();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position        = position;
                rb.rotation        = rotation;
                rb.linearVelocity        = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return obj;
        }

        /// <summary>
        /// Clear all objects from the pool
        /// </summary>
        public void Clear()
        {
            foreach (var obj in _allObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            _availableObjects.Clear();
            _allObjects.Clear();
        }

        /// <summary>
        /// Resize the pool
        /// </summary>
        public void Resize(int newSize)
        {
            if (newSize > _allObjects.Count)
            {
                // Grow pool
                int toCreate = newSize - _allObjects.Count;
                for (int i = 0; i < toCreate; i++)
                {
                    CreateNewObject();
                }
            }
            else if (newSize < _allObjects.Count)
            {
                // Shrink pool
                int toRemove = _allObjects.Count - newSize;
                for (int i = 0; i < toRemove; i++)
                {
                    if (_availableObjects.Count > 0)
                    {
                        T obj = _availableObjects.Dequeue();
                        _allObjects.Remove(obj);
                        Object.Destroy(obj.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Get pool statistics
        /// </summary>
        public PoolStats GetStats()
        {
            return new PoolStats
            {
                TotalObjects = _allObjects.Count,
                AvailableObjects = _availableObjects.Count,
                ActiveObjects = _allObjects.Count - _availableObjects.Count,
                MaxSize = _maxSize
            };
        }

        private T CreateNewObject()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _allObjects.Add(obj);
            return obj;
        }

        public struct PoolStats
        {
            public int TotalObjects;
            public int AvailableObjects;
            public int ActiveObjects;
            public int MaxSize;

            public override string ToString()
            {
                return $"Pool Stats - Total: {TotalObjects}, Available: {AvailableObjects}, Active: {ActiveObjects}, Max: {MaxSize}";
            }
        }
    }

    /// <summary>
    /// Simple object pool manager for multiple pools
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;
        public static PoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PoolManager");
                    _instance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<string, object> _pools = new Dictionary<string, object>();

        /// <summary>
        /// Create or get a pool by name
        /// </summary>
        public ObjectPool<T> GetOrCreatePool<T>(string poolName, T prefab, int initialSize = 10, int maxSize = 100) where T : Component
        {
            if (_pools.TryGetValue(poolName, out var pool1))
            {
                return pool1 as ObjectPool<T>;
            }

            Transform parent = new GameObject($"Pool_{poolName}").transform;
            parent.SetParent(transform);

            var pool = new ObjectPool<T>(prefab, parent, initialSize, maxSize);
            _pools[poolName] = pool;

            return pool;
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                if (pool is ObjectPool<Component> objPool)
                {
                    objPool.Clear();
                }
            }
            _pools.Clear();
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
}
