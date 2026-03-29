// ============================================================
// File:        ObjectPool.cs
// Namespace:   TWD.Utilities
// Description: Generic object pool to avoid Instantiate/Destroy
//              GC spikes. Used for bullets, VFX, corpses, etc.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace TWD.Utilities
{
    /// <summary>
    /// Generic object pool for GameObjects. Pre-instantiates objects
    /// and recycles them to avoid garbage collection spikes.
    /// 
    /// Usage:
    ///   var pool = new ObjectPool(bulletPrefab, 20, transform);
    ///   var bullet = pool.Get();
    ///   pool.Return(bullet);
    /// </summary>
    public class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Queue<GameObject> _available;
        private readonly HashSet<GameObject> _inUse;
        private readonly int _maxSize;

        /// <summary>
        /// How many objects are currently checked out.
        /// </summary>
        public int ActiveCount => _inUse.Count;

        /// <summary>
        /// How many objects are available in the pool.
        /// </summary>
        public int AvailableCount => _available.Count;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="initialSize">How many to pre-create.</param>
        /// <param name="parent">Optional parent transform for organization.</param>
        /// <param name="maxSize">Maximum pool size. 0 = unlimited.</param>
        public ObjectPool(GameObject prefab, int initialSize, Transform parent = null, int maxSize = 0)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;
            _available = new Queue<GameObject>(initialSize);
            _inUse = new HashSet<GameObject>();

            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        /// <summary>
        /// Gets an object from the pool. Creates a new one if pool is empty.
        /// </summary>
        /// <returns>An active GameObject ready to use.</returns>
        public GameObject Get()
        {
            GameObject obj;

            if (_available.Count > 0)
            {
                obj = _available.Dequeue();
            }
            else
            {
                if (_maxSize > 0 && _inUse.Count >= _maxSize)
                {
                    Debug.LogWarning($"[ObjectPool] Max size ({_maxSize}) reached for {_prefab.name}!");
                    return null;
                }
                obj = CreateNewObject();
            }

            obj.SetActive(true);
            _inUse.Add(obj);
            return obj;
        }

        /// <summary>
        /// Gets an object and sets its position/rotation.
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var obj = Get();
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
            }
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool. Deactivates it.
        /// </summary>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            if (!_inUse.Contains(obj))
            {
                Debug.LogWarning($"[ObjectPool] Trying to return {obj.name} which is not from this pool!");
                return;
            }

            obj.SetActive(false);
            _inUse.Remove(obj);
            _available.Enqueue(obj);
        }

        /// <summary>
        /// Returns all active objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            // Copy to list to avoid modifying collection during iteration
            var active = new List<GameObject>(_inUse);
            foreach (var obj in active)
            {
                Return(obj);
            }
        }

        /// <summary>
        /// Destroys all pooled objects. Call on scene unload.
        /// </summary>
        public void Clear()
        {
            foreach (var obj in _available)
            {
                if (obj != null) Object.Destroy(obj);
            }
            foreach (var obj in _inUse)
            {
                if (obj != null) Object.Destroy(obj);
            }
            _available.Clear();
            _inUse.Clear();
        }

        private GameObject CreateNewObject()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.SetActive(false);
            _available.Enqueue(obj);
            return obj;
        }
    }
}
