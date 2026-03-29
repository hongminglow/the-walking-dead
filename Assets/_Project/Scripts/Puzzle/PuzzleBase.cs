// ============================================================
// File:        PuzzleBase.cs
// Namespace:   TWD.Puzzle
// Description: Abstract base class for all puzzle types.
//              Provides completion tracking, event notifications,
//              and serialization-ready puzzle ID.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using TWD.Core;
using TWD.Environment;

namespace TWD.Puzzle
{
    /// <summary>
    /// Abstract base for all puzzle types. Subclasses implement
    /// specific puzzle logic (key-lock, sequence, push-block, etc.).
    /// </summary>
    public abstract class PuzzleBase : MonoBehaviour, IInteractable
    {
        #region Serialized Fields

        [Header("Puzzle Base")]
        [SerializeField] protected string _puzzleId;
        [SerializeField] protected string _puzzleName = "Puzzle";
        [SerializeField] protected bool _isCompleted;

        [Header("Rewards")]
        [SerializeField] protected GameObject _rewardObject;
        [SerializeField] protected GameObject _blockerToRemove;

        [Header("Audio")]
        [SerializeField] protected AudioClip _solvedSound;
        [SerializeField] protected AudioClip _failureSound;

        #endregion

        #region Protected Fields

        protected AudioSource _audioSource;

        #endregion

        #region IInteractable

        public virtual string InteractPrompt => _isCompleted ? "[Completed]" : "[E] Examine";
        public virtual bool CanInteract => !_isCompleted;

        public virtual void Interact()
        {
            if (_isCompleted) return;
            OnPuzzleInteract();
        }

        public virtual void OnLookAt()
        {
            EventBus.ShowInteractPrompt(InteractPrompt);
        }

        public virtual void OnLookAway()
        {
            EventBus.HideInteractPrompt();
        }

        #endregion

        #region Lifecycle

        protected virtual void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (string.IsNullOrEmpty(_puzzleId))
                _puzzleId = $"puzzle_{gameObject.GetInstanceID()}";
        }

        #endregion

        #region Abstract

        /// <summary>Called when the player interacts with the puzzle.</summary>
        protected abstract void OnPuzzleInteract();

        #endregion

        #region Completion

        /// <summary>Marks the puzzle as solved and triggers rewards.</summary>
        protected virtual void CompletePuzzle()
        {
            _isCompleted = true;

            // Play solved sound
            if (_audioSource != null && _solvedSound != null)
                _audioSource.PlayOneShot(_solvedSound);

            // Activate reward
            if (_rewardObject != null)
                _rewardObject.SetActive(true);

            // Remove blocker
            if (_blockerToRemove != null)
                _blockerToRemove.SetActive(false);

            EventBus.PuzzleCompleted(_puzzleId);
            Debug.Log($"[Puzzle] {_puzzleName} ({_puzzleId}) completed!");
        }

        /// <summary>Called when the player fails a puzzle attempt.</summary>
        protected virtual void OnPuzzleFailed()
        {
            if (_audioSource != null && _failureSound != null)
                _audioSource.PlayOneShot(_failureSound);
        }

        #endregion

        #region State

        /// <summary>Forces completion state (for save/load).</summary>
        public void SetCompleted(bool completed)
        {
            _isCompleted = completed;
            if (completed)
            {
                if (_rewardObject != null) _rewardObject.SetActive(true);
                if (_blockerToRemove != null) _blockerToRemove.SetActive(false);
            }
        }

        #endregion
    }
}
