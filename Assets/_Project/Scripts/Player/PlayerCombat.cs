// ============================================================
// File:        PlayerCombat.cs
// Namespace:   TWD.Player
// Description: Handles aiming, shooting, reloading, and melee.
//              Uses raycast for hitscan weapons and WeaponData SO.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;
using TWD.Combat;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Player
{
    /// <summary>
    /// Player combat controller. Manages weapon firing, reloading,
    /// and melee attacks. Works with WeaponData ScriptableObjects.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Weapon")]
        [SerializeField] private WeaponData _currentWeaponData;
        [SerializeField] private Transform _muzzlePoint;

        [Header("Raycast")]
        [SerializeField] private LayerMask _shootableLayers;
        [SerializeField] private Transform _cameraTransform;

        [Header("Melee Fallback")]
        [SerializeField] private WeaponData _meleeWeaponData;

        #endregion

        #region Private Fields

        private PlayerController _controller;
        private PlayerAnimator _playerAnimator;
        private AudioSource _audioSource;

        private int _currentAmmoInClip;
        private float _fireTimer;
        private float _reloadTimer;
        private bool _isReloading;
        private bool _canFire = true;

        #endregion

        #region Properties

        /// <summary>Currently equipped weapon data.</summary>
        public WeaponData CurrentWeapon => _currentWeaponData;

        /// <summary>Ammo count in current magazine.</summary>
        public int AmmoInClip => _currentAmmoInClip;

        /// <summary>Whether currently reloading.</summary>
        public bool IsReloading => _isReloading;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _playerAnimator = GetComponent<PlayerAnimator>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (_cameraTransform == null)
                _cameraTransform = UnityEngine.Camera.main?.transform;

            if (_currentWeaponData != null)
                _currentAmmoInClip = _currentWeaponData.magazineSize;

            UpdateAmmoUI();
        }

        private void Update()
        {
            if (_fireTimer > 0f)
                _fireTimer -= Time.deltaTime;

            HandleReloadTimer();
        }

        #endregion

        #region Input Callbacks

        /// <summary>Called by PlayerInput for Shoot action.</summary>
        public void OnShoot(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (!_controller.IsAiming || !GameManager.Instance.IsPlaying) return;

            TryFire();
        }

        /// <summary>Called by PlayerInput for Reload action.</summary>
        public void OnReload(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (!GameManager.Instance.IsPlaying) return;

            TryReload();
        }

        /// <summary>Called by PlayerInput for Melee action.</summary>
        public void OnMelee(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (!GameManager.Instance.IsPlaying) return;

            PerformMelee();
        }

        #endregion

        #region Firing

        private void TryFire()
        {
            if (_isReloading || _fireTimer > 0f || !_canFire) return;

            if (_currentWeaponData == null) return;

            // Ranged weapon ammo check
            if (_currentWeaponData.UsesAmmo)
            {
                if (_currentAmmoInClip <= 0)
                {
                    // Click — out of ammo, auto-reload
                    TryReload();
                    return;
                }

                _currentAmmoInClip--;
            }

            // Fire rate cooldown
            _fireTimer = _currentWeaponData.fireRate;

            // Perform hitscan raycast
            PerformShot();

            // Animation & audio
            _playerAnimator?.TriggerShoot();
            PlaySound(_currentWeaponData.fireSound);

            // Update UI
            UpdateAmmoUI();
            EventBus.WeaponFired();
        }

        private void PerformShot()
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _currentWeaponData.range, _shootableLayers))
            {
                // Check for headshot (collider named "Head" or tagged "Headshot")
                bool isHeadshot = hit.collider.CompareTag("Headshot") ||
                                  hit.collider.name.Contains("Head");

                float damage = _currentWeaponData.damage;
                if (isHeadshot)
                {
                    damage *= _currentWeaponData.headshotMultiplier;
                }

                // Apply damage via IDamageable
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage, hit.point, isHeadshot);
                }

                // Play impact sound
                PlaySound(_currentWeaponData.impactSound);

                Debug.Log($"[PlayerCombat] Hit {hit.collider.name} for {damage} damage. Headshot: {isHeadshot}");
            }
        }

        #endregion

        #region Reloading

        private void TryReload()
        {
            if (_isReloading) return;
            if (_currentWeaponData == null || !_currentWeaponData.UsesAmmo) return;
            if (_currentAmmoInClip >= _currentWeaponData.magazineSize) return;

            // TODO: Check if player has ammo in inventory
            // For now, just reload
            _isReloading = true;
            _reloadTimer = _currentWeaponData.reloadTime;
            _canFire = false;

            _playerAnimator?.TriggerReload();
            PlaySound(_currentWeaponData.reloadSound);

            Debug.Log($"[PlayerCombat] Reloading {_currentWeaponData.weaponName}...");
        }

        private void HandleReloadTimer()
        {
            if (!_isReloading) return;

            _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0f)
            {
                _currentAmmoInClip = _currentWeaponData.magazineSize;
                _isReloading = false;
                _canFire = true;

                UpdateAmmoUI();
                EventBus.WeaponReloaded();

                Debug.Log($"[PlayerCombat] Reload complete. Ammo: {_currentAmmoInClip}");
            }
        }

        #endregion

        #region Melee

        private void PerformMelee()
        {
            if (_fireTimer > 0f) return;

            WeaponData meleeData = _meleeWeaponData ?? _currentWeaponData;
            if (meleeData == null) return;

            _fireTimer = meleeData.fireRate;

            // Sphere cast for melee range
            Vector3 origin = transform.position + Vector3.up * 1f;
            Vector3 direction = transform.forward;

            if (Physics.SphereCast(origin, 0.3f, direction, out RaycastHit hit, meleeData.range, _shootableLayers))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(meleeData.damage, hit.point);
                }

                PlaySound(meleeData.impactSound);
                Debug.Log($"[PlayerCombat] Melee hit {hit.collider.name} for {meleeData.damage} damage.");
            }

            _playerAnimator?.TriggerMelee();
            PlaySound(meleeData.fireSound);
        }

        #endregion

        #region Weapon Management

        /// <summary>
        /// Equips a new weapon by its WeaponData ScriptableObject.
        /// </summary>
        public void EquipWeapon(WeaponData weaponData)
        {
            _currentWeaponData = weaponData;
            _currentAmmoInClip = weaponData.magazineSize;
            _isReloading = false;
            _canFire = true;
            _fireTimer = 0f;

            UpdateAmmoUI();
            EventBus.WeaponSwitched(weaponData.weaponName);

            Debug.Log($"[PlayerCombat] Equipped: {weaponData.weaponName}");
        }

        #endregion

        #region Helpers

        private void UpdateAmmoUI()
        {
            if (_currentWeaponData != null && _currentWeaponData.UsesAmmo)
            {
                EventBus.AmmoChanged(_currentAmmoInClip, _currentWeaponData.magazineSize);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        #endregion
    }
}
