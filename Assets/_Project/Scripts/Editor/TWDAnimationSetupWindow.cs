using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using TWD.Utilities;

namespace TWD.EditorTools
{
    /// <summary>
    /// Builds simple humanoid animator controllers for Quaternius characters
    /// driven by Mixamo clips, then assigns them to the existing gameplay roots.
    /// </summary>
    public class TWDAnimationSetupWindow : EditorWindow
    {
        private const string DefaultPlayerControllerPath = "Assets/_Project/Animations/Player/AnimatorControllers/AC_Player_Quaternius_Mixamo.controller";
        private const string DefaultZombieControllerPath = "Assets/_Project/Animations/Enemies/Zombie_Basic/AC_Zombie_Quaternius_Mixamo.controller";

        private Vector2 _scroll;

        private GameObject _playerTarget;
        private Avatar _playerAvatar;
        private string _playerControllerPath = DefaultPlayerControllerPath;
        private AnimationClip _playerIdle;
        private AnimationClip _playerWalk;
        private AnimationClip _playerRun;
        private AnimationClip _playerJump;
        private AnimationClip _playerAimIdle;
        private AnimationClip _playerCrouchIdle;
        private AnimationClip _playerCrouchWalk;
        private AnimationClip _playerShoot;
        private AnimationClip _playerReload;
        private AnimationClip _playerMelee;
        private AnimationClip _playerInteract;
        private AnimationClip _playerTakeDamage;
        private AnimationClip _playerDeath;

        private GameObject _zombieTarget;
        private Avatar _zombieAvatar;
        private string _zombieControllerPath = DefaultZombieControllerPath;
        private AnimationClip _zombieIdle;
        private AnimationClip _zombieWalk;
        private AnimationClip _zombieChase;
        private AnimationClip _zombieAttack;
        private AnimationClip _zombieStagger;
        private AnimationClip _zombieDeath;

        [MenuItem("TWD/Animation Setup/Quaternius + Mixamo Wizard")]
        private static void Open()
        {
            TWDAnimationSetupWindow window = GetWindow<TWDAnimationSetupWindow>("TWD Anim Setup");
            window.minSize = new Vector2(520f, 780f);
            window.TryAssignDefaults();
        }

