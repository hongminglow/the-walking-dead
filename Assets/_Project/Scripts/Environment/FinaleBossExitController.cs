// ============================================================
// File:        FinaleBossExitController.cs
// Namespace:   TWD.Environment
// Description: Keeps the finale escape locked until the boss is
//              defeated, then raises the gate and re-enables the
//              exit trigger.
// Author:      The Walking Dead Team
// Created:     2026-04-10
// ============================================================

using UnityEngine;
using TWD.Enemies;

namespace TWD.Environment
{
    /// <summary>
    /// Scene-specific finale flow controller. The exit trigger stays inactive
    /// until the brute has been defeated, which makes the final arena read as
    /// an actual boss gate instead of a free escape trigger.
    /// </summary>
    public class FinaleBossExitController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ExitZone _exitZone;
        [SerializeField] private Collider _exitTrigger;
        [SerializeField] private EnemyBase _bossEnemy;
        [SerializeField] private Transform _gateVisual;

        [Header("Gate Motion")]
        [SerializeField] private Vector3 _openedGateLocalOffset = new Vector3(0f, 3f, 0f);
        [SerializeField] private float _gateMoveSpeed = 2.5f;

        [Header("Fallback Search")]
        [SerializeField] private string _bossNameHint = "Brute";

        private Vector3 _closedGateLocalPosition;
        private Vector3 _openedGateLocalPosition;
        private bool _exitUnlocked;
        private bool _gatePositionsCached;
        private bool _bossWasResolved;
        private bool _missingBossWarningLogged;

        private void Awake()
        {
            ResolveReferences();
            CacheGatePositions();
        }

        private void Start()
        {
            ResolveReferences();
            CacheGatePositions();
            ApplyExitState(ShouldExitBeUnlocked(), snapGate: true);
        }

        private void Update()
        {
            if (!_exitUnlocked && ShouldExitBeUnlocked())
            {
                ApplyExitState(true, snapGate: false);
                Debug.Log("[FinaleBossExitController] Boss defeated. Escape route is now open.");
            }

            AnimateGate();
        }

        private void ResolveReferences()
        {
            if (_exitZone == null)
            {
                var exitGo = GameObject.Find("ExitZone_Escape");
                _exitZone = exitGo != null ? exitGo.GetComponent<ExitZone>() : FindAnyObjectByType<ExitZone>(FindObjectsInactive.Include);
            }

            if (_exitTrigger == null && _exitZone != null)
                _exitTrigger = _exitZone.GetComponent<Collider>();

            if (_gateVisual == null)
            {
                var gateGo = GameObject.Find("EscapeGateVisual");
                _gateVisual = gateGo != null ? gateGo.transform : null;
            }

            if (_bossEnemy == null)
            {
                var bruteGo = GameObject.Find("BOSS_ZombieBrute");
                _bossEnemy = bruteGo != null ? bruteGo.GetComponent<EnemyBase>() : null;

                if (_bossEnemy == null)
                {
                    EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsInactive.Include);
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        if (enemies[i] == null)
                            continue;

                        if (enemies[i].name.IndexOf(_bossNameHint, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _bossEnemy = enemies[i];
                            break;
                        }
                    }
                }
            }

            if (_bossEnemy != null)
                _bossWasResolved = true;
        }

        private void CacheGatePositions()
        {
            if (_gateVisual == null || _gatePositionsCached)
                return;

            _closedGateLocalPosition = _gateVisual.localPosition;
            _openedGateLocalPosition = _closedGateLocalPosition + _openedGateLocalOffset;
            _gatePositionsCached = true;
        }

        private bool ShouldExitBeUnlocked()
        {
            if (_bossEnemy == null)
            {
                ResolveReferences();
                if (_bossEnemy == null)
                {
                    if (!_bossWasResolved && !_missingBossWarningLogged)
                    {
                        Debug.LogWarning("[FinaleBossExitController] No boss reference found. Leaving exit unlocked to avoid a softlock.");
                        _missingBossWarningLogged = true;
                    }

                    return true;
                }
            }

            return !_bossEnemy || !_bossEnemy.IsAlive || !_bossEnemy.gameObject.activeInHierarchy;
        }

        private void ApplyExitState(bool unlocked, bool snapGate)
        {
            _exitUnlocked = unlocked;

            if (_exitZone != null)
                _exitZone.enabled = unlocked;

            if (_exitTrigger != null)
                _exitTrigger.enabled = unlocked;

            if (snapGate && _gateVisual != null)
            {
                _gateVisual.localPosition = unlocked ? _openedGateLocalPosition : _closedGateLocalPosition;
            }
        }

        private void AnimateGate()
        {
            if (_gateVisual == null || !_gatePositionsCached)
                return;

            Vector3 target = _exitUnlocked ? _openedGateLocalPosition : _closedGateLocalPosition;
            _gateVisual.localPosition = Vector3.MoveTowards(
                _gateVisual.localPosition,
                target,
                _gateMoveSpeed * Time.deltaTime);
        }
    }
}
