// ============================================================
// File:        ThirdPersonCamera.cs
// Namespace:   TWD.Camera
// Description: Over-the-shoulder third-person camera controller.
//              Supports aiming zoom, collision avoidance, and
//              smooth follow. Will be replaced by Cinemachine later.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.InputSystem;
using TWD.Core;
using TWD.Utilities;

namespace TWD.Camera
{
    /// <summary>
    /// RE4-style over-the-shoulder camera. Follows the player,
    /// handles mouse/gamepad look, and shifts for aim mode.
    /// This is a placeholder until Cinemachine is integrated.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Follow Target")]
        [SerializeField] private Transform _followTarget;
        [SerializeField] private float _followSpeed = 12f;

        [Header("Normal Camera")]
        [SerializeField] private Vector3 _normalOffset = new Vector3(0.5f, 1.6f, -3f);
        [SerializeField] private float _normalFOV = 60f;

        [Header("Aim Camera")]
        [SerializeField] private Vector3 _aimOffset = new Vector3(0.8f, 1.5f, -1.5f);
        [SerializeField] private float _aimFOV = 45f;

        [Header("Sensitivity")]
        [SerializeField] private float _mouseSensitivity = 0.55f;
        [SerializeField] private float _gamepadSensitivity = 100f;
        [SerializeField] private float _verticalClampMin = -40f;
        [SerializeField] private float _verticalClampMax = 70f;
        [SerializeField] private float _startingPitch = 10f;

        [Header("Collision")]
        [SerializeField] private float _collisionRadius = 0.3f;
        [SerializeField] private float _collisionPadding = 0.15f;
        [SerializeField] private float _minimumCollisionDistance = 0.45f;
        [SerializeField] private LayerMask _collisionLayers;

        [Header("Smoothing")]
        [SerializeField] private float _aimTransitionSpeed = 8f;
        [SerializeField] private float _startupLookDelay = 0.25f;

        #endregion

        #region Private Fields

        private UnityEngine.Camera _camera;
        private float _yaw;
        private float _pitch;
        private Vector2 _lookInput;
        private bool _isAiming;
        private Vector3 _currentOffset;
        private float _currentFOV;
        private float _ignoreLookInputUntil;

        #endregion

        #region Properties

        /// <summary>Horizontal rotation (yaw) of the camera.</summary>
        public float Yaw => _yaw;

        /// <summary>Vertical rotation (pitch) of the camera.</summary>
        public float Pitch => _pitch;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            if (_camera == null)
                _camera = UnityEngine.Camera.main;

