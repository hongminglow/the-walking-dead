// ============================================================
// File:        EnemyBase.cs
// Namespace:   TWD.Enemies
// Description: Abstract base class for all enemies. Contains
//              shared health, detection, damage, and death logic.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.AI;
using TWD.Combat;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Enemies
{
    /// <summary>
    /// Abstract base for all enemy types. Provides health, damage,
    /// detection, and death systems. Subclasses add unique behaviors.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        #region Serialized Fields

        [Header("Data")]
        [SerializeField] protected EnemyData _data;

        [Header("Detection")]
        [SerializeField] protected Transform _eyeTransform;
        [SerializeField] protected LayerMask _playerLayer;
        [SerializeField] protected LayerMask _obstacleLayers;

        [Header("Debug")]
        [SerializeField] private bool _showGizmos = true;

        #endregion

        #region Protected Fields

        protected NavMeshAgent _agent;
        protected Animator _animator;
        protected AudioSource _audioSource;
        protected Transform _playerTransform;
        protected EnemyState _currentState = EnemyState.Idle;
        protected float _currentHealth;
        protected float _attackTimer;
        protected float _stateTimer;
        protected Vector3 _lastKnownPlayerPosition;
        protected string _enemyId;

        #endregion

        #region IDamageable

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _data.maxHealth;
        public bool IsAlive => _currentState != EnemyState.Dead;

        #endregion

        #region Properties

        /// <summary>Current AI state.</summary>
        public EnemyState CurrentState => _currentState;

        /// <summary>Unique ID for save system tracking.</summary>
        public string EnemyId => _enemyId;

        #endregion

        #region Lifecycle

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            ResolveRuntimeDefaults();
            if (GetComponent<EnemyCharacterVisual>() == null)
                gameObject.AddComponent<EnemyCharacterVisual>();
            _enemyId = _data != null ? $"{_data.enemyType}_{gameObject.GetInstanceID()}" : $"enemy_{gameObject.GetInstanceID()}";
        }

        protected virtual void Start()
        {
            if (_data == null)
            {
                Debug.LogWarning($"[EnemyBase] Missing EnemyData on {gameObject.name}. Disabling enemy component.");
                enabled = false;
                return;
            }

            _currentHealth = _data.maxHealth;

            // Cache player reference
            var player = GameObject.FindWithTag(Constants.Tags.PLAYER);
            if (player != null) _playerTransform = player.transform;

            // Setup NavMeshAgent from data
            _agent.speed = _data.walkSpeed;
            _agent.angularSpeed = _data.angularSpeed;
            _agent.acceleration = _data.acceleration;
            _agent.stoppingDistance = _data.attackRange * 0.8f;

            SetState(EnemyState.Idle);
        }

        protected virtual void Update()
        {
            if (_currentState == EnemyState.Dead) return;
            if (!GameManager.Instance.IsPlaying) return;

            _attackTimer -= Time.deltaTime;
            _stateTimer -= Time.deltaTime;

            UpdateStateMachine();
            UpdateAnimator();
        }

        #endregion

        #region State Machine

        private void ResolveRuntimeDefaults()
        {
            if (_data == null)
            {
                EnemyType fallbackType = EnemyType.ZombieBasic;
                if (this is ZombieBrute) fallbackType = EnemyType.ZombieBrute;
                else if (this is ZombieCrawler) fallbackType = EnemyType.ZombieCrawler;

                _data = RuntimeSceneResolver.FindEnemyData(fallbackType);
            }

            if (_eyeTransform == null)
                _eyeTransform = transform;

            if (_playerLayer.value == 0)
                _playerLayer = RuntimeSceneResolver.MaskFromLayers(Constants.Layers.PLAYER);

            if (_obstacleLayers.value == 0)
                _obstacleLayers = RuntimeSceneResolver.MaskFromLayers(Constants.Layers.DEFAULT, Constants.Layers.GROUND, Constants.Layers.OBSTACLE);
        }

        protected virtual void UpdateStateMachine()
        {
            switch (_currentState)
            {
                case EnemyState.Idle:
                    OnIdle();
                    break;
                case EnemyState.Wandering:
                    OnWander();
                    break;
                case EnemyState.Chasing:
                    OnChase();
                    break;
                case EnemyState.Attacking:
                    OnAttack();
                    break;
                case EnemyState.Searching:
                    OnSearch();
                    break;
                case EnemyState.Staggered:
                    OnStagger();
                    break;
            }
        }

        /// <summary>Sets the AI to a new state.</summary>
        protected void SetState(EnemyState newState)
        {
            if (_currentState == newState) return;

            ExitState(_currentState);
            _currentState = newState;
            EnterState(newState);
        }

        protected virtual void EnterState(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Idle:
                    if (HasActiveNavMeshAgent())
                    {
                        _agent.speed = _data.walkSpeed;
                        _agent.ResetPath();
                    }
                    _stateTimer = Random.Range(2f, 5f);
                    break;

                case EnemyState.Wandering:
                    if (HasActiveNavMeshAgent())
                    {
                        _agent.speed = _data.walkSpeed;
                        WanderToRandomPoint();
                    }
                    else
                    {
                        _stateTimer = Random.Range(1.5f, 3f);
                    }
                    break;

                case EnemyState.Chasing:
                    if (HasActiveNavMeshAgent())
                        _agent.speed = _data.chaseSpeed;
                    PlaySound(_data.alertSound);
                    EventBus.EnemyAlerted(_enemyId);
                    break;

                case EnemyState.Attacking:
                    if (HasActiveNavMeshAgent())
                        _agent.ResetPath();
                    break;

                case EnemyState.Searching:
                    _stateTimer = _data.searchDuration;
                    if (HasActiveNavMeshAgent())
                    {
                        _agent.speed = _data.walkSpeed;
                        _agent.SetDestination(_lastKnownPlayerPosition);
                    }
                    break;

                case EnemyState.Staggered:
                    if (HasActiveNavMeshAgent())
                        _agent.ResetPath();
                    _stateTimer = 1f;
                    if (_animator != null)
                        _animator.SetTrigger(Constants.AnimParams.STAGGER);
                    break;

                case EnemyState.Dead:
                    _agent.enabled = false;
                    break;
            }
        }

        protected virtual void ExitState(EnemyState state)
        {
            // Override in subclasses if cleanup is needed
        }

        #endregion

        #region State Behaviors

        protected virtual void OnIdle()
        {
            if (CanSeePlayer() || CanHearPlayer())
            {
                SetState(EnemyState.Chasing);
                return;
            }

            if (_stateTimer <= 0f)
            {
                SetState(EnemyState.Wandering);
            }

            // Play idle sounds randomly
            if (Random.value < 0.001f && _data.idleSounds.Length > 0)
            {
                PlayRandomSound(_data.idleSounds);
            }
        }

        protected virtual void OnWander()
        {
            if (!HasActiveNavMeshAgent())
            {
                if (_stateTimer <= 0f)
                    SetState(EnemyState.Idle);
                return;
            }

            if (CanSeePlayer() || CanHearPlayer())
            {
                SetState(EnemyState.Chasing);
                return;
            }

            // Arrived at wander point
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                SetState(EnemyState.Idle);
            }
        }

        protected virtual void OnChase()
        {
            if (_playerTransform == null) return;

            if (CanSeePlayer())
            {
                _lastKnownPlayerPosition = _playerTransform.position;
                _stateTimer = _data.chaseLoseDuration;
            }

            if (HasActiveNavMeshAgent())
                _agent.SetDestination(_lastKnownPlayerPosition);

            // In attack range?
            float distToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            if (distToPlayer <= _data.attackRange && CanSeePlayer())
            {
                SetState(EnemyState.Attacking);
                return;
            }

            // Lost player for too long?
            if (!CanSeePlayer())
            {
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    SetState(EnemyState.Searching);
                    EventBus.EnemyLostPlayer(_enemyId);
                }
            }
        }

        protected virtual void OnAttack()
        {
            if (_playerTransform == null) return;

            // Face the player
            Vector3 direction = (_playerTransform.position - transform.position).Flat().normalized;
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * _data.angularSpeed
                );
            }

            // Attack on cooldown
            if (_attackTimer <= 0f)
            {
                PerformAttack();
                _attackTimer = _data.attackCooldown;
            }

            // Player left range?
            float distToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            if (distToPlayer > _data.attackRange * 1.2f)
            {
                SetState(EnemyState.Chasing);
            }
        }

        protected virtual void OnSearch()
        {
            if (!HasActiveNavMeshAgent())
            {
                if (_stateTimer <= 0f)
                    SetState(EnemyState.Idle);
                return;
            }

            if (CanSeePlayer() || CanHearPlayer())
            {
                SetState(EnemyState.Chasing);
                return;
            }

            if (_stateTimer <= 0f || (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance))
            {
                SetState(EnemyState.Idle);
            }
        }

        protected virtual void OnStagger()
        {
            if (_stateTimer <= 0f)
            {
                SetState(EnemyState.Chasing);
            }
        }

        #endregion

        #region Detection

        /// <summary>Checks if the player is within the sight cone and not occluded.</summary>
        protected bool CanSeePlayer()
        {
            if (_playerTransform == null) return false;

            Transform eye = _eyeTransform != null ? _eyeTransform : transform;
            Vector3 dirToPlayer = _playerTransform.position - eye.position;
            float distance = dirToPlayer.magnitude;

            if (distance > _data.sightRange) return false;

            float angle = Vector3.Angle(eye.forward, dirToPlayer);
            if (angle > _data.sightAngle * 0.5f) return false;

            // Line-of-sight check
            if (Physics.Raycast(eye.position, dirToPlayer.normalized, out RaycastHit hit, distance, _obstacleLayers))
            {
                // Hit something between us and the player = can't see
                return false;
            }

            return true;
        }

        /// <summary>Checks if the player is within hearing range.</summary>
        protected bool CanHearPlayer()
        {
            if (_playerTransform == null) return false;

            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            if (distance > _data.hearingRange) return false;

            // Players are noisier when sprinting
            var playerController = _playerTransform.GetComponent<Player.PlayerController>();
            if (playerController != null && playerController.IsSprinting)
            {
                return true;
            }

            // Crouching players are quieter (reduce hearing range)
            if (playerController != null && playerController.IsCrouching)
            {
                return distance <= _data.hearingRange * 0.3f;
            }

            return distance <= _data.hearingRange * 0.6f;
        }

        #endregion

        #region Combat

        protected virtual void PerformAttack()
        {
            if (_animator != null)
                _animator.SetTrigger(Constants.AnimParams.ATTACK);
            PlaySound(_data.attackSound);

            // Damage the player if still in range and visible
            if (_playerTransform == null) return;
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= _data.attackRange * 1.1f)
            {
                if (_playerTransform.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_data.attackDamage, transform.position);
                }
            }
        }

        /// <summary>IDamageable: Receives damage from player weapons.</summary>
        public virtual void TakeDamage(float amount, Vector3 hitPoint, bool isHeadshot = false)
        {
            if (!IsAlive) return;

            float finalDamage = isHeadshot ? amount * Constants.Player.HEADSHOT_MULTIPLIER : amount;
            _currentHealth -= finalDamage;

            PlaySound(_data.hitSound);

            // Stagger check
            if (DamageSystem.ShouldStagger(finalDamage, _data.staggerThreshold) &&
                _currentState != EnemyState.Staggered)
            {
                SetState(EnemyState.Staggered);
            }

            // If we weren't already chasing, now we know where the player is
            if (_currentState == EnemyState.Idle || _currentState == EnemyState.Wandering)
            {
                if (_playerTransform != null)
                    _lastKnownPlayerPosition = _playerTransform.position;
                SetState(EnemyState.Chasing);
            }

            if (_currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>IDamageable: Enemies cannot be healed.</summary>
        public void Heal(float amount) { /* Enemies don't heal */ }

        protected virtual void Die()
        {
            SetState(EnemyState.Dead);

            if (_animator != null)
                _animator.SetBool(Constants.AnimParams.IS_DEAD, true);
            PlaySound(_data.deathSound);

            // Disable collider so player can walk through
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;

            // Notify event bus
            EventBus.EnemyKilled(_enemyId);

            float destroyDelay = _data.corpseLifetime;
            EnemyCharacterVisual visual = GetComponent<EnemyCharacterVisual>();
            if (visual != null)
                destroyDelay = visual.TriggerDeathVanish();

            Destroy(gameObject, Mathf.Max(0.7f, destroyDelay));

            Debug.Log($"[EnemyBase] {_data.enemyName} died. ID: {_enemyId}");
        }

        #endregion

        #region Navigation Helpers

        protected void WanderToRandomPoint()
        {
            if (!HasActiveNavMeshAgent())
                return;

            Vector3 randomDirection = Random.insideUnitSphere * _data.wanderRadius;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, _data.wanderRadius, NavMesh.AllAreas))
            {
                _agent.SetDestination(navHit.position);
            }
        }

        #endregion

        #region Audio

        protected void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        protected void PlayRandomSound(AudioClip[] clips)
        {
            if (clips.Length == 0) return;
            PlaySound(clips[Random.Range(0, clips.Length)]);
        }

        #endregion

        #region Animator

        protected virtual void UpdateAnimator()
        {
            if (_animator == null) return;

            float speed = HasActiveNavMeshAgent() ? _agent.velocity.magnitude / _data.chaseSpeed : 0f;
            _animator.SetFloat(Constants.AnimParams.SPEED, speed);
            _animator.SetBool(Constants.AnimParams.IS_CHASING, _currentState == EnemyState.Chasing);
        }

        private bool HasActiveNavMeshAgent()
        {
            return _agent != null && _agent.enabled && _agent.isOnNavMesh;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos || _data == null) return;

            Transform eye = _eyeTransform != null ? _eyeTransform : transform;

            // Sight cone
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Vector3 leftBound = Quaternion.Euler(0, -_data.sightAngle / 2f, 0) * eye.forward * _data.sightRange;
            Vector3 rightBound = Quaternion.Euler(0, _data.sightAngle / 2f, 0) * eye.forward * _data.sightRange;
            Gizmos.DrawLine(eye.position, eye.position + leftBound);
            Gizmos.DrawLine(eye.position, eye.position + rightBound);

            // Hearing range
            Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
            Gizmos.DrawWireSphere(transform.position, _data.hearingRange);

            // Attack range
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _data.attackRange);
        }

        #endregion
    }
}
