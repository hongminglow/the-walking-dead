// ============================================================
// File:        ZombieBasic.cs
// Namespace:   TWD.Enemies
// Description: Standard shambling zombie. Most common enemy type.
//              Wanders, detects player via sight/sound, chases,
//              and performs swipe attacks.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;

namespace TWD.Enemies
{
    /// <summary>
    /// Basic zombie enemy. Shambles around, chases on detection,
    /// swipes at the player. 80 HP, 15 damage, 2.0 m/s chase speed.
    /// </summary>
    public class ZombieBasic : EnemyBase
    {
        [Header("Basic Zombie Settings")]
        [SerializeField] private float _grabChance = 0.15f;
        [SerializeField] private float _grabDamage = 20f;

        protected override void PerformAttack()
        {
            // Small chance of grab attack (more damaging)
            if (Random.value < _grabChance)
            {
                PerformGrab();
            }
            else
            {
                base.PerformAttack();
            }
        }

        private void PerformGrab()
        {
            // Grab attack — more damage, longer animation
            _animator?.SetTrigger("GrabAttack");

            if (_playerTransform == null) return;
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= _data.attackRange * 1.1f)
            {
                if (_playerTransform.TryGetComponent<Combat.IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_grabDamage, transform.position);
                }
            }

            Debug.Log("[ZombieBasic] Grab attack!");
        }
    }
}
