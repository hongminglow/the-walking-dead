// ============================================================
// File:        ItemPickup.cs
// Namespace:   TWD.Inventory
// Description: World-space item pickup. When the player interacts,
//              adds the item to inventory and destroys the pickup.
// Author:      The Walking Dead Team
// Created:     2026-03-29
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TWD.Core;
using TWD.Combat;
using TWD.Environment;
using TWD.Player;
using TWD.Utilities;

namespace TWD.Inventory
{
    /// <summary>
    /// Place this on a world object to make it a pickupable item.
    /// Implements IInteractable for the PlayerInteraction system.
    /// </summary>
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        private const string RuntimeVisualRootName = "[PickupVisual]";

        #region Serialized Fields

        [Header("Item")]
        [SerializeField] private ItemData _itemData;
        [SerializeField] private WeaponData _weaponData;
        [SerializeField] private int _quantity = 1;

        [Header("Pickup Settings")]
        [SerializeField] private string _pickupId;
        [SerializeField] private bool _destroyOnPickup = true;

        [Header("Visual")]
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.1f;
        [SerializeField] private float _rotateSpeed = 45f;
        [SerializeField] private bool _animatePickup = true;
        [SerializeField] private bool _showWorldPrompt = true;
        [SerializeField] private float _worldPromptDistance = 2.5f;

        #endregion

        #region Private Fields

        private Vector3 _startPosition;
        private bool _hasBeenPickedUp;
        private Transform _runtimeVisualRoot;
        private Canvas _promptCanvas;
        private Transform _playerTransform;

        #endregion

        #region IInteractable

        public string InteractPrompt => $"[E] Pick Up {GetDisplayName()}";
        public bool CanInteract => !_hasBeenPickedUp && (_itemData != null || _weaponData != null);

        public void Interact()
        {
            if (_hasBeenPickedUp) return;

            if (_itemData != null)
            {
                if (InventoryManager.Instance.AddItem(_itemData, _quantity))
                {
                    Debug.Log($"[ItemPickup] Picked up: {_itemData.itemName} x{_quantity}");
                    CompletePickup();
                }
                else
                {
                    Debug.Log("[ItemPickup] Inventory full!");
                    // TODO: Show "Inventory Full" message on HUD
                }

                return;
            }

            if (_weaponData != null)
            {
                var player = GameObject.FindWithTag(Constants.Tags.PLAYER);
                if (player != null && player.TryGetComponent<PlayerCombat>(out var combat))
                {
                    combat.EquipWeapon(_weaponData);
                    Debug.Log($"[ItemPickup] Equipped weapon pickup: {_weaponData.weaponName}");
                    CompletePickup();
                }
                else
                {
                    Debug.LogWarning($"[ItemPickup] Could not equip weapon pickup '{_weaponData.weaponName}' because PlayerCombat was not found.");
                }
            }
        }

        public void OnLookAt()
        {
            if (CanInteract)
                EventBus.ShowInteractPrompt(InteractPrompt);
        }

        public void OnLookAway()
        {
            EventBus.HideInteractPrompt();
        }

        #endregion

        #region Lifecycle

        private void Start()
        {
            if (_itemData == null)
            {
                _itemData = RuntimeSceneResolver.InferItemDataFromObjectName(gameObject.name);
            }

            if (_itemData == null && _weaponData == null)
            {
                _weaponData = RuntimeSceneResolver.InferWeaponFromObjectName(gameObject.name);
            }

            if (_itemData == null && _weaponData == null)
            {
                Debug.LogWarning($"[ItemPickup] Could not infer item or weapon data for pickup '{gameObject.name}'.");
            }

            _startPosition = transform.position;

            if (string.IsNullOrEmpty(_pickupId))
                _pickupId = gameObject.name.Replace(" ", "_").ToLowerInvariant();

            EnsurePickupPresentation();
            EnsureWorldPrompt();
            _playerTransform = GameObject.FindWithTag(Constants.Tags.PLAYER)?.transform;
        }

        private void Update()
        {
            if (!_animatePickup) return;

            // Gentle bob and rotate
            float y = _startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = new Vector3(_startPosition.x, y, _startPosition.z);
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);