            _currentOffset = _normalOffset;
            _currentFOV = _normalFOV;
        }

        private void Start()
        {
            if (_followTarget == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    _followTarget = RuntimeSceneResolver.FindChildRecursive(player.transform, "CameraTarget") ?? player.transform;
                }
            }

            _collisionLayers = RuntimeSceneResolver.MaskFromLayers(
                Constants.Layers.DEFAULT,
                Constants.Layers.GROUND,
                Constants.Layers.OBSTACLE
            );

            // Seed the opening camera from the follow target so the first frame
            // matches the authored player facing direction rather than stale editor camera data.
            _yaw = _followTarget != null ? _followTarget.eulerAngles.y : transform.eulerAngles.y;

            float initialPitch = transform.eulerAngles.x;
            if (initialPitch > 180f)
                initialPitch -= 360f;

            _pitch = Mathf.Clamp(Mathf.Approximately(initialPitch, 0f) ? _startingPitch : initialPitch, _verticalClampMin, _verticalClampMax);
            _ignoreLookInputUntil = Time.unscaledTime + _startupLookDelay;

            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_followTarget == null || !GameManager.Instance.IsPlaying) return;

            HandleLookInput();
            UpdatePosition();
            HandleCollision();
            UpdateFOV();
        }

        #endregion

        #region Input Callbacks

        /// <summary>Called by PlayerInput for Look action.</summary>
        public void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        /// <summary>Called by PlayerInput for Aim action.</summary>
        public void OnAim(InputAction.CallbackContext context)
        {
            _isAiming = context.performed;
        }

        #endregion

        #region Camera Logic

        private void HandleLookInput()
        {
            if (Time.unscaledTime < _ignoreLookInputUntil)
            {
                _lookInput = Vector2.zero;
                return;
            }

            Vector2 lookThisFrame = _lookInput;
            bool usingMouseLook = false;
            var mouse = Mouse.current;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                lookThisFrame = mouse.delta.ReadValue();
                usingMouseLook = true;
                _isAiming = mouse.rightButton.isPressed;
            }

            float sensitivity = usingMouseLook
                ? _mouseSensitivity
                : _gamepadSensitivity * Time.deltaTime;

            _yaw += lookThisFrame.x * sensitivity;
            _pitch -= lookThisFrame.y * sensitivity;
            _pitch = Mathf.Clamp(_pitch, _verticalClampMin, _verticalClampMax);
        }

        private void UpdatePosition()
        {
            // Smooth transition between normal and aim offsets
            Vector3 targetOffset = _isAiming ? _aimOffset : _normalOffset;
            _currentOffset = Vector3.Lerp(_currentOffset, targetOffset, Time.deltaTime * _aimTransitionSpeed);

            // Build rotation
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            // CameraTarget already sits at head height, so treat it as the orbit
            // pivot instead of stacking another authored vertical offset on top.
            Vector3 pivotPosition = GetPivotPosition();
            Vector3 orbitOffset = new Vector3(_currentOffset.x, 0f, _currentOffset.z);

            // Calculate desired position and clamp it against room geometry so the
            // camera does not leak outside the shell on sharp turns.
            Vector3 desiredPosition = pivotPosition + rotation * orbitOffset;
            Vector3 targetPosition = ResolveCollision(pivotPosition, desiredPosition);

            // Smooth follow
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _followSpeed);
            transform.rotation = rotation;
        }

        private void HandleCollision()
        {
            // Re-run collision against the smoothed position so we never lerp
            // through a wall for a frame when the player turns quickly.
            transform.position = ResolveCollision(GetPivotPosition(), transform.position);
        }

        private Vector3 ResolveCollision(Vector3 pivotPosition, Vector3 desiredPosition)
        {
            Vector3 direction = desiredPosition - pivotPosition;
            float desiredDistance = direction.magnitude;

            if (desiredDistance <= Mathf.Epsilon)
                return desiredPosition;

            if (Physics.SphereCast(
                    pivotPosition,
                    _collisionRadius,
                    direction.normalized,
                    out RaycastHit hit,
                    desiredDistance,
                    _collisionLayers,
                    QueryTriggerInteraction.Ignore))
            {
                float safeDistance = Mathf.Max(_minimumCollisionDistance, hit.distance - _collisionPadding);
                return pivotPosition + direction.normalized * safeDistance;
            }

            return desiredPosition;
        }

        private Vector3 GetPivotPosition()
        {
            if (_followTarget == null)
                return transform.position;

            bool useFollowTargetAsPivot =
                _followTarget.name.IndexOf("CameraTarget", System.StringComparison.OrdinalIgnoreCase) >= 0;

            return useFollowTargetAsPivot
                ? _followTarget.position
                : _followTarget.position + Vector3.up * _currentOffset.y;
        }

        private void SnapToTarget()
        {
            if (_followTarget == null)
                return;

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 orbitOffset = new Vector3(_currentOffset.x, 0f, _currentOffset.z);
            transform.position = GetPivotPosition() + rotation * orbitOffset;
            transform.rotation = rotation;
            HandleCollision();
        }

        private void UpdateFOV()
        {
            if (_camera == null) return;

            float targetFOV = _isAiming ? _aimFOV : _normalFOV;
            _currentFOV = Mathf.Lerp(_currentFOV, targetFOV, Time.deltaTime * _aimTransitionSpeed);
            _camera.fieldOfView = _currentFOV;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies a camera shake effect. Call from CameraShake or directly.
        /// </summary>
        public void ApplyShake(float intensity, float duration)
        {
            // Simplified inline shake — will be replaced by CameraShake component
            StartCoroutine(ShakeRoutine(intensity, duration));
        }

        private System.Collections.IEnumerator ShakeRoutine(float intensity, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;

                transform.localPosition += new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                intensity = Mathf.Lerp(intensity, 0f, elapsed / duration);
                yield return null;
            }
        }

        /// <summary>Sets the follow target (useful for cutscenes).</summary>
        public void SetTarget(Transform target)
        {
            _followTarget = target;
        }

        #endregion
    }
}
