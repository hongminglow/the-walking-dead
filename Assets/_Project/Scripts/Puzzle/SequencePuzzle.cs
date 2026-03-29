// ============================================================
// File:        SequencePuzzle.cs
// Namespace:   TWD.Puzzle
// Description: Sequence-based puzzle where the player must
//              activate elements in the correct order.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace TWD.Puzzle
{
    /// <summary>
    /// Sequence puzzle. Player interacts with multiple elements
    /// in the correct order to solve. Wrong order resets progress.
    /// </summary>
    public class SequencePuzzle : PuzzleBase
    {
        #region Serialized Fields

        [Header("Sequence")]
        [SerializeField] private int[] _correctSequence;
        [SerializeField] private bool _resetOnWrong = true;

        #endregion

        #region Private Fields

        private List<int> _playerInput = new List<int>();

        #endregion

        public override string InteractPrompt =>
            _isCompleted ? "[Completed]" : $"[E] {_puzzleName}";

        protected override void OnPuzzleInteract()
        {
            // This is called from individual sequence elements
            // via SubmitInput() — not directly from player
        }

        /// <summary>
        /// Submit an input value. Called by sequence puzzle elements
        /// (e.g., buttons, switches) when the player activates them.
        /// </summary>
        public void SubmitInput(int value)
        {
            if (_isCompleted) return;

            _playerInput.Add(value);

            int currentIndex = _playerInput.Count - 1;

            // Check if current input matches expected
            if (_correctSequence[currentIndex] != value)
            {
                // Wrong input
                OnPuzzleFailed();
                if (_resetOnWrong)
                {
                    _playerInput.Clear();
                    Debug.Log("[SequencePuzzle] Wrong! Sequence reset.");
                }
                return;
            }

            // Check if sequence is complete
            if (_playerInput.Count >= _correctSequence.Length)
            {
                CompletePuzzle();
                Debug.Log("[SequencePuzzle] Correct sequence!");
            }
            else
            {
                Debug.Log($"[SequencePuzzle] Progress: {_playerInput.Count}/{_correctSequence.Length}");
            }
        }

        /// <summary>Resets the player's input progress.</summary>
        public void ResetProgress()
        {
            _playerInput.Clear();
        }
    }
}
