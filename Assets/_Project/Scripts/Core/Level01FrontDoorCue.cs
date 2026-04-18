using UnityEngine;
using UnityEngine.SceneManagement;
using TWD.Utilities;

namespace TWD.Core
{
    public static class Level01FrontDoorCue
    {
        private const string CueRootName = "[FrontDoorFrameCue]";
        private const string LeafCueRootName = "[FrontDoorLeafCue]";
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

            Transform staticCueRoot = frontDoor.transform.parent != null
                ? frontDoor.transform.parent.Find(CueRootName)
                : GameObject.Find(CueRootName)?.transform;
            if (staticCueRoot == null)
            {
                staticCueRoot = new GameObject(CueRootName).transform;
                if (frontDoor.transform.parent != null)
                    staticCueRoot.SetParent(frontDoor.transform.parent, false);
            }

            staticCueRoot.position = frontDoor.transform.position;
            staticCueRoot.rotation = frontDoor.transform.rotation;
            staticCueRoot.localScale = Vector3.one;
            ClearChildren(staticCueRoot);

            Transform leafCueRoot = frontDoor.transform.Find(LeafCueRootName);
            if (leafCueRoot == null)
            {
                leafCueRoot = new GameObject(LeafCueRootName).transform;
                leafCueRoot.SetParent(frontDoor.transform, false);
            }

            leafCueRoot.localPosition = Vector3.zero;
            leafCueRoot.localRotation = Quaternion.identity;
            leafCueRoot.localScale = Vector3.one;
            ClearChildren(leafCueRoot);

            Material doorMaterial = CreateLitMaterial(new Color(0.42f, 0.24f, 0.12f, 1f), 0.02f, 0.42f, Color.black);
            Material panelMaterial = CreateLitMaterial(new Color(0.34f, 0.18f, 0.1f, 1f), 0.02f, 0.38f, Color.black);
            Material frameMaterial = CreateLitMaterial(new Color(0.24f, 0.13f, 0.08f, 1f), 0.05f, 0.32f, Color.black);
            Material hardwareMaterial = CreateLitMaterial(new Color(0.84f, 0.68f, 0.3f, 1f), 0.46f, 0.74f, new Color(0.1f, 0.08f, 0.03f, 1f) * 0.08f);
            Material matMaterial = CreateLitMaterial(new Color(0.15f, 0.12f, 0.1f, 1f), 0.01f, 0.18f, Color.black);

            Renderer frontDoorRenderer = frontDoor.GetComponent<Renderer>();
            if (frontDoorRenderer != null)
                frontDoorRenderer.sharedMaterial = doorMaterial;

            CreatePart("FrameLeft", PrimitiveType.Cube, staticCueRoot, new Vector3(-0.61f, 0f, -0.02f), new Vector3(0.1f, 1.9f, 0.14f), frameMaterial);
            CreatePart("FrameRight", PrimitiveType.Cube, staticCueRoot, new Vector3(0.61f, 0f, -0.02f), new Vector3(0.1f, 1.9f, 0.14f), frameMaterial);
            CreatePart("FrameTop", PrimitiveType.Cube, staticCueRoot, new Vector3(0f, 0.92f, -0.02f), new Vector3(1.32f, 0.1f, 0.14f), frameMaterial);
            CreatePart("Threshold", PrimitiveType.Cube, staticCueRoot, new Vector3(0f, -0.92f, 0.03f), new Vector3(1.14f, 0.05f, 0.18f), frameMaterial);
            CreatePart("DoorMat", PrimitiveType.Cube, staticCueRoot, new Vector3(0f, -1.01f, 0.66f), new Vector3(0.9f, 0.02f, 0.44f), matMaterial);

            CreatePart("UpperPanel", PrimitiveType.Cube, leafCueRoot, new Vector3(0f, 0.38f, -0.02f), new Vector3(0.74f, 0.56f, 0.06f), panelMaterial);
            CreatePart("LowerPanel", PrimitiveType.Cube, leafCueRoot, new Vector3(0f, -0.32f, -0.02f), new Vector3(0.74f, 0.74f, 0.06f), panelMaterial);
            CreatePart("MiddleRail", PrimitiveType.Cube, leafCueRoot, new Vector3(0f, 0.02f, -0.03f), new Vector3(0.82f, 0.08f, 0.07f), frameMaterial);
            CreatePart("LeftStile", PrimitiveType.Cube, leafCueRoot, new Vector3(-0.44f, 0f, -0.03f), new Vector3(0.08f, 1.64f, 0.07f), frameMaterial);
            CreatePart("RightStile", PrimitiveType.Cube, leafCueRoot, new Vector3(0.44f, 0f, -0.03f), new Vector3(0.08f, 1.64f, 0.07f), frameMaterial);
            CreatePart("TopRail", PrimitiveType.Cube, leafCueRoot, new Vector3(0f, 0.78f, -0.03f), new Vector3(0.82f, 0.08f, 0.07f), frameMaterial);
            CreatePart("BottomRail", PrimitiveType.Cube, leafCueRoot, new Vector3(0f, -0.78f, -0.03f), new Vector3(0.82f, 0.08f, 0.07f), frameMaterial);
            CreatePart("HandlePlate", PrimitiveType.Cube, leafCueRoot, new Vector3(0.34f, -0.04f, -0.11f), new Vector3(0.06f, 0.22f, 0.03f), hardwareMaterial);
            CreatePart("HandleGrip", PrimitiveType.Cube, leafCueRoot, new Vector3(0.42f, -0.04f, -0.14f), new Vector3(0.12f, 0.03f, 0.03f), hardwareMaterial);
            CreatePart("KeyLock", PrimitiveType.Sphere, leafCueRoot, new Vector3(0.33f, -0.18f, -0.12f), new Vector3(0.04f, 0.04f, 0.02f), hardwareMaterial);
            CreatePart("Peephole", PrimitiveType.Sphere, leafCueRoot, new Vector3(0f, 0.58f, -0.11f), new Vector3(0.04f, 0.04f, 0.02f), hardwareMaterial);
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

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }
    }
}
