// ============================================================
// File:        MainMenuUI.cs
// Namespace:   TWD.UI
// Description: Main menu controller. Handles New Game, Continue,
//              Settings, and Quit button actions.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Core;

namespace TWD.UI
{
    /// <summary>
    /// Main menu UI controller. Wire button OnClick events to
    /// these public methods in the Unity Inspector.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _loadGamePanel;

        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button _continueButton;

        #endregion

        #region Lifecycle

        private void Start()
        {
            ShowMainPanel();

            // Disable Continue button if no saves exist
            if (_continueButton != null)
            {
                _continueButton.interactable = SaveManager.IsInitialized &&
                                               SaveManager.Instance.HasAnySave();
            }
        }

        #endregion

        #region Button Actions

        /// <summary>Starts a new game.</summary>
        public void OnNewGame()
        {
            GameManager.Instance.StartNewGame();
        }

        /// <summary>Continues from last save.</summary>
        public void OnContinue()
        {
            if (!SaveManager.IsInitialized)
            {
                Debug.LogWarning("[MainMenu] SaveManager not available. Cannot continue.");
                return;
            }

            if (!SaveManager.Instance.LoadLatest())
            {
                Debug.LogWarning("[MainMenu] No valid save found to continue.");
            }
        }

        /// <summary>Opens load game panel.</summary>
        public void OnLoadGame()
        {
            _mainPanel?.SetActive(false);
            _loadGamePanel?.SetActive(true);
        }

        /// <summary>Opens settings panel.</summary>
        public void OnSettings()
        {
            _mainPanel?.SetActive(false);
            _settingsPanel?.SetActive(true);
        }

        /// <summary>Returns to main panel from sub-panel.</summary>
        public void OnBack()
        {
            ShowMainPanel();
        }

        /// <summary>Quits the application.</summary>
        public void OnQuit()
        {
            GameManager.Instance.QuitGame();
        }

        #endregion

        #region Helpers

        private void ShowMainPanel()
        {
            _mainPanel?.SetActive(true);
            _settingsPanel?.SetActive(false);
            _loadGamePanel?.SetActive(false);
        }

        #endregion
    }
}