            UpdateWorldPrompt();
        }

        #endregion

        #region Properties

        /// <summary>Unique ID for save system tracking.</summary>
        public string PickupId => _pickupId;

        /// <summary>The item data for this pickup.</summary>
        public ItemData Item => _itemData;

        private string GetDisplayName()
        {
            if (_itemData != null)
                return _itemData.itemName;

            if (_weaponData != null)
                return _weaponData.weaponName;

            return "Item";
        }

        private void CompletePickup()
        {
            _hasBeenPickedUp = true;

            if (_destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void EnsureWorldPrompt()
        {
            if (!_showWorldPrompt || _promptCanvas != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("[PickupPrompt]", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.transform.localPosition = new Vector3(0f, 0.55f, 0f);

            _promptCanvas = canvasObject.GetComponent<Canvas>();
            _promptCanvas.renderMode = RenderMode.WorldSpace;
            _promptCanvas.worldCamera = Camera.main;
            _promptCanvas.sortingOrder = 50;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(180f, 60f);
            canvasRect.localScale = Vector3.one * 0.01f;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 20f;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(150f, 44f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.06f, 0.07f, 0.08f, 0.82f);

            CreatePromptText("KeyText", panelObject.transform, font, "[E]", 20, new Vector2(0f, 8f), FontStyle.Bold);
            CreatePromptText("ActionText", panelObject.transform, font, "PICK UP", 11, new Vector2(0f, -10f), FontStyle.Normal);

            canvasObject.SetActive(false);
        }

        private void UpdateWorldPrompt()
        {
            if (_promptCanvas == null)
            {
                return;
            }

            if (_playerTransform == null)
            {
                _playerTransform = GameObject.FindWithTag(Constants.Tags.PLAYER)?.transform;
            }

            Camera worldCamera = Camera.main;
            if (worldCamera == null || _playerTransform == null || _hasBeenPickedUp || !CanInteract)
            {
                _promptCanvas.gameObject.SetActive(false);
                return;
            }

            _promptCanvas.worldCamera = worldCamera;
            _promptCanvas.transform.forward = worldCamera.transform.forward;

            float distance = Vector3.Distance(_playerTransform.position, transform.position);
            bool shouldShow = distance <= _worldPromptDistance;
            _promptCanvas.gameObject.SetActive(shouldShow);
        }

        private static void CreatePromptText(string name, Transform parent, Font font, string content, int fontSize, Vector2 anchoredPosition, FontStyle fontStyle)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(130f, 22f);

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = content;
        }

        private void EnsurePickupPresentation()
        {
            Transform existingVisual = transform.Find(RuntimeVisualRootName);
            if (existingVisual != null)
            {
                _runtimeVisualRoot = existingVisual;
                return;
            }

            _runtimeVisualRoot = new GameObject(RuntimeVisualRootName).transform;
            _runtimeVisualRoot.SetParent(transform, false);

            switch (GetVisualKind())
            {
                case "key":
                    CreateKeyVisual(_runtimeVisualRoot);
                    break;
                case "health":
                    CreateHealthVisual(_runtimeVisualRoot);
                    break;
                case "ammo":
                    CreateAmmoVisual(_runtimeVisualRoot);
                    break;
                case "weapon":
                    CreateWeaponVisual(_runtimeVisualRoot);
                    break;
                default:
                    CreateGenericPickupVisual(_runtimeVisualRoot);
                    break;
            }

            Renderer rootRenderer = GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }
        }

        private string GetVisualKind()
        {
            if (_weaponData != null)
            {
                return "weapon";
            }

            if (_itemData == null)
            {
                return "generic";
            }

            string itemId = (_itemData.itemId ?? string.Empty).ToLowerInvariant();
            string itemName = (_itemData.itemName ?? string.Empty).ToLowerInvariant();

            if (_itemData.isKeyItem || itemId.Contains("key") || itemName.Contains("key"))
            {
                return "key";
            }

            if (itemId.Contains("health") || itemName.Contains("health") || itemName.Contains("med"))
            {
                return "health";
            }

            if (itemId.Contains("ammo") || itemName.Contains("ammo") || itemName.Contains("shell"))
            {
                return "ammo";
            }

            return "generic";
        }

        private void CreateKeyVisual(Transform parent)
        {
            Material brass = CreateLitMaterial(new Color(0.77f, 0.62f, 0.24f, 1f));
            Material accent = CreateLitMaterial(new Color(0.43f, 0.29f, 0.08f, 1f));

            CreateVisualPart("BowTop", PrimitiveType.Cube, parent, new Vector3(-0.28f, 0.16f, 0f), Quaternion.identity, new Vector3(0.34f, 0.08f, 0.08f), brass);
            CreateVisualPart("BowBottom", PrimitiveType.Cube, parent, new Vector3(-0.28f, -0.16f, 0f), Quaternion.identity, new Vector3(0.34f, 0.08f, 0.08f), brass);
            CreateVisualPart("BowLeft", PrimitiveType.Cube, parent, new Vector3(-0.43f, 0f, 0f), Quaternion.identity, new Vector3(0.08f, 0.32f, 0.08f), brass);
            CreateVisualPart("BowRight", PrimitiveType.Cube, parent, new Vector3(-0.13f, 0f, 0f), Quaternion.identity, new Vector3(0.08f, 0.32f, 0.08f), brass);
            CreateVisualPart("Stem", PrimitiveType.Cube, parent, new Vector3(0.18f, 0f, 0f), Quaternion.identity, new Vector3(0.6f, 0.08f, 0.08f), brass);
            CreateVisualPart("ToothA", PrimitiveType.Cube, parent, new Vector3(0.42f, -0.1f, 0f), Quaternion.identity, new Vector3(0.1f, 0.16f, 0.08f), accent);
            CreateVisualPart("ToothB", PrimitiveType.Cube, parent, new Vector3(0.58f, -0.06f, 0f), Quaternion.identity, new Vector3(0.12f, 0.2f, 0.08f), accent);
            parent.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }

        private void CreateHealthVisual(Transform parent)
        {
            Material body = CreateLitMaterial(new Color(0.74f, 0.74f, 0.7f, 1f));
            Material cross = CreateLitMaterial(new Color(0.72f, 0.08f, 0.08f, 1f));

            CreateVisualPart("Case", PrimitiveType.Cube, parent, new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(0.5f, 0.34f, 0.22f), body);
            CreateVisualPart("CrossV", PrimitiveType.Cube, parent, new Vector3(0f, 0f, 0.12f), Quaternion.identity, new Vector3(0.08f, 0.22f, 0.03f), cross);
            CreateVisualPart("CrossH", PrimitiveType.Cube, parent, new Vector3(0f, 0f, 0.12f), Quaternion.identity, new Vector3(0.22f, 0.08f, 0.03f), cross);
        }

        private void CreateAmmoVisual(Transform parent)
        {
            Material crate = CreateLitMaterial(new Color(0.18f, 0.22f, 0.18f, 1f));
            Material brass = CreateLitMaterial(new Color(0.72f, 0.58f, 0.22f, 1f));

            CreateVisualPart("AmmoBox", PrimitiveType.Cube, parent, new Vector3(0f, -0.02f, 0f), Quaternion.identity, new Vector3(0.42f, 0.24f, 0.24f), crate);
            CreateVisualPart("RoundA", PrimitiveType.Cylinder, parent, new Vector3(-0.1f, 0.15f, 0f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.06f, 0.14f, 0.06f), brass);
            CreateVisualPart("RoundB", PrimitiveType.Cylinder, parent, new Vector3(0f, 0.15f, 0f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.06f, 0.14f, 0.06f), brass);
            CreateVisualPart("RoundC", PrimitiveType.Cylinder, parent, new Vector3(0.1f, 0.15f, 0f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.06f, 0.14f, 0.06f), brass);
        }

        private void CreateWeaponVisual(Transform parent)
        {
            Material metal = CreateLitMaterial(new Color(0.24f, 0.24f, 0.26f, 1f));
            Material grip = CreateLitMaterial(new Color(0.14f, 0.1f, 0.08f, 1f));

            CreateVisualPart("Body", PrimitiveType.Cube, parent, new Vector3(0.08f, 0.04f, 0f), Quaternion.identity, new Vector3(0.52f, 0.12f, 0.12f), metal);
            CreateVisualPart("Barrel", PrimitiveType.Cube, parent, new Vector3(0.32f, 0.12f, 0f), Quaternion.identity, new Vector3(0.24f, 0.07f, 0.07f), metal);
            CreateVisualPart("Grip", PrimitiveType.Cube, parent, new Vector3(-0.1f, -0.16f, 0f), Quaternion.Euler(0f, 0f, 22f), new Vector3(0.12f, 0.26f, 0.1f), grip);
            parent.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        }

        private void CreateGenericPickupVisual(Transform parent)
        {
            Material body = CreateLitMaterial(new Color(0.72f, 0.72f, 0.78f, 1f));
            CreateVisualPart("Core", PrimitiveType.Sphere, parent, Vector3.zero, Quaternion.identity, new Vector3(0.28f, 0.28f, 0.28f), body);
        }

        private void CreateVisualPart(string name, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            part.layer = gameObject.layer;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private Material CreateLitMaterial(Color baseColor)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = baseColor;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.35f);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.2f);
            }

            return material;
        }

        #endregion
    }
}
