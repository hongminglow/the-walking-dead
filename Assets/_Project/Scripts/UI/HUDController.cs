// ============================================================
// File:        HUDController.cs
// Namespace:   TWD.UI
// Description: Manages the in-game HUD — health bar, ammo count,
//              weapon icon, interaction prompts, and status effects.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TWD.Core;
using TWD.Player;

namespace TWD.UI
{
    /// <summary>
    /// Controls the in-game HUD overlay. Updates health bar, ammo count,
    /// weapon display, and interaction prompts via EventBus subscriptions.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Health")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Color _healthColorFine = new Color(0.545f, 0f, 0f, 1f);     // #8B0000
        [SerializeField] private Color _healthColorLow = new Color(1f, 0.267f, 0.267f, 1f);   // #FF4444

        [Header("Ammo")]
        [SerializeField] private Text _ammoText;
        [SerializeField] private Image _weaponIcon;

        [Header("Interaction")]
        [SerializeField] private GameObject _interactPromptPanel;
        [SerializeField] private Text _interactPromptText;

        [Header("Stamina")]
        [SerializeField] private Slider _staminaBar;

        [Header("Crosshair")]
        [SerializeField] private GameObject _crosshair;

        [Header("Damage Overlay")]
        [SerializeField] private Image _damageOverlay;
        [SerializeField] private float _damageFlashDuration = 0.5f;

        #endregion

        #region Private Fields

        private float _damageFlashTimer;
        private PlayerController _playerController;

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            EventBus.OnPlayerHealthChanged += UpdateHealth;
            EventBus.OnPlayerStaminaChanged += UpdateStamina;
            EventBus.OnAmmoChanged += UpdateAmmo;
            EventBus.OnWeaponSwitched += UpdateWeapon;
            EventBus.OnShowInteractPrompt += ShowPrompt;
            EventBus.OnHideInteractPrompt += HidePrompt;
            EventBus.OnPlayerDamaged += OnDamageTaken;
        }

        private void OnDisable()
        {
            EventBus.OnPlayerHealthChanged -= UpdateHealth;
            EventBus.OnPlayerStaminaChanged -= UpdateStamina;
            EventBus.OnAmmoChanged -= UpdateAmmo;
            EventBus.OnWeaponSwitched -= UpdateWeapon;
            EventBus.OnShowInteractPrompt -= ShowPrompt;
            EventBus.OnHideInteractPrompt -= HidePrompt;
            EventBus.OnPlayerDamaged -= OnDamageTaken;
        }

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerController = player.GetComponent<PlayerController>();

            if (_damageOverlay != null)
            {
                var c = _damageOverlay.color;
                c.a = 0f;
                _damageOverlay.color = c;
            }
        }

        private void Update()
        {
            // Crosshair: visible only when aiming
            if (_crosshair != null && _playerController != null)
                _crosshair.SetActive(_playerController.IsAiming);

            // Damage overlay fade
            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer -= Time.deltaTime;
                if (_damageOverlay != null)
                {
                    float alpha = Mathf.Clamp01(_damageFlashTimer / _damageFlashDuration) * 0.4f;
                    var c = _damageOverlay.color;
                    c.a = alpha;
                    _damageOverlay.color = c;
                }
            }
        }

        #endregion

        #region Health

        private void UpdateHealth(float health)
        {
            if (_healthBar != null)
            {
                _healthBar.value = health / Utilities.Constants.Player.MAX_HEALTH;
            }

            if (_healthBarFill != null)
            {
                float healthPercent = health / Utilities.Constants.Player.MAX_HEALTH;
                _healthBarFill.color = Color.Lerp(_healthColorLow, _healthColorFine, healthPercent);
            }
        }

        #endregion

        #region Stamina

        private void UpdateStamina(float stamina)
        {
            if (_staminaBar != null)
            {
                _staminaBar.value = stamina / Utilities.Constants.Player.MAX_STAMINA;

                // Hide stamina bar when full
                _staminaBar.gameObject.SetActive(stamina < Utilities.Constants.Player.MAX_STAMINA);
            }
        }

        #endregion

        #region Ammo

        private void UpdateAmmo(int current, int max)
        {
            if (_ammoText != null)
            {
                _ammoText.text = $"{current} / {max}";
            }
        }

        private void UpdateWeapon(string weaponName)
        {
            // TODO: Update weapon icon based on weapon name
            Debug.Log($"[HUD] Weapon switched to: {weaponName}");
        }

        #endregion

        #region Interaction Prompt

        private void ShowPrompt(string text)
        {
            if (_interactPromptPanel != null)
            {
                _interactPromptPanel.SetActive(true);
            }

            if (_interactPromptText != null)
            {
                _interactPromptText.text = text;
            }
        }

        private void HidePrompt()
        {
            if (_interactPromptPanel != null)
            {
                _interactPromptPanel.SetActive(false);
            }
        }

        #endregion

        #region Crosshair

        /// <summary>Shows or hides the crosshair (visible only when aiming).</summary>
        public void SetCrosshairVisible(bool visible)
        {
            if (_crosshair != null)
            {
                _crosshair.SetActive(visible);
            }
        }

        #endregion

        #region Damage Overlay

        private void OnDamageTaken(float amount)
        {
            _damageFlashTimer = _damageFlashDuration;
            if (_damageOverlay != null)
            {
                var c = _damageOverlay.color;
                c.a = 0.4f;
                _damageOverlay.color = c;
            }
        }

        #endregion
    }
}
