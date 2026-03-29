// ============================================================
// File:        EnemyData.cs
// Namespace:   TWD.Enemies
// Description: ScriptableObject definition for enemy stats.
//              Create instances via Assets > Create > TWD > Enemies.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Utilities;

namespace TWD.Enemies
{
    /// <summary>
    /// Data container for enemy stats and behavior configuration.
    /// Create instances in Assets/_Project/ScriptableObjects/Enemies/.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_NewEnemy", menuName = "TWD/Enemies/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Enemy display name (for debug/UI).")]
        public string enemyName;

        [Tooltip("What kind of zombie this is.")]
        public EnemyType enemyType;

        [Header("Health")]
        [Tooltip("Maximum health points.")]
        public float maxHealth = 80f;

        [Tooltip("Damage threshold that causes stagger animation.")]
        public float staggerThreshold = 30f;

        [Header("Combat")]
        [Tooltip("Damage dealt per attack.")]
        public float attackDamage = 15f;

        [Tooltip("Seconds between attacks.")]
        public float attackCooldown = 1.5f;

        [Tooltip("Range at which the enemy will start attacking.")]
        public float attackRange = 1.5f;

        [Header("Movement")]
        [Tooltip("Normal walking/shambling speed.")]
        public float walkSpeed = 1.2f;

        [Tooltip("Chase/run speed when pursuing player.")]
        public float chaseSpeed = 2.0f;

        [Tooltip("NavMeshAgent angular speed.")]
        public float angularSpeed = 120f;

        [Tooltip("NavMeshAgent acceleration.")]
        public float acceleration = 8f;

        [Header("Detection")]
        [Tooltip("Sight cone half-angle in degrees.")]
        public float sightAngle = 60f;

        [Tooltip("Maximum sight distance in meters.")]
        public float sightRange = 15f;

        [Tooltip("Radius within which sounds alert this enemy.")]
        public float hearingRange = 8f;

        [Header("Behavior Timings")]
        [Tooltip("Seconds to keep chasing after losing sight.")]
        public float chaseLoseDuration = 8f;

        [Tooltip("Seconds to search around last known position.")]
        public float searchDuration = 5f;

        [Tooltip("Wander radius when idle.")]
        public float wanderRadius = 5f;

        [Header("Death")]
        [Tooltip("Seconds before corpse fades out.")]
        public float corpseLifetime = 30f;

        [Tooltip("XP or score value when killed.")]
        public int killScore = 100;

        [Header("Audio")]
        [Tooltip("Idle groaning/moaning sounds (random pick).")]
        public AudioClip[] idleSounds;

        [Tooltip("Alert/detection scream.")]
        public AudioClip alertSound;

        [Tooltip("Attack sound.")]
        public AudioClip attackSound;

        [Tooltip("Pain/hit reaction sound.")]
        public AudioClip hitSound;

        [Tooltip("Death sound.")]
        public AudioClip deathSound;
    }
}