        private void OnEnable()
        {
            TryAssignDefaults();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Import the Quaternius character as Humanoid, import each Mixamo animation as Humanoid, and set each Mixamo FBX to 'Copy From Other Avatar' using the matching Quaternius character avatar. Then use this window to build and assign the controllers.",
                MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawPlayerSection();
            EditorGUILayout.Space(16f);
            DrawZombieSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawPlayerSection()
        {
            EditorGUILayout.LabelField("Player Setup", EditorStyles.boldLabel);
            _playerTarget = (GameObject)EditorGUILayout.ObjectField("Player Root", _playerTarget, typeof(GameObject), true);
            _playerAvatar = (Avatar)EditorGUILayout.ObjectField("Player Avatar", _playerAvatar, typeof(Avatar), false);
            _playerControllerPath = EditorGUILayout.TextField("Controller Path", _playerControllerPath);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Required Clips", EditorStyles.miniBoldLabel);
            _playerIdle = (AnimationClip)EditorGUILayout.ObjectField("Idle", _playerIdle, typeof(AnimationClip), false);
            _playerWalk = (AnimationClip)EditorGUILayout.ObjectField("Walk", _playerWalk, typeof(AnimationClip), false);
            _playerRun = (AnimationClip)EditorGUILayout.ObjectField("Run", _playerRun, typeof(AnimationClip), false);
            _playerJump = (AnimationClip)EditorGUILayout.ObjectField("Jump", _playerJump, typeof(AnimationClip), false);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Optional Clips", EditorStyles.miniBoldLabel);
            _playerAimIdle = (AnimationClip)EditorGUILayout.ObjectField("Aim Idle", _playerAimIdle, typeof(AnimationClip), false);
            _playerCrouchIdle = (AnimationClip)EditorGUILayout.ObjectField("Crouch Idle", _playerCrouchIdle, typeof(AnimationClip), false);
            _playerCrouchWalk = (AnimationClip)EditorGUILayout.ObjectField("Crouch Walk", _playerCrouchWalk, typeof(AnimationClip), false);
            _playerShoot = (AnimationClip)EditorGUILayout.ObjectField("Shoot", _playerShoot, typeof(AnimationClip), false);
            _playerReload = (AnimationClip)EditorGUILayout.ObjectField("Reload", _playerReload, typeof(AnimationClip), false);
            _playerMelee = (AnimationClip)EditorGUILayout.ObjectField("Melee", _playerMelee, typeof(AnimationClip), false);
            _playerInteract = (AnimationClip)EditorGUILayout.ObjectField("Interact", _playerInteract, typeof(AnimationClip), false);
            _playerTakeDamage = (AnimationClip)EditorGUILayout.ObjectField("Take Damage", _playerTakeDamage, typeof(AnimationClip), false);
            _playerDeath = (AnimationClip)EditorGUILayout.ObjectField("Death", _playerDeath, typeof(AnimationClip), false);

            if (GUILayout.Button("Build And Assign Player Controller", GUILayout.Height(28f)))
            {
                BuildAndAssignPlayerController();
            }
        }

        private void DrawZombieSection()
        {
            EditorGUILayout.LabelField("Zombie Setup", EditorStyles.boldLabel);
            _zombieTarget = (GameObject)EditorGUILayout.ObjectField("Zombie Root / Prefab", _zombieTarget, typeof(GameObject), true);
            _zombieAvatar = (Avatar)EditorGUILayout.ObjectField("Zombie Avatar", _zombieAvatar, typeof(Avatar), false);
            _zombieControllerPath = EditorGUILayout.TextField("Controller Path", _zombieControllerPath);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Required Clips", EditorStyles.miniBoldLabel);
            _zombieIdle = (AnimationClip)EditorGUILayout.ObjectField("Idle", _zombieIdle, typeof(AnimationClip), false);
            _zombieWalk = (AnimationClip)EditorGUILayout.ObjectField("Walk", _zombieWalk, typeof(AnimationClip), false);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Optional Clips", EditorStyles.miniBoldLabel);
            _zombieChase = (AnimationClip)EditorGUILayout.ObjectField("Chase / Fast Walk", _zombieChase, typeof(AnimationClip), false);
            _zombieAttack = (AnimationClip)EditorGUILayout.ObjectField("Attack", _zombieAttack, typeof(AnimationClip), false);
            _zombieStagger = (AnimationClip)EditorGUILayout.ObjectField("Stagger", _zombieStagger, typeof(AnimationClip), false);
            _zombieDeath = (AnimationClip)EditorGUILayout.ObjectField("Death", _zombieDeath, typeof(AnimationClip), false);

            if (GUILayout.Button("Build And Assign Zombie Controller", GUILayout.Height(28f)))
            {
                BuildAndAssignZombieController();
            }
        }

        private void TryAssignDefaults()
        {
            if (_playerTarget == null)
                _playerTarget = GameObject.Find("Player");

            if (_zombieTarget == null)
                _zombieTarget = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/ZombieBasic.prefab");
        }

        private void BuildAndAssignPlayerController()
        {
            if (_playerTarget == null || _playerAvatar == null || _playerIdle == null || _playerWalk == null || _playerRun == null || _playerJump == null)
            {
                EditorUtility.DisplayDialog("Player Setup", "Assign the player root, player avatar, and the required Idle/Walk/Run/Jump clips first.", "OK");
                return;
            }

            Animator targetAnimator = FindRootAnimator(_playerTarget);
            if (targetAnimator == null)
            {
                EditorUtility.DisplayDialog("Player Setup", "No Animator component was found on the selected player root.", "OK");
                return;
            }

            AnimatorController controller = CreateFreshController(_playerControllerPath);
            AddParameter(controller, Constants.AnimParams.SPEED, AnimatorControllerParameterType.Float);
            AddParameter(controller, Constants.AnimParams.IS_SPRINTING, AnimatorControllerParameterType.Bool);
            AddParameter(controller, Constants.AnimParams.IS_CROUCHING, AnimatorControllerParameterType.Bool);
            AddParameter(controller, Constants.AnimParams.IS_AIMING, AnimatorControllerParameterType.Bool);
            AddParameter(controller, Constants.AnimParams.SHOOT, AnimatorControllerParameterType.Trigger);
            AddParameter(controller, Constants.AnimParams.RELOAD, AnimatorControllerParameterType.Trigger);
            AddParameter(controller, Constants.AnimParams.MELEE, AnimatorControllerParameterType.Trigger);
            AddParameter(controller, Constants.AnimParams.TAKE_DAMAGE, AnimatorControllerParameterType.Trigger);
            AddParameter(controller, Constants.AnimParams.IS_DEAD, AnimatorControllerParameterType.Bool);
            AddParameter(controller, Constants.AnimParams.INTERACT, AnimatorControllerParameterType.Trigger);
            AddParameter(controller, Constants.AnimParams.JUMP, AnimatorControllerParameterType.Trigger);
            AddParameter(controller, Constants.AnimParams.IS_GROUNDED, AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState locomotion = sm.AddState("Locomotion", new Vector3(260f, 140f, 0f));
            sm.defaultState = locomotion;
            locomotion.motion = CreatePlayerLocomotionTree(controller);

            AnimatorState crouchLocomotion = null;
            if (_playerCrouchIdle != null && _playerCrouchWalk != null)
            {
                crouchLocomotion = sm.AddState("Crouch", new Vector3(260f, 280f, 0f));
                crouchLocomotion.motion = CreatePlayerCrouchTree(controller);
                AddBoolTransition(locomotion, crouchLocomotion, Constants.AnimParams.IS_CROUCHING, true);
                AddBoolTransition(crouchLocomotion, locomotion, Constants.AnimParams.IS_CROUCHING, false);
            }

            AnimatorState aimIdle = null;
            if (_playerAimIdle != null)
            {
                aimIdle = sm.AddState("AimIdle", new Vector3(260f, 20f, 0f));
                aimIdle.motion = _playerAimIdle;
                AddBoolTransition(locomotion, aimIdle, Constants.AnimParams.IS_AIMING, true);
                AddBoolTransition(aimIdle, locomotion, Constants.AnimParams.IS_AIMING, false);
            }

            if (_playerJump != null)
            {
                AnimatorState jump = sm.AddState("Jump", new Vector3(520f, 140f, 0f));
                jump.motion = _playerJump;
                AddAnyStateTrigger(sm, jump, Constants.AnimParams.JUMP);
                AddReturnTransition(jump, locomotion);
            }

            AddOptionalActionState(sm, locomotion, "Shoot", _playerShoot, Constants.AnimParams.SHOOT, new Vector3(520f, -40f, 0f));
            AddOptionalActionState(sm, locomotion, "Reload", _playerReload, Constants.AnimParams.RELOAD, new Vector3(520f, 20f, 0f));
            AddOptionalActionState(sm, locomotion, "Melee", _playerMelee, Constants.AnimParams.MELEE, new Vector3(520f, 80f, 0f));
            AddOptionalActionState(sm, locomotion, "Interact", _playerInteract, Constants.AnimParams.INTERACT, new Vector3(520f, 200f, 0f));
            AddOptionalActionState(sm, locomotion, "TakeDamage", _playerTakeDamage, Constants.AnimParams.TAKE_DAMAGE, new Vector3(520f, 260f, 0f));

            if (_playerDeath != null)
            {
                AnimatorState death = sm.AddState("Death", new Vector3(760f, 140f, 0f));
                death.motion = _playerDeath;
                AddAnyStateBool(sm, death, Constants.AnimParams.IS_DEAD, true);
            }

            AssignAnimator(targetAnimator, _playerAvatar, controller);
            DisableNestedAnimators(_playerTarget, targetAnimator);

            if (crouchLocomotion == null)
                Debug.Log("[TWDAnimationSetupWindow] Player crouch clips were not assigned, so crouch will continue using the standing locomotion visuals.");
            if (aimIdle == null)
                Debug.Log("[TWDAnimationSetupWindow] Player aim clip was not assigned, so aiming will stay on locomotion/idle.");

            EditorUtility.DisplayDialog("Player Setup", "Player controller created and assigned. Drag your imported character under PlayerModel, reset its local transform, and disable/remove any nested Animator on the imported child if needed.", "OK");
        }

        private void BuildAndAssignZombieController()
        {
            if (_zombieTarget == null || _zombieAvatar == null || _zombieIdle == null || _zombieWalk == null)
            {
                EditorUtility.DisplayDialog("Zombie Setup", "Assign the zombie root/prefab, zombie avatar, and the required Idle/Walk clips first.", "OK");
                return;
            }

            Animator targetAnimator = FindRootAnimator(_zombieTarget);
            if (targetAnimator == null)
            {
                EditorUtility.DisplayDialog("Zombie Setup", "No Animator component was found on the selected zombie root/prefab.", "OK");
                return;
            }

            AnimatorController controller = CreateFreshController(_zombieControllerPath);
            AddParameter(controller, Constants.AnimParams.SPEED, AnimatorControllerParameterType.Float);
            AddParameter(controller, Constants.AnimParams.IS_CHASING, AnimatorControllerParameterType.Bool);
            AddParameter(controller, Constants.AnimParams.ATTACK, AnimatorControllerParameterType.Trigger);
            AddParameter(controller, Constants.AnimParams.STAGGER, AnimatorControllerParameterType.Trigger);
            AddParameter(controller, Constants.AnimParams.IS_DEAD, AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState locomotion = sm.AddState("Locomotion", new Vector3(260f, 140f, 0f));
            sm.defaultState = locomotion;
            locomotion.motion = CreateZombieLocomotionTree(controller);

            AddOptionalActionState(sm, locomotion, "Attack", _zombieAttack, Constants.AnimParams.ATTACK, new Vector3(520f, 60f, 0f));
            AddOptionalActionState(sm, locomotion, "Stagger", _zombieStagger, Constants.AnimParams.STAGGER, new Vector3(520f, 180f, 0f));

            if (_zombieDeath != null)
            {
                AnimatorState death = sm.AddState("Death", new Vector3(760f, 140f, 0f));
                death.motion = _zombieDeath;
                AddAnyStateBool(sm, death, Constants.AnimParams.IS_DEAD, true);
            }

            AssignAnimator(targetAnimator, _zombieAvatar, controller);
            DisableNestedAnimators(_zombieTarget, targetAnimator);

            EditorUtility.DisplayDialog("Zombie Setup", "Zombie controller created and assigned. Place your imported zombie under ZombieModel or replace that child entirely, then reset the imported model's local transform.", "OK");
        }

        private BlendTree CreatePlayerLocomotionTree(AnimatorController controller)
        {
            BlendTree tree = new BlendTree
            {
                name = "PlayerLocomotion",
                blendType = BlendTreeType.Simple1D,
                blendParameter = Constants.AnimParams.SPEED,
                useAutomaticThresholds = false
            };

            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(_playerIdle, 0f);
            tree.AddChild(_playerWalk, 1f);
            tree.AddChild(_playerRun, 1.5f);
            return tree;
        }

        private BlendTree CreatePlayerCrouchTree(AnimatorController controller)
        {
            BlendTree tree = new BlendTree
            {
                name = "PlayerCrouch",
                blendType = BlendTreeType.Simple1D,
                blendParameter = Constants.AnimParams.SPEED,
                useAutomaticThresholds = false
            };

            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(_playerCrouchIdle, 0f);
            tree.AddChild(_playerCrouchWalk, 1f);
            return tree;
        }

        private BlendTree CreateZombieLocomotionTree(AnimatorController controller)
        {
            BlendTree tree = new BlendTree
            {
                name = "ZombieLocomotion",
                blendType = BlendTreeType.Simple1D,
                blendParameter = Constants.AnimParams.SPEED,
                useAutomaticThresholds = false
            };

            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(_zombieIdle, 0f);
            tree.AddChild(_zombieWalk, 0.5f);
            tree.AddChild(_zombieChase != null ? _zombieChase : _zombieWalk, 1f);
            return tree;
        }

        private static AnimatorController CreateFreshController(string assetPath)
        {
            EnsureFolderForAsset(assetPath);

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            AssetDatabase.ImportAsset(assetPath);
            return controller;
        }

        private static void AddParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            controller.AddParameter(name, type);
        }

        private static void AddBoolTransition(AnimatorState from, AnimatorState to, string parameterName, bool value)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameterName);
        }

