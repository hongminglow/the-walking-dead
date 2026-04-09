// ============================================================
// File:        PauseMenu.cs
// Namespace:   TWD.UI
// Description: Pause menu UI controller. Handles resume, settings,
//              save, load, and quit menu functionality.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;
using TWD.Core;

namespace TWD.UI
{
    /// <summary>
    /// Pause menu controller. Toggles on Escape/Start input.
    /// Manages menu button actions: resume, save, load, quit.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI References")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _settingsPanel;

        #endregion

        #region Lifecycle

        private void Start()
        {
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        #endregion

        #region Input Callbacks

        /// <summary>Called by PlayerInput for Pause action.</summary>
        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            TogglePause();
        }

        #endregion

        #region Pause Logic

        /// <summary>Toggles the pause menu on/off.</summary>
        public void TogglePause()
        {
            if (GameManager.Instance.IsPaused)
            {
                Resume();
            }
            else if (GameManager.Instance.IsPlaying)
            {
                Pause();
            }
        }

        private void Pause()
        {
            if (_pausePanel == null)
            {
                Debug.LogWarning("[PauseMenu] Pause panel is not configured in this scene.");
                return;
            }

            _pausePanel?.SetActive(true);
            GameManager.Instance.TogglePause();
        }

        /// <summary>Resumes gameplay from pause.</summary>
        public void Resume()
        {
            _pausePanel?.SetActive(false);
            _settingsPanel?.SetActive(false);
            GameManager.Instance.TogglePause();
        }

        #endregion

        #region Menu Actions

        /// <summary>Opens the settings sub-panel.</summary>
        public void OpenSettings()
        {
            _settingsPanel?.SetActive(true);
        }

        /// <summary>Closes settings and returns to pause menu.</summary>
        public void CloseSettings()
        {
            _settingsPanel?.SetActive(false);
        }

        /// <summary>Saves the current game.</summary>
        public void SaveGame()
        {
            SaveManager.Instance.Save(0);
            Debug.Log("[PauseMenu] Game saved to slot 0.");
        }

        /// <summary>Loads the most recent save.</summary>
        public void LoadGame()
        {
            Resume();
            SaveManager.Instance.Load(0);
            Debug.Log("[PauseMenu] Game loaded from slot 0.");
        }

        /// <summary>Returns to the main menu.</summary>
        public void QuitToMainMenu()
        {
            Resume();
            GameManager.Instance.ReturnToMainMenu();
        }

        /// <summary>Quits the application.</summary>
        public void QuitGame()
        {
            GameManager.Instance.QuitGame();
        }

        #endregion
    }
}
