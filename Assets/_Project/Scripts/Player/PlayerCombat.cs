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
using TWD.Inventory;
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
        private int _reserveAmmo;
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

        /// <summary>Reserve ammo not in magazine.</summary>
        public int ReserveAmmo => _reserveAmmo;

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

            if (_muzzlePoint == null)
                _muzzlePoint = RuntimeSceneResolver.FindChildRecursive(transform, "MuzzlePoint");

            if (_shootableLayers.value == 0)
                _shootableLayers = RuntimeSceneResolver.MaskFromLayers(Constants.Layers.ENEMY);

            if (_currentWeaponData == null)
                _currentWeaponData = RuntimeSceneResolver.FindWeaponById("weapon_pistol") ??
                                     RuntimeSceneResolver.FindWeaponByType(WeaponType.Pistol, "pistol");

            if (_meleeWeaponData == null)
                _meleeWeaponData = RuntimeSceneResolver.FindWeaponById("weapon_knife") ??
                                   RuntimeSceneResolver.FindWeaponByType(WeaponType.Melee, "knife");

            if (_currentWeaponData != null && _currentAmmoInClip <= 0)
                _currentAmmoInClip = _currentWeaponData.magazineSize;

            UpdateAmmoUI();
        }

        private void Update()
        {
            if (_fireTimer > 0f)
                _fireTimer -= Time.deltaTime;

            HandleReloadTimer();

            if (GameManager.Instance.IsPlaying)
                ReadCombatInput();
        }

        #endregion

        #region Direct Input Polling

        private void ReadCombatInput()
        {
            var mouse = Mouse.current;
            var kb = Keyboard.current;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame && _controller != null && _controller.IsAiming)
                TryFire();

            if (kb != null && kb.rKey.wasPressedThisFrame)
                TryReload();

            if (kb != null && kb.vKey.wasPressedThisFrame)
                PerformMelee();
        }

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

            // Check reserve ammo first; if none, try consuming from inventory
            if (_reserveAmmo <= 0)
            {
                _reserveAmmo += ConsumeAmmoFromInventory(_currentWeaponData.ammoType);
            }

            if (_reserveAmmo <= 0)
            {
                Debug.Log("[PlayerCombat] No ammo available to reload!");
                return;
            }

            _isReloading = true;
            _reloadTimer = _currentWeaponData.reloadTime;
            _canFire = false;

            _playerAnimator?.TriggerReload();
            PlaySound(_currentWeaponData.reloadSound);

            Debug.Log($"[PlayerCombat] Reloading {_currentWeaponData.weaponName}...");
        }

        private int ConsumeAmmoFromInventory(AmmoType ammoType)
        {
            if (InventoryManager.Instance == null) return 0;

            var slots = InventoryManager.Instance.Slots;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty) continue;
                var item = slots[i].ItemData;
                if (item.itemType == ItemType.Ammo && item.ammoType == ammoType)
                {
                    int amount = item.ammoAmount * slots[i].Quantity;
                    InventoryManager.Instance.RemoveItem(i, slots[i].Quantity);
                    return amount;
                }
            }
            return 0;
        }

        private void HandleReloadTimer()
        {
            if (!_isReloading) return;

            _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0f)
            {
                int needed = _currentWeaponData.magazineSize - _currentAmmoInClip;
                int toLoad = Mathf.Min(needed, _reserveAmmo);
                _currentAmmoInClip += toLoad;
                _reserveAmmo -= toLoad;

                _isReloading = false;
                _canFire = true;

                UpdateAmmoUI();
                EventBus.WeaponReloaded();

                Debug.Log($"[PlayerCombat] Reload complete. Ammo: {_currentAmmoInClip} (reserve: {_reserveAmmo})");
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
            _reserveAmmo = 0;
            _isReloading = false;
            _canFire = true;
            _fireTimer = 0f;

            UpdateAmmoUI();
            EventBus.WeaponSwitched(weaponData.weaponName);

            Debug.Log($"[PlayerCombat] Equipped: {weaponData.weaponName}");
        }

        /// <summary>Adds ammo to reserve pool (from inventory pickup).</summary>
        public void AddReserveAmmo(int amount)
        {
            _reserveAmmo += amount;
            UpdateAmmoUI();
            Debug.Log($"[PlayerCombat] Added {amount} reserve ammo. Total reserve: {_reserveAmmo}");
        }

        /// <summary>Writes the current weapon/ammo state into SaveData.</summary>
        public void WriteSaveData(SaveData data)
        {
            if (data == null || _currentWeaponData == null)
            {
                return;
            }

            data.equippedWeaponId = _currentWeaponData.weaponId;

            if (_currentWeaponData.UsesAmmo)
            {
                data.ammoCounts.Set(GetReserveAmmoKey(_currentWeaponData.ammoType), Mathf.Max(0, _reserveAmmo));
                data.ammoCounts.Set(GetClipAmmoKey(_currentWeaponData.weaponId), Mathf.Clamp(_currentAmmoInClip, 0, _currentWeaponData.magazineSize));
            }
        }

        /// <summary>Restores weapon/ammo state from SaveData using a best-effort weapon lookup.</summary>
        public void ApplySaveData(SaveData data, System.Collections.Generic.IDictionary<string, WeaponData> weaponLookup)
        {
            if (data == null)
            {
                return;
            }

            WeaponData savedWeapon = null;

            if (!string.IsNullOrEmpty(data.equippedWeaponId))
            {
                if (weaponLookup != null)
                {
                    weaponLookup.TryGetValue(data.equippedWeaponId, out savedWeapon);
                }

                if (savedWeapon == null &&
                    _currentWeaponData != null &&
                    _currentWeaponData.weaponId == data.equippedWeaponId)
                {
                    savedWeapon = _currentWeaponData;
                }
            }

            if (savedWeapon != null)
            {
                _currentWeaponData = savedWeapon;
            }

            if (_currentWeaponData != null && _currentWeaponData.UsesAmmo)
            {
                _currentAmmoInClip = Mathf.Clamp(
                    data.ammoCounts.Get(GetClipAmmoKey(_currentWeaponData.weaponId), _currentWeaponData.magazineSize),
                    0,
                    _currentWeaponData.magazineSize);

                _reserveAmmo = Mathf.Max(
                    0,
                    data.ammoCounts.Get(GetReserveAmmoKey(_currentWeaponData.ammoType), 0));
            }
            else if (_currentWeaponData != null)
            {
                _currentAmmoInClip = _currentWeaponData.magazineSize;
                _reserveAmmo = 0;
            }

            _isReloading = false;
            _canFire = true;
            _reloadTimer = 0f;
            _fireTimer = 0f;

            UpdateAmmoUI();

            if (_currentWeaponData != null)
            {
                EventBus.WeaponSwitched(_currentWeaponData.weaponName);
            }
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

        private static string GetReserveAmmoKey(AmmoType ammoType)
        {
            return $"reserve::{ammoType}";
        }

        private static string GetClipAmmoKey(string weaponId)
        {
            return $"clip::{weaponId}";
        }

        #endregion
    }
}
