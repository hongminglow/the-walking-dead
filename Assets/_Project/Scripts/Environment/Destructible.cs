// ============================================================
// File:        Destructible.cs
// Namespace:   TWD.Environment
// Description: Destructible world object. Takes damage and
//              breaks apart when health reaches zero.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Combat;

namespace TWD.Environment
{
    /// <summary>
    /// Makes a world object destructible. Can be shot, hit, or
    /// damaged by explosions. Spawns debris on destruction.
    /// </summary>
    public class Destructible : MonoBehaviour, IDamageable
    {
        #region Serialized Fields

        [Header("Health")]
        [SerializeField] private float _maxHealth = 50f;

        [Header("Destruction")]
        [SerializeField] private GameObject _destroyedVersion;
        [SerializeField] private GameObject _destroyVFX;
        [SerializeField] private AudioClip _destroySound;

        [Header("Drops")]
        [SerializeField] private GameObject[] _itemDrops;
        [SerializeField] private float _dropChance = 0.5f;

        #endregion

        #region Private Fields

        private float _currentHealth;
        private AudioSource _audioSource;

        #endregion

        #region IDamageable

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsAlive => _currentHealth > 0f;

        public void TakeDamage(float amount, Vector3 hitPoint, bool isHeadshot = false)
        {
            if (!IsAlive) return;

            _currentHealth -= amount;

            if (_currentHealth <= 0f)
            {
                DestroyObject();
            }
        }

        public void Heal(float amount) { /* Not applicable */ }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _audioSource = GetComponent<AudioSource>();
        }

        #endregion

        #region Destruction

        private void DestroyObject()
        {
            // Spawn destroyed version
            if (_destroyedVersion != null)
            {
                Instantiate(_destroyedVersion, transform.position, transform.rotation);
            }

            // Spawn VFX
            if (_destroyVFX != null)
            {
                Instantiate(_destroyVFX, transform.position, Quaternion.identity);
            }

            // Play sound
            if (_audioSource != null && _destroySound != null)
            {
                AudioSource.PlayClipAtPoint(_destroySound, transform.position);
            }

            // Drop items
            if (_itemDrops.Length > 0 && Random.value <= _dropChance)
            {
                var drop = _itemDrops[Random.Range(0, _itemDrops.Length)];
                Instantiate(drop, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        #endregion
    }
}
