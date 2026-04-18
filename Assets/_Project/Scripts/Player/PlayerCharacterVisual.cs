using UnityEngine;
using UnityEngine.SceneManagement;

namespace TWD.Player
{
    public class PlayerCharacterVisual : MonoBehaviour
    {
        private const string VisualRootName = "[CharacterVisual]";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureInstalled(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureInstalled(scene);
        }

        private static void EnsureInstalled(Scene scene)
        {
            GameObject playerModel = GameObject.Find("PlayerModel");
            if (playerModel == null)
                return;

            if (playerModel.GetComponent<PlayerCharacterVisual>() == null)
                playerModel.AddComponent<PlayerCharacterVisual>();
        }

        private void Awake()
        {
            BuildVisual();
        }

        private void BuildVisual()
        {
            MeshRenderer existingRenderer = GetComponent<MeshRenderer>();
            if (existingRenderer != null)
                existingRenderer.enabled = false;

            Transform visualRoot = transform.Find(VisualRootName);
            if (visualRoot == null)
            {
                visualRoot = new GameObject(VisualRootName).transform;
                visualRoot.SetParent(transform, false);
            }

            if (visualRoot.childCount > 0)
                return;

            Material jacket = CreateMaterial(new Color(0.3f, 0.38f, 0.44f, 1f), 0.04f, 0.24f);
            Material shirt = CreateMaterial(new Color(0.62f, 0.21f, 0.18f, 1f), 0.02f, 0.16f);
            Material pants = CreateMaterial(new Color(0.18f, 0.24f, 0.32f, 1f), 0.03f, 0.2f);
            Material boots = CreateMaterial(new Color(0.09f, 0.08f, 0.08f, 1f), 0.02f, 0.12f);
            Material skin = CreateMaterial(new Color(0.77f, 0.64f, 0.54f, 1f), 0.01f, 0.24f);

            CreatePart("Hips", PrimitiveType.Cube, visualRoot, new Vector3(0f, -0.02f, 0f), new Vector3(0.34f, 0.18f, 0.18f), pants);
            CreatePart("Torso", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.24f, 0f), new Vector3(0.44f, 0.42f, 0.2f), jacket);
            CreatePart("Chest", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.26f, 0.11f), new Vector3(0.2f, 0.18f, 0.05f), shirt);
            CreatePart("Head", PrimitiveType.Sphere, visualRoot, new Vector3(0f, 0.62f, 0.02f), new Vector3(0.24f, 0.24f, 0.24f), skin);
            CreatePart("LeftArm", PrimitiveType.Cube, visualRoot, new Vector3(-0.31f, 0.19f, 0f), new Vector3(0.11f, 0.34f, 0.11f), jacket, new Vector3(0f, 0f, -12f));
            CreatePart("RightArm", PrimitiveType.Cube, visualRoot, new Vector3(0.31f, 0.19f, 0f), new Vector3(0.11f, 0.34f, 0.11f), jacket, new Vector3(0f, 0f, 12f));
            CreatePart("LeftLeg", PrimitiveType.Cube, visualRoot, new Vector3(-0.1f, -0.34f, 0f), new Vector3(0.12f, 0.48f, 0.13f), pants);
            CreatePart("RightLeg", PrimitiveType.Cube, visualRoot, new Vector3(0.1f, -0.34f, 0f), new Vector3(0.12f, 0.48f, 0.13f), pants);
            CreatePart("LeftFoot", PrimitiveType.Cube, visualRoot, new Vector3(-0.1f, -0.64f, 0.07f), new Vector3(0.14f, 0.06f, 0.22f), boots);
            CreatePart("RightFoot", PrimitiveType.Cube, visualRoot, new Vector3(0.1f, -0.64f, 0.07f), new Vector3(0.14f, 0.06f, 0.22f), boots);
        }

        private static void CreatePart(string name, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            CreatePart(name, primitiveType, parent, localPosition, localScale, material, Vector3.zero);
        }

        private static void CreatePart(string name, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, Vector3 localEulerAngles)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localEulerAngles = localEulerAngles;
            part.transform.localScale = localScale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private static Material CreateMaterial(Color baseColor, float metallic, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader);
            material.color = baseColor;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", baseColor);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", smoothness);

            return material;
        }
    }
}
