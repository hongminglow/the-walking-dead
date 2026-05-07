using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TWD.Combat;
using TWD.Core;
using TWD.Inventory;
using TWD.Player;
using TWD.Utilities;

namespace TWD.UI
{
    public class HUDController : MonoBehaviour
    {
        private const float PickupToastDuration = 2.25f;
        private const int HotbarSlotCount = 9;

        [Header("Health")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Text _healthStatusText;
        [SerializeField] private TMP_Text _healthStatusTmpText;
        [SerializeField] private Text _healthValueText;
        [SerializeField] private TMP_Text _healthValueTmpText;
        [SerializeField] private Color _healthColorFine = new Color(0.83f, 0.16f, 0.16f, 1f);
        [SerializeField] private Color _healthColorLow = new Color(0.98f, 0.42f, 0.3f, 1f);

        [Header("Ammo")]
        [SerializeField] private Text _ammoText;
        [SerializeField] private TMP_Text _ammoTmpText;
        [SerializeField] private Text _ammoReserveText;
        [SerializeField] private TMP_Text _ammoReserveTmpText;
        [SerializeField] private Text _weaponNameText;
        [SerializeField] private TMP_Text _weaponNameTmpText;
        [SerializeField] private Image _weaponIcon;

        [Header("Interaction")]
        [SerializeField] private GameObject _interactPromptPanel;
        [SerializeField] private Text _interactPromptText;
        [SerializeField] private TMP_Text _interactPromptTmpText;

        [Header("Stamina")]
        [SerializeField] private Slider _staminaBar;
        [SerializeField] private Text _inventoryStatusText;
        [SerializeField] private TMP_Text _inventoryStatusTmpText;

        [Header("Objective")]
        [SerializeField] private Text _objectiveText;
        [SerializeField] private TMP_Text _objectiveTmpText;

        [Header("Pickup Toast")]
        [SerializeField] private GameObject _pickupToastPanel;
        [SerializeField] private Text _pickupToastText;
        [SerializeField] private TMP_Text _pickupToastTmpText;

        [Header("Crosshair")]
        [SerializeField] private GameObject _crosshair;

        [Header("Damage Overlay")]
        [SerializeField] private Image _damageOverlay;
        [SerializeField] private float _damageFlashDuration = 0.5f;

        private float _damageFlashTimer;
        private float _pickupToastTimer;
        private bool _frontDoorUnlocked;
        private CanvasGroup _pickupToastCanvasGroup;
        private PlayerController _playerController;
        private PlayerHealth _playerHealth;
        private PlayerCombat _playerCombat;
        private GameObject _hotbarPanel;
        private Image[] _hotbarSlotBackgrounds;
        private Image[] _hotbarSlotIcons;
        private Text[] _hotbarSlotLabels;
        private Text[] _hotbarSlotNumbers;

        private void Awake()
        {
            RemoveLegacyVitalsRectFix();
            EnsureRuntimeHudScaffold();
            ResolveRuntimeReferences();
        }

        private void OnEnable()
        {
            EventBus.OnPlayerHealthChanged += UpdateHealth;
            EventBus.OnPlayerStaminaChanged += UpdateStamina;
            EventBus.OnAmmoChanged += UpdateAmmo;
            EventBus.OnWeaponSwitched += UpdateWeapon;
            EventBus.OnShowInteractPrompt += ShowPrompt;
            EventBus.OnHideInteractPrompt += HidePrompt;
            EventBus.OnPlayerDamaged += OnDamageTaken;
            EventBus.OnItemPickedUp += OnItemPickedUp;
            EventBus.OnItemUsed += OnItemUsed;
            EventBus.OnItemDropped += OnItemDropped;
            EventBus.OnKeyItemObtained += OnKeyItemObtained;
            EventBus.OnDoorUnlocked += OnDoorUnlocked;
            EventBus.OnGameSaved += OnGameSaved;
            EventBus.OnGameLoaded += OnGameLoaded;
            EventBus.OnLevelComplete += OnLevelComplete;
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
            EventBus.OnItemPickedUp -= OnItemPickedUp;
            EventBus.OnItemUsed -= OnItemUsed;
            EventBus.OnItemDropped -= OnItemDropped;
            EventBus.OnKeyItemObtained -= OnKeyItemObtained;
            EventBus.OnDoorUnlocked -= OnDoorUnlocked;
            EventBus.OnGameSaved -= OnGameSaved;
            EventBus.OnGameLoaded -= OnGameLoaded;
            EventBus.OnLevelComplete -= OnLevelComplete;
        }

        private void Start()
        {
            EnsureRuntimeHudScaffold();
            ResolveRuntimeReferences();
            ResolveGameplayReferences();
            InitializeState();
            if (_interactPromptPanel != null) _interactPromptPanel.SetActive(false);
            if (_pickupToastPanel != null) _pickupToastPanel.SetActive(false);
            if (_crosshair != null) _crosshair.SetActive(false);
            if (_damageOverlay != null)
            {
                Color c = _damageOverlay.color;
                c.a = 0f;
                _damageOverlay.color = c;
            }
        }

        private void Update()
        {
            if (_playerController == null || _playerHealth == null || _playerCombat == null) ResolveGameplayReferences();
            if (_crosshair != null && _playerController != null) _crosshair.SetActive(_playerController.IsAiming);
            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer -= Time.deltaTime;
                if (_damageOverlay != null)
                {
                    Color c = _damageOverlay.color;
                    c.a = Mathf.Clamp01(_damageFlashTimer / _damageFlashDuration) * 0.4f;
                    _damageOverlay.color = c;
                }
            }

            if (_pickupToastTimer > 0f)
            {
                _pickupToastTimer -= Time.deltaTime;
                if (_pickupToastCanvasGroup != null)
                    _pickupToastCanvasGroup.alpha = Mathf.Clamp01((_pickupToastTimer / PickupToastDuration) * 1.35f);
            }
            else if (_pickupToastPanel != null && _pickupToastPanel.activeSelf)
            {
                _pickupToastPanel.SetActive(false);
            }
        }

