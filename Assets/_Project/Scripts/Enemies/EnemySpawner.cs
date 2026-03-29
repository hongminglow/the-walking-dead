// ============================================================
// File:        EnemySpawner.cs
// Namespace:   TWD.Enemies
// Description: Trigger-based enemy spawner. Spawns waves of
//              enemies when the player enters a zone. Disables
//              after all waves are cleared.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Enemies
{
    /// <summary>
    /// Spawns enemies when the player enters a trigger zone.
    /// Supports multiple waves with configurable delays.
    /// Disables permanently after all waves are cleared.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class EnemySpawner : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] _enemyPrefabs;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private int _maxEnemiesPerWave = 3;
        [SerializeField] private int _totalWaves = 1;
        [SerializeField] private float _delayBetweenSpawns = 0.5f;
        [SerializeField] private float _delayBetweenWaves = 3f;

        [Header("Behavior")]
        [SerializeField] private bool _activateOnTrigger = true;
        [SerializeField] private bool _disableAfterCleared = true;
        [SerializeField] private string _spawnerId;

        #endregion

        #region Private Fields

        private List<EnemyBase> _activeEnemies = new List<EnemyBase>();
        private int _currentWave;
        private bool _isSpawning;
        private bool _isCleared;
        private bool _hasActivated;

        #endregion

        #region Properties

        /// <summary>Number of currently alive enemies from this spawner.</summary>
        public int ActiveEnemyCount => _activeEnemies.Count;

        /// <summary>Whether all waves have been cleared.</summary>
        public bool IsCleared => _isCleared;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            // Ensure the collider is a trigger
            var col = GetComponent<BoxCollider>();
            col.isTrigger = true;

            if (string.IsNullOrEmpty(_spawnerId))
                _spawnerId = $"spawner_{gameObject.GetInstanceID()}";
        }

        private void Start()
        {
            EventBus.OnEnemyKilled += OnEnemyKilled;
        }

        private void OnDestroy()
        {
            EventBus.OnEnemyKilled -= OnEnemyKilled;
        }

        #endregion

        #region Trigger

        private void OnTriggerEnter(Collider other)
        {
            if (!_activateOnTrigger || _hasActivated || _isCleared) return;

            if (other.CompareTag(Constants.Tags.PLAYER))
            {
                _hasActivated = true;
                StartCoroutine(SpawnWaves());
            }
        }

        #endregion

        #region Spawning

        /// <summary>
        /// Manually activates the spawner (for scripted spawns).
        /// </summary>
        public void Activate()
        {
            if (_hasActivated || _isCleared) return;
            _hasActivated = true;
            StartCoroutine(SpawnWaves());
        }

        private IEnumerator SpawnWaves()
        {
            _isSpawning = true;

            for (int wave = 0; wave < _totalWaves; wave++)
            {
                _currentWave = wave + 1;

                // Don't exceed max active enemies globally
                int toSpawn = Mathf.Min(
                    _maxEnemiesPerWave,
                    Constants.Enemies.MAX_ACTIVE_ENEMIES - GetGlobalEnemyCount()
                );

                for (int i = 0; i < toSpawn; i++)
                {
                    SpawnEnemy();
                    yield return new WaitForSeconds(_delayBetweenSpawns);
                }

                // Wait for wave to clear before spawning next
                if (wave < _totalWaves - 1)
                {
                    yield return new WaitUntil(() => _activeEnemies.Count == 0);
                    yield return new WaitForSeconds(_delayBetweenWaves);
                }
            }

            _isSpawning = false;
        }

        private void SpawnEnemy()
        {
            if (_enemyPrefabs.Length == 0 || _spawnPoints.Length == 0) return;

            // Pick random prefab and spawn point
            GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

            GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            var enemyBase = enemy.GetComponent<EnemyBase>();

            if (enemyBase != null)
            {
                _activeEnemies.Add(enemyBase);
            }

            Debug.Log($"[EnemySpawner] Spawned {prefab.name} at {spawnPoint.name}. " +
                      $"Wave {_currentWave}/{_totalWaves}. Active: {_activeEnemies.Count}");
        }

        #endregion

        #region Enemy Tracking

        private void OnEnemyKilled(string enemyId)
        {
            // Remove dead enemies from tracking
            _activeEnemies.RemoveAll(e => e == null || !e.IsAlive || e.EnemyId == enemyId);

            // Check if all waves cleared
            if (!_isSpawning && _activeEnemies.Count == 0 && _hasActivated)
            {
                _isCleared = true;

                if (_disableAfterCleared)
                {
                    Debug.Log($"[EnemySpawner] {_spawnerId} cleared!");
                    gameObject.SetActive(false);
                }
            }
        }

        private int GetGlobalEnemyCount()
        {
            return FindObjectsByType<EnemyBase>(FindObjectsSortMode.None).Length;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            // Draw spawn points
            Gizmos.color = Color.red;
            if (_spawnPoints != null)
            {
                foreach (var point in _spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                        Gizmos.DrawLine(point.position, point.position + point.forward);
                    }
                }
            }
        }

        #endregion
    }
}
