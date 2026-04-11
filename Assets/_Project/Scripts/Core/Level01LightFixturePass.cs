using UnityEngine;
using UnityEngine.SceneManagement;
using TWD.Utilities;

namespace TWD.Core
{
    public static class Level01LightFixturePass
    {
        private const string FixtureRootName = "[Fixture]";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureForScene(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureForScene(scene);
        }

        private static void EnsureForScene(Scene scene)
        {
            if (!string.Equals(scene.name, Constants.Scenes.LEVEL_01_HOUSE, System.StringComparison.Ordinal))
                return;

            EnsureFixture("Light_Hallway", new Color(1f, 0.9f, 0.74f, 1f), 3.2f, 8.5f, true);
            EnsureFixture("Light_Living", new Color(1f, 0.88f, 0.72f, 1f), 3.8f, 9.5f, false);
            EnsureFixture("Light_Kitchen", new Color(1f, 0.91f, 0.78f, 1f), 3.4f, 9f, false);
            EnsureFixture("Light_Bedroom", new Color(1f, 0.86f, 0.7f, 1f), 3.1f, 8f, false);
            EnsureFixture("Light_Bathroom", new Color(0.95f, 0.96f, 1f, 1f), 2.6f, 7f, true);
        }

        private static void EnsureFixture(string objectName, Color lightColor, float minIntensity, float minRange, bool compact)
        {
            GameObject lightObject = GameObject.Find(objectName);
            if (lightObject == null)
                return;

            Light light = lightObject.GetComponent<Light>();
            if (light == null)
                return;

            light.color = lightColor;
            light.intensity = Mathf.Max(light.intensity, minIntensity);
            light.range = Mathf.Max(light.range, minRange);
            light.shadows = LightShadows.None;

            if (lightObject.transform.Find(FixtureRootName) != null)
                return;

            Material metal = CreateFixtureMaterial(new Color(0.16f, 0.16f, 0.18f, 1f), 0.1f, 0.45f, Color.black);
            Material bulb = CreateFixtureMaterial(new Color(1f, 0.92f, 0.76f, 1f), 0.05f, 0.75f, lightColor * 2.8f);

            Transform root = new GameObject(FixtureRootName).transform;
            root.SetParent(lightObject.transform, false);
            root.localPosition = Vector3.zero;

            CreatePart("Canopy", PrimitiveType.Cylinder, root, new Vector3(0f, 0.18f, 0f), Quaternion.identity, new Vector3(0.28f, 0.025f, 0.28f), metal);
            CreatePart("Stem", PrimitiveType.Cylinder, root, new Vector3(0f, 0.08f, 0f), Quaternion.identity, new Vector3(0.05f, 0.09f, 0.05f), metal);

            if (compact)
            {
                CreatePart("Shade", PrimitiveType.Sphere, root, new Vector3(0f, -0.02f, 0f), Quaternion.identity, new Vector3(0.4f, 0.16f, 0.4f), metal);
                CreatePart("Bulb", PrimitiveType.Sphere, root, new Vector3(0f, -0.12f, 0f), Quaternion.identity, new Vector3(0.16f, 0.16f, 0.16f), bulb);
            }
            else
            {
                CreatePart("Shade", PrimitiveType.Cylinder, root, new Vector3(0f, -0.04f, 0f), Quaternion.identity, new Vector3(0.34f, 0.12f, 0.34f), metal);
                CreatePart("Bulb", PrimitiveType.Sphere, root, new Vector3(0f, -0.18f, 0f), Quaternion.identity, new Vector3(0.16f, 0.16f, 0.16f), bulb);
                CreatePart("Trim", PrimitiveType.Cylinder, root, new Vector3(0f, -0.18f, 0f), Quaternion.identity, new Vector3(0.36f, 0.01f, 0.36f), metal);
            }
        }

        private static void CreatePart(string name, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private static Material CreateFixtureMaterial(Color baseColor, float metallic, float smoothness, Color emissionColor)
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

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emissionColor);
            }

            return material;
        }
    }
}