        private void UpdateHealth(float health)
        {
            if (_healthBar != null)
            {
                _healthBar.maxValue = Constants.Player.MAX_HEALTH;
                _healthBar.value = Mathf.Clamp(health, 0f, Constants.Player.MAX_HEALTH);
            }

            if (_healthBarFill != null)
                _healthBarFill.color = Color.Lerp(_healthColorLow, _healthColorFine, health / Constants.Player.MAX_HEALTH);

            SetText(_healthValueText, _healthValueTmpText, $"{Mathf.CeilToInt(health):000}");
            UpdateHealthStatusText(health);
        }

        private void UpdateStamina(float stamina)
        {
            if (_staminaBar == null) return;
            _staminaBar.maxValue = Constants.Player.MAX_STAMINA;
            _staminaBar.value = Mathf.Clamp(stamina, 0f, Constants.Player.MAX_STAMINA);
            _staminaBar.gameObject.SetActive(true);
        }

        private void UpdateAmmo(int current, int max)
        {
            SetText(_ammoText, _ammoTmpText, max > 0 ? BuildAmmoPips(current, max) : string.Empty);
            bool usesAmmo = _playerCombat != null && _playerCombat.CurrentWeapon != null && _playerCombat.CurrentWeapon.UsesAmmo;
            SetText(_ammoReserveText, _ammoReserveTmpText, usesAmmo ? $"{current:00}/{max:00}" : "MELEE");
        }

        private void UpdateWeapon(string weaponName)
        {
            SetText(_weaponNameText, _weaponNameTmpText, string.IsNullOrWhiteSpace(weaponName) ? "UNARMED" : weaponName);
            RefreshHotbar();
        }

        private void ShowPrompt(string text)
        {
            if (_interactPromptPanel != null) _interactPromptPanel.SetActive(true);
            SetText(_interactPromptText, _interactPromptTmpText, text);
        }

        private void HidePrompt()
        {
            if (_interactPromptPanel != null) _interactPromptPanel.SetActive(false);
        }

        private void OnDamageTaken(float amount)
        {
            _damageFlashTimer = _damageFlashDuration;
            if (_damageOverlay == null) return;
            Color c = _damageOverlay.color;
            c.a = 0.4f;
            _damageOverlay.color = c;
        }

        private void OnItemPickedUp(string itemName)
        {
            UpdateInventoryStatus();
            ShowPickupToast($"+ {itemName.ToUpperInvariant()}");
            RefreshObjective();
        }

        private void OnItemUsed(string itemName)
        {
            UpdateInventoryStatus();
            ShowPickupToast($"USED {itemName.ToUpperInvariant()}");
        }

        private void OnItemDropped(string itemName)
        {
            UpdateInventoryStatus();
            ShowPickupToast($"DROPPED {itemName.ToUpperInvariant()}");
        }

        private void OnKeyItemObtained(string keyId)
        {
            UpdateInventoryStatus();
            ShowPickupToast($"KEY ITEM: {HumanizeToken(keyId)}");
            RefreshObjective();
        }

        private void OnDoorUnlocked(string doorId)
        {
            if (!string.IsNullOrWhiteSpace(doorId) && doorId.ToLowerInvariant().Contains("front"))
                _frontDoorUnlocked = true;
            ShowPickupToast($"UNLOCKED {HumanizeToken(doorId)}");
            RefreshObjective();
        }

        private void OnGameSaved(int slot) => ShowPickupToast($"GAME SAVED  SLOT {slot}");

