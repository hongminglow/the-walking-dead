// ============================================================
// File:        IDamageable.cs
// Namespace:   TWD.Combat
// Description: Interface for anything that can receive damage.
//              Implemented by PlayerHealth, EnemyHealth, etc.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;

namespace TWD.Combat
{
    /// <summary>
    /// Contract for any object that can take damage.
    /// Implemented by player, enemies, destructible props, etc.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Current health points.</summary>
        float CurrentHealth { get; }

        /// <summary>Maximum health points.</summary>
        float MaxHealth { get; }

        /// <summary>Whether this entity is still alive.</summary>
        bool IsAlive { get; }

        /// <summary>
        /// Applies damage to this entity.
        /// </summary>
        /// <param name="amount">Raw damage before any modifiers.</param>
        /// <param name="hitPoint">World-space point where damage was applied.</param>
        /// <param name="isHeadshot">Whether this was a headshot for bonus damage.</param>
        void TakeDamage(float amount, Vector3 hitPoint, bool isHeadshot = false);

        /// <summary>
        /// Heals this entity.
        /// </summary>
        /// <param name="amount">Amount to heal. Won't exceed MaxHealth.</param>
        void Heal(float amount);
    }
}
