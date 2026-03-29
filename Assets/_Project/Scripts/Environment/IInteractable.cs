// ============================================================
// File:        IInteractable.cs
// Namespace:   TWD.Environment
// Description: Interface for all interactable objects in the world.
//              Doors, pickups, puzzles, switches, etc.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

namespace TWD.Environment
{
    /// <summary>
    /// Contract for all world objects the player can interact with.
    /// The PlayerInteraction raycast system calls these methods.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// The text shown in the UI prompt, e.g. "[E] Pick Up" or "[E] Open Door".
        /// </summary>
        string InteractPrompt { get; }

        /// <summary>
        /// Whether this object can currently be interacted with.
        /// For example, a door might not be interactable while animating.
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// Called when the player presses the interact button while looking at this object.
        /// </summary>
        void Interact();

        /// <summary>
        /// Called when the player's raycast first hits this object (for UI prompt).
        /// </summary>
        void OnLookAt();

        /// <summary>
        /// Called when the player's raycast stops hitting this object (hide UI prompt).
        /// </summary>
        void OnLookAway();
    }
}