        private void OnGameLoaded(int slot)
        {
            ResolveGameplayReferences();
            InitializeState();
            ShowPickupToast($"SAVE LOADED  SLOT {slot}");
        }

        private void OnLevelComplete(string sceneName)
        {
            SetText(_objectiveText, _objectiveTmpText, "EXIT REACHED");
        }

        private void ResolveGameplayReferences()
        {
            GameObject player = GameObject.FindWithTag(Constants.Tags.PLAYER);
            if (player == null) return;
            if (_playerController == null) _playerController = player.GetComponent<PlayerController>();
            if (_playerHealth == null) _playerHealth = player.GetComponent<PlayerHealth>();
            if (_playerCombat == null) _playerCombat = player.GetComponent<PlayerCombat>();
        }

        private void InitializeState()
        {
            UpdateInventoryStatus();
            RefreshObjective();
            if (_playerHealth != null) UpdateHealth(_playerHealth.CurrentHealth);
            if (_playerController != null) UpdateStamina(_playerController.CurrentStamina);
            if (_playerCombat != null)
            {
                WeaponData weapon = _playerCombat.CurrentWeapon;
                UpdateWeapon(weapon != null ? weapon.weaponName : "Knife");
                UpdateAmmo(_playerCombat.AmmoInClip, weapon != null ? weapon.magazineSize : 0);
            }
            RefreshHotbar();
        }

        private void UpdateInventoryStatus()
        {
            if (InventoryManager.Instance == null)
            {
                SetText(_inventoryStatusText, _inventoryStatusTmpText, "INV --/24");
                return;
            }

            SetText(_inventoryStatusText, _inventoryStatusTmpText, $"INV {InventoryManager.Instance.UsedSlots:00}/{InventoryManager.Instance.SlotCount:00}");
        }

        private void RefreshObjective()
        {
            string objective = SceneManager.GetActiveScene().name switch
            {
                Constants.Scenes.LEVEL_01_HOUSE => GetLevel01Objective(),
                Constants.Scenes.LEVEL_02_STREETS => "REACH THE HOSPITAL AND FIND A WAY INSIDE",
                Constants.Scenes.LEVEL_03_HOSPITAL => "RESTORE POWER AND FIND THE SEWER MAP",
                Constants.Scenes.LEVEL_04_UNDERGROUND => "FIND THE VALVE HANDLE AND OPEN THE GATE",
                Constants.Scenes.LEVEL_05_FINALE => "SURVIVE THE BRUTE AND REACH THE EXIT",
                _ => "SURVIVE AND KEEP MOVING"
            };
            SetText(_objectiveText, _objectiveTmpText, objective);
        }

        private string GetLevel01Objective()
        {
            if (_frontDoorUnlocked) return "LEAVE THE HOUSE";
            if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem("key_house")) return "UNLOCK THE FRONT DOOR";
            return "FIND THE HOUSE KEY";
        }

        private void UpdateHealthStatusText(float health)
        {
            string status = "FINE";
            Color color = new Color(0.84f, 0.85f, 0.79f, 1f);
            if (health <= Constants.Player.HEALTH_HURT_THRESHOLD)
            {
                status = "CRITICAL";
                color = new Color(1f, 0.36f, 0.3f, 1f);
            }
            else if (health <= Constants.Player.HEALTH_FINE_THRESHOLD)
            {
                status = "HURT";
                color = new Color(0.93f, 0.63f, 0.28f, 1f);
            }

            if (_healthStatusText != null)
            {
                _healthStatusText.text = status;
                _healthStatusText.color = color;
            }

            if (_healthStatusTmpText != null)
            {
                _healthStatusTmpText.text = status;
                _healthStatusTmpText.color = color;
            }
        }

        private void ShowPickupToast(string message)
        {
            if (_pickupToastPanel == null) return;
            _pickupToastPanel.SetActive(true);
            _pickupToastTimer = PickupToastDuration;
            if (_pickupToastCanvasGroup != null) _pickupToastCanvasGroup.alpha = 1f;
            SetText(_pickupToastText, _pickupToastTmpText, message);
        }

