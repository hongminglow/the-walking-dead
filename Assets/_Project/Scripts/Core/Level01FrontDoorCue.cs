using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TWD.Utilities;

namespace TWD.Core
{
    public static class Level01FrontDoorCue
    {
        private const string CueRootName = "[FrontDoorCue]";
        private const string LeftWallSegmentName = "Wall_South_Left";
        private const string RightWallSegmentName = "Wall_South_Right";

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

            GameObject frontDoor = GameObject.Find("FrontDoor");
            if (frontDoor == null)
                return;

            EnsureDoorway(frontDoor);
            EnsureExitZone();

            if (frontDoor.transform.Find(CueRootName) != null)
                return;

            Transform cueRoot = new GameObject(CueRootName).transform;
            cueRoot.SetParent(frontDoor.transform, false);
            cueRoot.localPosition = Vector3.zero;

            Material doorMaterial = CreateLitMaterial(new Color(0.32f, 0.19f, 0.1f, 1f), 0.04f, 0.4f, Color.black);
            Material frameMaterial = CreateLitMaterial(new Color(0.29f, 0.16f, 0.11f, 1f), 0.08f, 0.34f, Color.black);
            Material signMaterial = CreateLitMaterial(new Color(0.18f, 0.19f, 0.16f, 1f), 0.03f, 0.28f, new Color(0.35f, 0.85f, 0.5f, 1f) * 0.65f);

            Renderer frontDoorRenderer = frontDoor.GetComponent<Renderer>();
            if (frontDoorRenderer != null)
                frontDoorRenderer.sharedMaterial = doorMaterial;

            CreatePart("FrameLeft", PrimitiveType.Cube, cueRoot, new Vector3(-0.58f, 0f, -0.02f), new Vector3(0.06f, 1.82f, 0.08f), frameMaterial);
            CreatePart("FrameRight", PrimitiveType.Cube, cueRoot, new Vector3(0.58f, 0f, -0.02f), new Vector3(0.06f, 1.82f, 0.08f), frameMaterial);
            CreatePart("FrameTop", PrimitiveType.Cube, cueRoot, new Vector3(0f, 0.88f, -0.02f), new Vector3(1.22f, 0.08f, 0.08f), frameMaterial);
            CreatePart("DoorHandle", PrimitiveType.Sphere, cueRoot, new Vector3(0.42f, -0.05f, -0.12f), new Vector3(0.08f, 0.08f, 0.08f), CreateLitMaterial(new Color(0.86f, 0.73f, 0.36f, 1f), 0.42f, 0.7f, new Color(0.18f, 0.14f, 0.04f, 1f) * 0.15f));
            CreatePart("LampBackplate", PrimitiveType.Cube, cueRoot, new Vector3(0f, 1.12f, -0.08f), new Vector3(0.42f, 0.12f, 0.04f), signMaterial);
            CreatePart("LampBulb", PrimitiveType.Sphere, cueRoot, new Vector3(0f, 0.98f, -0.05f), new Vector3(0.12f, 0.12f, 0.12f), CreateLitMaterial(new Color(1f, 0.92f, 0.72f, 1f), 0.05f, 0.72f, new Color(1f, 0.92f, 0.72f, 1f) * 3f));

            GameObject signCanvasObject = new GameObject("ExitSign", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            signCanvasObject.transform.SetParent(cueRoot, false);
            signCanvasObject.transform.localPosition = new Vector3(0f, 1.22f, -0.06f);

            Canvas canvas = signCanvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = UnityEngine.Camera.main;
            canvas.sortingOrder = 40;

            RectTransform canvasRect = signCanvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(100f, 24f);
            canvasRect.localScale = Vector3.one * 0.01f;

            Image background = signCanvasObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.08f, 0.05f, 0.8f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Outline));
            labelObject.transform.SetParent(signCanvasObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObject.GetComponent<Text>();
            label.font = font;
            label.fontSize = 16;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.72f, 1f, 0.72f, 1f);
            label.text = "EXIT";

            Outline outline = labelObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);

            Light cueLight = signCanvasObject.AddComponent<Light>();
            cueLight.type = LightType.Point;
            cueLight.color = new Color(1f, 0.9f, 0.76f, 1f);
            cueLight.intensity = 1.2f;
            cueLight.range = 4.8f;
            cueLight.shadows = LightShadows.None;
        }

        private static void EnsureDoorway(GameObject frontDoor)
        {
            GameObject southWall = GameObject.Find("Wall_South");
            if (southWall == null)
                return;

            southWall.transform.position = new Vector3(12.5f, 1.75f, -0.15f);
            southWall.transform.localScale = new Vector3(15f, 3.5f, 0.3f);

            Renderer wallRenderer = southWall.GetComponent<Renderer>();
            Material wallMaterial = wallRenderer != null ? wallRenderer.sharedMaterial : null;

            GameObject leftSegment = GameObject.Find(LeftWallSegmentName);
            if (leftSegment == null)
            {
                leftSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftSegment.name = LeftWallSegmentName;
                leftSegment.layer = southWall.layer;
            }

            leftSegment.transform.position = new Vector3(1.5f, 1.75f, -0.15f);
            leftSegment.transform.localScale = new Vector3(3f, 3.5f, 0.3f);

            Renderer leftRenderer = leftSegment.GetComponent<Renderer>();
            if (leftRenderer != null && wallMaterial != null)
                leftRenderer.sharedMaterial = wallMaterial;

            Collider leftCollider = leftSegment.GetComponent<Collider>();
            if (leftCollider != null)
                leftCollider.enabled = true;

            GameObject strayRightSegment = GameObject.Find(RightWallSegmentName);
            if (strayRightSegment != null)
                Object.Destroy(strayRightSegment);

            frontDoor.transform.position = new Vector3(4f, 1.75f, -0.08f);
            frontDoor.transform.localScale = new Vector3(2f, 3.5f, 0.18f);
        }

        private static void EnsureExitZone()
        {
            GameObject exitZone = GameObject.Find("ExitZone_ToStreets");
            if (exitZone == null)
                return;

            exitZone.transform.position = new Vector3(4f, 1f, -1.5f);
        }

        private static void CreatePart(string name, PrimitiveType primitiveType, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
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

        private static Material CreateLitMaterial(Color baseColor, float metallic, float smoothness, Color emission)
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
                material.SetColor("_EmissionColor", emission);
            }

            return material;
        }
    }
}
