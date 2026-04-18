using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TWD.Enemies
{
    public class EnemyCharacterVisual : MonoBehaviour
    {
        private const string VisualRootName = "[EnemyVisual]";
        private const string DeathBurstRootName = "[DeathBurst]";

        private readonly List<Renderer> _legacyRenderers = new List<Renderer>();
        private readonly List<Transform> _burstParticles = new List<Transform>();
        private readonly List<Vector3> _burstVelocities = new List<Vector3>();

        private Transform _visualRoot;
        private Transform _burstRoot;
        private bool _isBrute;
        private bool _isCrawler;
        private bool _isVanishing;

        private void Awake()
        {
            ResolveVariant();
            BuildVisual();
        }

        public float TriggerDeathVanish()
        {
            const float vanishDuration = 0.9f;

            if (_isVanishing)
                return vanishDuration;

            StartCoroutine(VanishRoutine(vanishDuration));
            return vanishDuration;
        }

        private void ResolveVariant()
        {
            _isBrute = GetComponent<ZombieBrute>() != null;
            _isCrawler = GetComponent<ZombieCrawler>() != null;
        }

        private void BuildVisual()
        {
            _visualRoot = transform.Find(VisualRootName);
            if (_visualRoot == null)
            {
                _visualRoot = new GameObject(VisualRootName).transform;
                _visualRoot.SetParent(transform, false);
            }

            if (_visualRoot.childCount > 0)
                return;

            HideLegacyMeshes();

            if (_isCrawler)
                BuildCrawler();
            else
                BuildWalker();
        }

        private void HideLegacyMeshes()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                if (_visualRoot != null && renderer.transform.IsChildOf(_visualRoot))
                    continue;

                _legacyRenderers.Add(renderer);
                renderer.enabled = false;
            }
        }

        private void BuildWalker()
        {
            float shoulderWidth = _isBrute ? 0.52f : 0.38f;
            float torsoHeight = _isBrute ? 0.54f : 0.46f;
            float armWidth = _isBrute ? 0.14f : 0.11f;
            float armHeight = _isBrute ? 0.42f : 0.36f;
            float legWidth = _isBrute ? 0.17f : 0.13f;
            float legHeight = _isBrute ? 0.58f : 0.5f;
            float headScale = _isBrute ? 0.26f : 0.23f;
            float torsoY = _isBrute ? 0.38f : 0.3f;
            float headY = _isBrute ? 0.82f : 0.7f;

            Material skin = CreateMaterial(_isBrute ? new Color(0.48f, 0.59f, 0.43f, 1f) : new Color(0.52f, 0.64f, 0.48f, 1f), 0.02f, 0.2f);
            Material shirt = CreateMaterial(_isBrute ? new Color(0.33f, 0.13f, 0.12f, 1f) : new Color(0.22f, 0.24f, 0.28f, 1f), 0.02f, 0.28f);
            Material pants = CreateMaterial(new Color(0.16f, 0.18f, 0.2f, 1f), 0.03f, 0.22f);
            Material wound = CreateMaterial(new Color(0.4f, 0.08f, 0.08f, 1f), 0.01f, 0.18f);
            Material eye = CreateMaterial(new Color(0.93f, 0.92f, 0.74f, 1f), 0.01f, 0.65f);

            _visualRoot.localPosition = _isBrute ? new Vector3(0f, -0.04f, 0f) : new Vector3(0f, -0.02f, 0f);
            _visualRoot.localRotation = Quaternion.identity;

            CreatePart("Hips", PrimitiveType.Cube, _visualRoot, new Vector3(0f, 0.02f, 0f), new Vector3(0.34f, 0.18f, 0.2f), pants);
            CreatePart("Torso", PrimitiveType.Cube, _visualRoot, new Vector3(0f, torsoY, 0.02f), new Vector3(shoulderWidth, torsoHeight, 0.22f), shirt, new Vector3(10f, 0f, 0f));
            CreatePart("Head", PrimitiveType.Sphere, _visualRoot, new Vector3(0f, headY, 0.09f), Vector3.one * headScale, skin, new Vector3(12f, 0f, 0f));
            CreatePart("Jaw", PrimitiveType.Cube, _visualRoot, new Vector3(0f, headY - 0.08f, 0.18f), new Vector3(0.16f, 0.08f, 0.06f), skin, new Vector3(18f, 0f, 0f));
            CreatePart("LeftArm", PrimitiveType.Cube, _visualRoot, new Vector3(-0.28f, torsoY + 0.02f, 0.12f), new Vector3(armWidth, armHeight, 0.12f), shirt, new Vector3(28f, 0f, -22f));
            CreatePart("RightArm", PrimitiveType.Cube, _visualRoot, new Vector3(0.28f, torsoY + 0.02f, 0.12f), new Vector3(armWidth, armHeight, 0.12f), shirt, new Vector3(28f, 0f, 22f));
            CreatePart("LeftLeg", PrimitiveType.Cube, _visualRoot, new Vector3(-0.1f, -0.4f, 0.02f), new Vector3(legWidth, legHeight, 0.14f), pants, new Vector3(-4f, 0f, -4f));
            CreatePart("RightLeg", PrimitiveType.Cube, _visualRoot, new Vector3(0.1f, -0.4f, 0.02f), new Vector3(legWidth, legHeight, 0.14f), pants, new Vector3(4f, 0f, 4f));
            CreatePart("LeftFoot", PrimitiveType.Cube, _visualRoot, new Vector3(-0.1f, -0.72f, 0.11f), new Vector3(0.16f, 0.07f, 0.24f), pants);
            CreatePart("RightFoot", PrimitiveType.Cube, _visualRoot, new Vector3(0.1f, -0.72f, 0.11f), new Vector3(0.16f, 0.07f, 0.24f), pants);
            CreatePart("ShoulderTear", PrimitiveType.Cube, _visualRoot, new Vector3(-0.08f, torsoY + 0.12f, 0.12f), new Vector3(0.12f, 0.08f, 0.05f), wound, new Vector3(14f, 0f, -8f));
            CreatePart("ChestWound", PrimitiveType.Cube, _visualRoot, new Vector3(0.12f, torsoY - 0.04f, 0.16f), new Vector3(0.12f, 0.12f, 0.05f), wound, new Vector3(16f, 0f, 8f));
            CreatePart("LeftEye", PrimitiveType.Sphere, _visualRoot, new Vector3(-0.05f, headY + 0.02f, 0.19f), new Vector3(0.03f, 0.03f, 0.02f), eye);
            CreatePart("RightEye", PrimitiveType.Sphere, _visualRoot, new Vector3(0.05f, headY + 0.02f, 0.19f), new Vector3(0.03f, 0.03f, 0.02f), eye);
        }

        private void BuildCrawler()
        {
            Material skin = CreateMaterial(new Color(0.5f, 0.62f, 0.46f, 1f), 0.02f, 0.2f);
            Material shirt = CreateMaterial(new Color(0.2f, 0.22f, 0.26f, 1f), 0.02f, 0.26f);
            Material pants = CreateMaterial(new Color(0.15f, 0.17f, 0.18f, 1f), 0.03f, 0.22f);
            Material wound = CreateMaterial(new Color(0.42f, 0.08f, 0.08f, 1f), 0.01f, 0.18f);

            _visualRoot.localPosition = new Vector3(0f, -0.62f, 0.18f);
            _visualRoot.localRotation = Quaternion.identity;

            CreatePart("Torso", PrimitiveType.Cube, _visualRoot, new Vector3(0f, 0.22f, 0.1f), new Vector3(0.42f, 0.24f, 0.56f), shirt, new Vector3(18f, 0f, 0f));
            CreatePart("Head", PrimitiveType.Sphere, _visualRoot, new Vector3(0f, 0.26f, 0.42f), new Vector3(0.2f, 0.2f, 0.2f), skin, new Vector3(16f, 0f, 0f));
            CreatePart("LeftArm", PrimitiveType.Cube, _visualRoot, new Vector3(-0.34f, 0.12f, 0.3f), new Vector3(0.1f, 0.16f, 0.4f), skin, new Vector3(0f, 0f, -38f));
            CreatePart("RightArm", PrimitiveType.Cube, _visualRoot, new Vector3(0.34f, 0.12f, 0.3f), new Vector3(0.1f, 0.16f, 0.4f), skin, new Vector3(0f, 0f, 38f));
            CreatePart("LeftLeg", PrimitiveType.Cube, _visualRoot, new Vector3(-0.12f, 0.02f, -0.16f), new Vector3(0.12f, 0.18f, 0.3f), pants, new Vector3(-12f, 0f, -10f));
            CreatePart("RightLeg", PrimitiveType.Cube, _visualRoot, new Vector3(0.12f, 0.02f, -0.16f), new Vector3(0.12f, 0.18f, 0.3f), pants, new Vector3(-12f, 0f, 10f));
            CreatePart("SpineWound", PrimitiveType.Cube, _visualRoot, new Vector3(0f, 0.28f, 0.12f), new Vector3(0.14f, 0.08f, 0.18f), wound, new Vector3(22f, 0f, 0f));
        }

        private IEnumerator VanishRoutine(float duration)
        {
            _isVanishing = true;
            CreateDeathBurst();

            Vector3 startScale = _visualRoot != null ? _visualRoot.localScale : Vector3.one;
            Vector3 startPosition = _visualRoot != null ? _visualRoot.localPosition : Vector3.zero;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float normalized = Mathf.Clamp01(timer / duration);
                float eased = 1f - Mathf.Pow(1f - normalized, 2f);

                if (_visualRoot != null)
                {
                    _visualRoot.localScale = Vector3.Lerp(startScale, Vector3.zero, eased);
                    _visualRoot.localPosition = Vector3.Lerp(startPosition, startPosition + new Vector3(0f, 0.28f, 0f), eased);
                }

                UpdateDeathBurst(normalized);
                yield return null;
            }

            if (_visualRoot != null)
                _visualRoot.gameObject.SetActive(false);

            if (_burstRoot != null)
                _burstRoot.gameObject.SetActive(false);
        }

        private void CreateDeathBurst()
        {
            _burstRoot = transform.Find(DeathBurstRootName);
            if (_burstRoot == null)
            {
                _burstRoot = new GameObject(DeathBurstRootName).transform;
                _burstRoot.SetParent(transform, false);
            }

            for (int i = _burstRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_burstRoot.GetChild(i).gameObject);
            }

            _burstParticles.Clear();
            _burstVelocities.Clear();

            Material burstMaterial = CreateMaterial(new Color(0.72f, 0.78f, 0.82f, 1f), 0.01f, 0.2f);
            int burstCount = _isBrute ? 16 : 12;

            for (int i = 0; i < burstCount; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                particle.name = $"Burst_{i}";
                particle.transform.SetParent(_burstRoot, false);
                particle.transform.localPosition = new Vector3(Random.Range(-0.18f, 0.18f), Random.Range(0.12f, 0.78f), Random.Range(-0.12f, 0.22f));
                particle.transform.localRotation = Random.rotationUniform;
                particle.transform.localScale = Vector3.one * Random.Range(0.04f, 0.08f);

                Collider collider = particle.GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);

                Renderer renderer = particle.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = burstMaterial;

                _burstParticles.Add(particle.transform);
                _burstVelocities.Add(new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(0.6f, 1.4f), Random.Range(0.1f, 0.8f)));
            }
        }

        private void UpdateDeathBurst(float normalized)
        {
            for (int i = 0; i < _burstParticles.Count; i++)
            {
                Transform particle = _burstParticles[i];
                if (particle == null)
                    continue;

                Vector3 velocity = _burstVelocities[i];
                particle.localPosition += velocity * Time.deltaTime * (1f - (normalized * 0.35f));
                particle.localScale = Vector3.Lerp(particle.localScale, Vector3.zero, normalized * 0.2f);
                particle.Rotate(new Vector3(120f, 180f, 90f) * Time.deltaTime);
            }
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