        private void ResolveRuntimeReferences()
        {
            RectTransform vitalsPanel = FindNamedComponentInChildren<RectTransform>("VitalsPanel");
            RectTransform weaponPanel = FindNamedComponentInChildren<RectTransform>("WeaponPanel");
            RectTransform objectivePanel = FindNamedComponentInChildren<RectTransform>("ObjectivePanel");

            Slider runtimeHealthBar = FindNamedComponentInChildren<Slider>("HealthBar", vitalsPanel);
            if (runtimeHealthBar != null) _healthBar = runtimeHealthBar;
            else if (_healthBar == null) _healthBar = FindNamedComponentInChildren<Slider>("HealthBar");
            if (_healthBarFill == null && _healthBar != null && _healthBar.fillRect != null) _healthBarFill = _healthBar.fillRect.GetComponent<Image>();
            if (_healthStatusText == null) _healthStatusText = FindNamedComponentInChildren<Text>("HealthStatusText", vitalsPanel);
            if (_healthStatusTmpText == null) _healthStatusTmpText = FindNamedComponentInChildren<TMP_Text>("HealthStatusText", vitalsPanel);
            if (_healthValueText == null) _healthValueText = FindNamedComponentInChildren<Text>("HealthValueText", vitalsPanel);
            if (_healthValueTmpText == null) _healthValueTmpText = FindNamedComponentInChildren<TMP_Text>("HealthValueText", vitalsPanel);
            if (_ammoText == null) _ammoText = FindNamedComponentInChildren<Text>("WeaponAmmoText", weaponPanel) ?? FindNamedComponentInChildren<Text>("AmmoText");
            if (_ammoTmpText == null) _ammoTmpText = FindNamedComponentInChildren<TMP_Text>("WeaponAmmoText", weaponPanel) ?? FindNamedComponentInChildren<TMP_Text>("AmmoText");
            if (_ammoReserveText == null) _ammoReserveText = FindNamedComponentInChildren<Text>("WeaponReserveText", weaponPanel);
            if (_ammoReserveTmpText == null) _ammoReserveTmpText = FindNamedComponentInChildren<TMP_Text>("WeaponReserveText", weaponPanel);
            if (_weaponNameText == null) _weaponNameText = FindNamedComponentInChildren<Text>("WeaponNameText", weaponPanel);
            if (_weaponNameTmpText == null) _weaponNameTmpText = FindNamedComponentInChildren<TMP_Text>("WeaponNameText", weaponPanel);
            if (_inventoryStatusText == null) _inventoryStatusText = FindNamedComponentInChildren<Text>("InventoryStatusText", objectivePanel);
            if (_inventoryStatusTmpText == null) _inventoryStatusTmpText = FindNamedComponentInChildren<TMP_Text>("InventoryStatusText", objectivePanel);
            if (_objectiveText == null) _objectiveText = FindNamedComponentInChildren<Text>("ObjectiveText", objectivePanel);
            if (_objectiveTmpText == null) _objectiveTmpText = FindNamedComponentInChildren<TMP_Text>("ObjectiveText", objectivePanel);
            if (_interactPromptPanel == null)
            {
                RectTransform prompt = FindNamedComponentInChildren<RectTransform>("InteractPrompt");
                if (prompt != null) _interactPromptPanel = prompt.gameObject;
            }
            if (_interactPromptText == null) _interactPromptText = FindNamedComponentInChildren<Text>("InteractPromptLabel");
            if (_interactPromptTmpText == null) _interactPromptTmpText = FindNamedComponentInChildren<TMP_Text>("InteractPromptLabel");
            if (_interactPromptText == null && _interactPromptPanel != null) _interactPromptText = _interactPromptPanel.GetComponentInChildren<Text>(true);
            if (_interactPromptTmpText == null && _interactPromptPanel != null) _interactPromptTmpText = _interactPromptPanel.GetComponentInChildren<TMP_Text>(true);
            Slider runtimeStaminaBar = FindNamedComponentInChildren<Slider>("StaminaBar", vitalsPanel);
            if (runtimeStaminaBar != null) _staminaBar = runtimeStaminaBar;
            else if (_staminaBar == null) _staminaBar = FindNamedComponentInChildren<Slider>("StaminaBar");
            if (_crosshair == null)
            {
                RectTransform crosshair = FindNamedComponentInChildren<RectTransform>("Crosshair");
                if (crosshair != null) _crosshair = crosshair.gameObject;
            }
            if (_pickupToastPanel == null)
            {
                RectTransform toast = FindNamedComponentInChildren<RectTransform>("PickupToast");
                if (toast != null) _pickupToastPanel = toast.gameObject;
            }
            if (_pickupToastText == null) _pickupToastText = FindNamedComponentInChildren<Text>("PickupToastText");
            if (_pickupToastTmpText == null) _pickupToastTmpText = FindNamedComponentInChildren<TMP_Text>("PickupToastText");
            if (_pickupToastPanel != null && _pickupToastCanvasGroup == null) _pickupToastCanvasGroup = _pickupToastPanel.GetComponent<CanvasGroup>();
            if (_damageOverlay == null) _damageOverlay = FindNamedComponentInChildren<Image>("DamageOverlay");
            if (_hotbarPanel == null)
            {
                RectTransform hotbar = FindNamedComponentInChildren<RectTransform>("HotbarPanel");
                if (hotbar != null) _hotbarPanel = hotbar.gameObject;
            }
            ResolveHotbarReferences();
        }