        private static void AddAnyStateTrigger(AnimatorStateMachine stateMachine, AnimatorState targetState, string parameterName)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(targetState);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameterName);
        }

        private static void AddAnyStateBool(AnimatorStateMachine stateMachine, AnimatorState targetState, string parameterName, bool value)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(targetState);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameterName);
        }

        private static void AddReturnTransition(AnimatorState from, AnimatorState to)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = 0.9f;
            transition.duration = 0.08f;
        }

        private static void AddOptionalActionState(
            AnimatorStateMachine stateMachine,
            AnimatorState locomotion,
            string stateName,
            AnimationClip clip,
            string triggerParam,
            Vector3 position)
        {
            if (clip == null)
                return;

            AnimatorState state = stateMachine.AddState(stateName, position);
            state.motion = clip;
            AddAnyStateTrigger(stateMachine, state, triggerParam);
            AddReturnTransition(state, locomotion);
        }

        private static Animator FindRootAnimator(GameObject targetRoot)
        {
            if (targetRoot == null)
                return null;

            Animator rootAnimator = targetRoot.GetComponent<Animator>();
            if (rootAnimator != null)
                return rootAnimator;

            return targetRoot.GetComponentInChildren<Animator>(true);
        }

        private static void AssignAnimator(Animator targetAnimator, Avatar avatar, RuntimeAnimatorController controller)
        {
            Undo.RecordObject(targetAnimator, "Assign TWD Animator");
            targetAnimator.avatar = avatar;
            targetAnimator.runtimeAnimatorController = controller;
            targetAnimator.applyRootMotion = false;
            targetAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            EditorUtility.SetDirty(targetAnimator);
            PrefabUtility.RecordPrefabInstancePropertyModifications(targetAnimator);

            if (targetAnimator.gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(targetAnimator.gameObject.scene);

            AssetDatabase.SaveAssets();
        }

        private static void DisableNestedAnimators(GameObject targetRoot, Animator rootAnimator)
        {
            Animator[] animators = targetRoot.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator == null || animator == rootAnimator)
                    continue;

                Undo.RecordObject(animator, "Disable Nested Animator");
                animator.enabled = false;
                EditorUtility.SetDirty(animator);
                PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
            }
        }

        private static void EnsureFolderForAsset(string assetPath)
        {
            string folder = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(folder))
                return;

            folder = folder.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }
    }
}
