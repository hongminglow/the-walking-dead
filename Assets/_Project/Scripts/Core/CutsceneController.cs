// ============================================================
// File:        CutsceneController.cs
// Namespace:   TWD.Core
// Description: Lightweight Timeline cutscene wrapper with
//              skip support, one-shot playback, and state sync.
// Author:      The Walking Dead Team
// Created:     2026-04-09
// ============================================================

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using TWD.Utilities;

namespace TWD.Core
{
    /// <summary>
    /// Wraps a PlayableDirector so cutscenes can safely enter/exit
    /// gameplay state and be skipped without bespoke scene code.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public class CutsceneController : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string _cutsceneId = "cutscene_intro";

        [Header("Playback")]
        [SerializeField] private bool _playOnStart;
        [SerializeField] private bool _playOnlyOnce = true;
        [SerializeField] private bool _skippable = true;

        [Header("Completion")]
        [SerializeField] private string _nextSceneName;
        [SerializeField] private bool _loadNextSceneOnComplete;
        [SerializeField] private UnityEvent _onCutsceneComplete;

        private PlayableDirector _director;
        private Player.PlayerController _playerController;
        private bool _isPlaying;
        private bool _skipHandled;

        private string SaveFlagKey => $"cutscene::{_cutsceneId}::played";

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
        }

        private void OnEnable()
        {
            if (_director != null)
            {
                _director.stopped += OnDirectorStopped;
            }
        }

        private void OnDisable()
        {
            if (_director != null)
            {
                _director.stopped -= OnDirectorStopped;
            }
        }

        private void Start()
        {
            if (_playOnStart && (!_playOnlyOnce || !HasPlayedBefore()))
            {
                Play();
            }
        }

        private void Update()
        {
            if (!_isPlaying || !_skippable || _skipHandled)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Skip();
            }
        }

        public void Play()
        {
            if (_director == null || _isPlaying)
            {
                return;
            }

            _playerController = FindFirstObjectByType<Player.PlayerController>();
            _playerController?.SetInputEnabled(false);

            GameManager.Instance.SetState(GameState.Cutscene);
            EventBus.CutsceneStarted(_cutsceneId);

            _skipHandled = false;
            _isPlaying = true;
            _director.time = 0d;
            _director.Play();
        }

        public void Skip()
        {
            if (!_isPlaying || _director == null)
            {
                return;
            }

            _skipHandled = true;
            _director.time = _director.duration;
            _director.Evaluate();
            _director.Stop();
        }

        private void OnDirectorStopped(PlayableDirector director)
        {
            if (!_isPlaying)
            {
                return;
            }

            _isPlaying = false;

            if (_playOnlyOnce)
            {
                PlayerPrefs.SetInt(SaveFlagKey, 1);
                PlayerPrefs.Save();
            }

            _playerController?.SetInputEnabled(true);
            _playerController = null;

            EventBus.CutsceneEnded(_cutsceneId);
            GameManager.Instance.SetState(GameState.Playing);
            _onCutsceneComplete?.Invoke();

            if (_loadNextSceneOnComplete && !string.IsNullOrEmpty(_nextSceneName))
            {
                GameManager.Instance.CompleteLevel(_nextSceneName);
            }
        }

        private bool HasPlayedBefore()
        {
            return PlayerPrefs.GetInt(SaveFlagKey, 0) == 1;
        }
    }
}