        private void EnsureRuntimeHudScaffold()
        {
            HideLegacyHudArtifacts();
            RebuildRuntimeElement("ObjectivePanel");
            RebuildRuntimeElement("VitalsPanel");
            RebuildRuntimeElement("WeaponPanel");
            RebuildRuntimeElement("HotbarPanel");
            RebuildRuntimeElement("PickupToast");
            RebuildRuntimeElement("DamageOverlay");
            if (FindNamedComponentInChildren<RectTransform>("ObjectivePanel") == null) CreateRuntimeObjectivePanel();
            if (FindNamedComponentInChildren<RectTransform>("VitalsPanel") == null) CreateRuntimeVitalsPanel();
            if (FindNamedComponentInChildren<RectTransform>("WeaponPanel") == null) CreateRuntimeWeaponPanel();
            if (FindNamedComponentInChildren<RectTransform>("HotbarPanel") == null) CreateRuntimeHotbarPanel();
            if (FindNamedComponentInChildren<RectTransform>("PickupToast") == null) CreateRuntimePickupToast();
            if (FindNamedComponentInChildren<RectTransform>("InteractPrompt") == null) CreateRuntimeInteractPrompt();
            if (FindNamedComponentInChildren<RectTransform>("Crosshair") == null) CreateRuntimeCrosshair();
            if (FindNamedComponentInChildren<Image>("DamageOverlay") == null) CreateRuntimeDamageOverlay();
        }

        private void RemoveLegacyVitalsRectFix()
        {
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour == this)
                    continue;

                if (behaviour.GetType().FullName != "TWD.UI.HUDVitalsRectFix")
                    continue;

                behaviour.enabled = false;
                Destroy(behaviour);
            }
        }

        private void HideLegacyHudArtifacts()
        {
            Transform health = transform.Find("HealthBarBg");
            if (health != null) health.gameObject.SetActive(false);
            Transform legacyHealthBar = transform.Find("HealthBar");
            if (legacyHealthBar != null) legacyHealthBar.gameObject.SetActive(false);
            Transform ammo = transform.Find("AmmoText");
            if (ammo != null) ammo.gameObject.SetActive(false);
        }

        private void RebuildRuntimeElement(string objectName)
        {
            Transform existing = transform.Find(objectName);
            if (existing != null)
                DestroyImmediate(existing.gameObject);
        }

        private void CreateRuntimeObjectivePanel()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject panel = CreatePanel("ObjectivePanel", transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(304f, 34f), new Color(0.02f, 0.03f, 0.04f, 0.2f));
            CreateText("ObjectiveHeader", panel.transform, font, "OBJ", 9, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(10f, -8f), new Vector2(24f, 14f), new Color(0.85f, 0.78f, 0.62f, 1f));
            CreateText("ObjectiveText", panel.transform, font, "FIND THE HOUSE KEY", 12, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(36f, -7f), new Vector2(196f, 16f), Color.white);
            CreateText("InventoryStatusText", panel.transform, font, "INV 00/24", 9, FontStyle.Normal, TextAnchor.UpperRight, new Vector2(234f, -8f), new Vector2(60f, 14f), new Color(0.74f, 0.74f, 0.72f, 0.95f));
        }

        private void CreateRuntimeVitalsPanel()
        {
            GameObject panel = CreatePanel("VitalsPanel", transform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(16f, 16f), new Vector2(146f, 24f), new Color(0.03f, 0.04f, 0.05f, 0.12f));
            CreateHeartIcon("HealthIcon", panel.transform, new Vector2(3f, -5f), new Color(0.95f, 0.22f, 0.22f, 0.98f));
            CreateSlider("HealthBar", panel.transform, new Vector2(18f, -6f), new Vector2(82f, 5f), new Color(0.08f, 0.08f, 0.08f, 0.3f), _healthColorFine);
            CreateStaminaIcon("StaminaIcon", panel.transform, new Vector2(4f, -15f), new Color(0.28f, 0.68f, 1f, 0.98f));
            Slider stamina = CreateSlider("StaminaBar", panel.transform, new Vector2(18f, -16f), new Vector2(82f, 5f), new Color(0.08f, 0.08f, 0.08f, 0.22f), new Color(0.22f, 0.6f, 0.98f, 1f));
            stamina.gameObject.SetActive(true);
        }

        private void CreateRuntimeWeaponPanel()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject panel = CreatePanel("WeaponPanel", transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 16f), new Vector2(164f, 44f), new Color(0.02f, 0.03f, 0.04f, 0.12f));
            CreateText("WeaponNameText", panel.transform, font, "Pistol", 10, FontStyle.Bold, TextAnchor.UpperRight, new Vector2(76f, -6f), new Vector2(74f, 12f), new Color(0.87f, 0.87f, 0.84f, 0.95f));
            CreateAmmoIcon(panel.transform);
            CreateText("WeaponAmmoText", panel.transform, font, "|||||", 12, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(26f, -18f), new Vector2(70f, 14f), new Color(0.94f, 0.9f, 0.78f, 1f));
            CreateText("WeaponReserveText", panel.transform, font, "05/12", 12, FontStyle.Bold, TextAnchor.UpperRight, new Vector2(102f, -18f), new Vector2(48f, 14f), Color.white);
        }

        private void CreateRuntimeHotbarPanel()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject panel = CreatePanel("HotbarPanel", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(504f, 56f), new Color(0f, 0f, 0f, 0f));
            _hotbarPanel = panel;
            _hotbarSlotBackgrounds = new Image[HotbarSlotCount];
            _hotbarSlotIcons = new Image[HotbarSlotCount];
            _hotbarSlotLabels = new Text[HotbarSlotCount];
            _hotbarSlotNumbers = new Text[HotbarSlotCount];

            for (int i = 0; i < HotbarSlotCount; i++)
            {
                GameObject slot = CreateUiChild($"HotbarSlot_{i}", panel.transform);
                RectTransform slotRect = slot.GetComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0f, 0.5f);
                slotRect.anchorMax = new Vector2(0f, 0.5f);
                slotRect.pivot = new Vector2(0f, 0.5f);
                slotRect.anchoredPosition = new Vector2(6f + (i * 55f), 0f);
                slotRect.sizeDelta = new Vector2(50f, 50f);

                Image bg = slot.AddComponent<Image>();
                bg.color = new Color(0.06f, 0.07f, 0.08f, 0.76f);
                _hotbarSlotBackgrounds[i] = bg;

                Outline slotOutline = slot.AddComponent<Outline>();
                slotOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
                slotOutline.effectDistance = new Vector2(1f, -1f);

                Text number = CreateText($"HotbarNumber_{i}", slot.transform, font, (i + 1).ToString(), 9, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(4f, -4f), new Vector2(16f, 12f), new Color(0.74f, 0.77f, 0.8f, 0.95f), false);
                Image icon = CreateHotbarIcon($"HotbarIcon_{i}", slot.transform);
                Text label = CreateText($"HotbarLabel_{i}", slot.transform, font, "---", 10, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(5f, -10f), new Vector2(40f, 30f), new Color(0.9f, 0.9f, 0.88f, 0.96f));
                _hotbarSlotNumbers[i] = number;
                _hotbarSlotIcons[i] = icon;
                _hotbarSlotLabels[i] = label;
            }
        }

        private static Image CreateHotbarIcon(string name, Transform parent)
        {
            GameObject iconObject = CreateUiChild(name, parent);
            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -1f);
            rect.sizeDelta = new Vector2(31f, 31f);

            Image icon = iconObject.AddComponent<Image>();
            icon.enabled = false;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return icon;
        }

        private void CreateRuntimePickupToast()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject panel = CreatePanel("PickupToast", transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(320f, 44f), new Color(0.05f, 0.06f, 0.04f, 0.88f));
            panel.AddComponent<CanvasGroup>();
            CreateText("PickupToastText", panel.transform, font, string.Empty, 15, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(20f, -9f), new Vector2(280f, 26f), new Color(0.98f, 0.9f, 0.62f, 1f));
            panel.SetActive(false);
        }

        private void CreateRuntimeInteractPrompt()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject prompt = CreatePanel("InteractPrompt", transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 84f), new Vector2(360f, 48f), new Color(0f, 0f, 0f, 0.64f));
            CreateText("InteractPromptLabel", prompt.transform, font, string.Empty, 17, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(20f, -9f), new Vector2(320f, 28f), Color.white);
            prompt.SetActive(false);
        }

        private void CreateRuntimeCrosshair()
        {
            GameObject crosshair = CreateUiChild("Crosshair", transform);
            RectTransform rect = crosshair.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(12f, 12f);
            Image image = crosshair.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.78f);
            crosshair.SetActive(false);
        }

        private void CreateRuntimeDamageOverlay()
        {
            GameObject overlay = CreateUiChild("DamageOverlay", transform);
            RectTransform rect = overlay.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = overlay.AddComponent<Image>();
            image.color = new Color(0.45f, 0f, 0f, 0f);
        }

        private void CreateAmmoIcon(Transform parent)
        {
            GameObject iconRoot = CreateUiChild("AmmoIcon", parent);
            RectTransform rootRect = iconRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = new Vector2(8f, -18f);
            rootRect.sizeDelta = new Vector2(14f, 14f);

            GameObject shell = CreateUiChild("Shell", iconRoot.transform);
            RectTransform shellRect = shell.GetComponent<RectTransform>();
            shellRect.anchorMin = new Vector2(0f, 0f);
            shellRect.anchorMax = new Vector2(0f, 0f);
            shellRect.pivot = new Vector2(0f, 0f);
            shellRect.anchoredPosition = new Vector2(2f, 2f);
            shellRect.sizeDelta = new Vector2(6f, 9f);
            Image shellImage = shell.AddComponent<Image>();
            shellImage.color = new Color(0.9f, 0.78f, 0.45f, 1f);

            GameObject tip = CreateUiChild("Tip", iconRoot.transform);
            RectTransform tipRect = tip.GetComponent<RectTransform>();
            tipRect.anchorMin = new Vector2(0f, 0f);
            tipRect.anchorMax = new Vector2(0f, 0f);
            tipRect.pivot = new Vector2(0f, 0f);
            tipRect.anchoredPosition = new Vector2(8f, 5f);
            tipRect.sizeDelta = new Vector2(3f, 4f);
            Image tipImage = tip.AddComponent<Image>();
            tipImage.color = new Color(0.97f, 0.88f, 0.62f, 1f);
        }

        private void CreateHeartIcon(string name, Transform parent, Vector2 anchoredPosition, Color color)
        {
            GameObject iconRoot = CreateUiChild(name, parent);
            RectTransform rootRect = iconRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(10f, 9f);

            CreateIconBlock("LeftLobe", iconRoot.transform, new Vector2(0f, 0f), new Vector2(4f, 3f), color);
            CreateIconBlock("RightLobe", iconRoot.transform, new Vector2(6f, 0f), new Vector2(4f, 3f), color);
            CreateIconBlock("Center", iconRoot.transform, new Vector2(2f, -2f), new Vector2(6f, 3f), color);
            CreateIconBlock("Lower", iconRoot.transform, new Vector2(1f, -4f), new Vector2(8f, 3f), color);
            CreateIconBlock("Tip", iconRoot.transform, new Vector2(3f, -6f), new Vector2(4f, 3f), color);
        }

        private void CreateStaminaIcon(string name, Transform parent, Vector2 anchoredPosition, Color color)
        {
            GameObject iconRoot = CreateUiChild(name, parent);
            RectTransform rootRect = iconRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(10f, 9f);

            CreateIconBlock("Upper", iconRoot.transform, new Vector2(4f, 0f), new Vector2(4f, 3f), color);
            CreateIconBlock("Middle", iconRoot.transform, new Vector2(2f, -3f), new Vector2(4f, 3f), color);
            CreateIconBlock("Lower", iconRoot.transform, new Vector2(5f, -6f), new Vector2(3f, 3f), color);
        }

        private void CreateIconBlock(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject block = CreateUiChild(name, parent);
            RectTransform rect = block.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = block.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private Slider CreateSlider(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color backgroundColor, Color fillColor)
        {
            GameObject background = CreateUiChild(name, parent);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 1f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.pivot = new Vector2(0f, 1f);
            bgRect.anchoredPosition = anchoredPosition;
            bgRect.sizeDelta = size;
            Image bg = background.AddComponent<Image>();
            bg.color = backgroundColor;

            GameObject fillArea = CreateUiChild("FillArea", background.transform);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            float inset = size.y <= 6f ? 1f : 2f;
            fillAreaRect.offsetMin = new Vector2(inset, inset);
            fillAreaRect.offsetMax = new Vector2(-inset, -inset);

            GameObject fill = CreateUiChild("Fill", fillArea.transform);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;

            Slider slider = background.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 100f;
            slider.targetGraphic = bg;
            slider.fillRect = fillRect;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject panel = CreateUiChild(name, parent);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private Text CreateText(string name, Transform parent, Font font, string content, int fontSize, FontStyle fontStyle, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size, Color color, bool outlined = true)
        {
            GameObject textObject = CreateUiChild(name, parent);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.text = content;
            if (outlined)
            {
                Outline outline = textObject.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
                outline.effectDistance = new Vector2(1f, -1f);
            }
            return text;
        }

        private static GameObject CreateUiChild(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private T FindNamedComponentInChildren<T>(string objectName, Transform root = null) where T : Component
        {
            Transform searchRoot = root != null ? root : transform;
            T[] components = searchRoot.GetComponentsInChildren<T>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i].gameObject.name == objectName) return components[i];
            }
            return null;
        }

        private static void SetText(Text uiText, TMP_Text tmpText, string value)
        {
            if (uiText != null) uiText.text = value;
            if (tmpText != null) tmpText.text = value;
        }

        private static string HumanizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "UNKNOWN";
            return value.Replace("_", " ").ToUpperInvariant();
        }

        private void ResolveHotbarReferences()
        {
            if (_hotbarPanel == null)
                return;

            if (_hotbarSlotBackgrounds == null || _hotbarSlotBackgrounds.Length != HotbarSlotCount)
                _hotbarSlotBackgrounds = new Image[HotbarSlotCount];

            if (_hotbarSlotLabels == null || _hotbarSlotLabels.Length != HotbarSlotCount)
                _hotbarSlotLabels = new Text[HotbarSlotCount];

            if (_hotbarSlotIcons == null || _hotbarSlotIcons.Length != HotbarSlotCount)
                _hotbarSlotIcons = new Image[HotbarSlotCount];

            if (_hotbarSlotNumbers == null || _hotbarSlotNumbers.Length != HotbarSlotCount)
                _hotbarSlotNumbers = new Text[HotbarSlotCount];

            for (int i = 0; i < HotbarSlotCount; i++)
            {
                if (_hotbarSlotBackgrounds[i] == null)
                    _hotbarSlotBackgrounds[i] = FindNamedComponentInChildren<Image>($"HotbarSlot_{i}", _hotbarPanel.transform);
                if (_hotbarSlotLabels[i] == null)
                    _hotbarSlotLabels[i] = FindNamedComponentInChildren<Text>($"HotbarLabel_{i}", _hotbarPanel.transform);
                if (_hotbarSlotIcons[i] == null)
                    _hotbarSlotIcons[i] = FindNamedComponentInChildren<Image>($"HotbarIcon_{i}", _hotbarPanel.transform);
                if (_hotbarSlotNumbers[i] == null)
                    _hotbarSlotNumbers[i] = FindNamedComponentInChildren<Text>($"HotbarNumber_{i}", _hotbarPanel.transform);
            }
        }

        private void RefreshHotbar()
        {
            if (_playerCombat == null || _hotbarSlotBackgrounds == null || _hotbarSlotLabels == null)
                return;

            for (int i = 0; i < HotbarSlotCount; i++)
            {
                if (_hotbarSlotBackgrounds[i] == null || _hotbarSlotLabels[i] == null)
                    continue;

                WeaponData weapon = _playerCombat.GetHotbarWeapon(i);
                bool isSelected = _playerCombat.SelectedHotbarIndex == i;
                Sprite weaponIcon = weapon != null ? weapon.icon : null;

                _hotbarSlotBackgrounds[i].color = isSelected
                    ? new Color(0.76f, 0.18f, 0.14f, 0.94f)
                    : weapon != null
                        ? new Color(0.1f, 0.12f, 0.14f, 0.82f)
                        : new Color(0.05f, 0.06f, 0.07f, 0.55f);

                if (_hotbarSlotIcons != null && _hotbarSlotIcons[i] != null)
                {
                    _hotbarSlotIcons[i].sprite = weaponIcon;
                    _hotbarSlotIcons[i].enabled = weaponIcon != null;
                    _hotbarSlotIcons[i].color = isSelected
                        ? new Color(1f, 0.96f, 0.88f, 1f)
                        : new Color(0.88f, 0.9f, 0.9f, weaponIcon != null ? 0.96f : 0f);
                }

                _hotbarSlotLabels[i].text = weapon == null ? "---" : weaponIcon != null ? string.Empty : GetHotbarLabel(weapon);
                _hotbarSlotLabels[i].color = isSelected
                    ? new Color(1f, 0.96f, 0.88f, 1f)
                    : weapon != null
                        ? new Color(0.88f, 0.9f, 0.9f, 0.96f)
                        : new Color(0.42f, 0.46f, 0.5f, 0.85f);

                if (_hotbarSlotNumbers[i] != null)
                {
                    _hotbarSlotNumbers[i].color = isSelected
                        ? new Color(1f, 0.93f, 0.7f, 1f)
                        : new Color(0.74f, 0.77f, 0.8f, weapon != null ? 0.96f : 0.65f);
                }
            }
        }

        private static string GetHotbarLabel(WeaponData weapon)
        {
            if (weapon == null || string.IsNullOrWhiteSpace(weapon.weaponName))
                return "---";

            string upperName = weapon.weaponName.ToUpperInvariant();

            if (upperName.Contains("KNIFE"))
                return "KNF";
            if (upperName.Contains("PISTOL"))
                return "PST";
            if (upperName.Contains("SHOTGUN"))
                return "SGN";
            if (upperName.Contains("WRENCH"))
                return "WRN";

            return upperName.Length <= 3 ? upperName : upperName.Substring(0, 3);
        }

        private static string BuildAmmoPips(int current, int max)
        {
            if (max <= 0 || current <= 0)
                return "-";

            int pipCount = Mathf.Clamp(current, 0, 6);
            if (max > 6)
            {
                float normalized = (float)current / max;
                pipCount = Mathf.Clamp(Mathf.CeilToInt(normalized * 6f), 0, 6);
            }

            return new string('|', pipCount);
        }
    }
}
