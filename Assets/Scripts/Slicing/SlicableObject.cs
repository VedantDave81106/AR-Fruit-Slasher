using UnityEngine;
using ARFruitSlasher.Core;
using ARFruitSlasher.Procedural;

namespace ARFruitSlasher.Slicing
{
    public enum ObjectType
    {
        Watermelon,
        Orange,
        Apple,
        GoldenPineapple,
        Bomb
    }

    public class SlicableObject : MonoBehaviour
    {
        [Header("Object Config")]
        [SerializeField] private ObjectType objectType = ObjectType.Watermelon;
        [SerializeField] private int points = 10;
        [SerializeField] private Color juiceColor = new Color(0.9f, 0.1f, 0.2f, 1f); // Watermelon red
        [SerializeField] private float separationForce = 3.5f;

        [Header("Optional Custom Halves (If not using procedural mesh)")]
        [SerializeField] private GameObject leftHalfPrefab;
        [SerializeField] private GameObject rightHalfPrefab;
        [SerializeField] private GameObject juiceSplashPrefab;
        [SerializeField] private GameObject floatingScorePrefab;

        private Rigidbody rb;
        private bool isSliced = false;
        private bool hasBeenAlerted = false;

        public ObjectType Type => objectType;
        public bool IsSliced => isSliced;
        public bool HasBeenAlerted
        {
            get => hasBeenAlerted;
            set => hasBeenAlerted = value;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        }

        public void Slice(Vector3 sliceNormal, Vector3 sliceOrigin)
        {
            if (isSliced) return;
            isSliced = true;

            if (objectType == ObjectType.Bomb)
            {
                HandleBombExplosion();
                return;
            }

            HandleFruitSliced(sliceNormal, sliceOrigin);
        }

        private void HandleFruitSliced(Vector3 sliceNormal, Vector3 sliceOrigin)
        {
            // Register score & combo in GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterFruitSlice(transform.position, points);
            }

            // Play slice audio
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySlice(transform.position);
            }

            // Spawn juice particle splatter
            SpawnJuiceParticles();

            // Spawn floating score popup
            SpawnFloatingScore();

            // Spawn split halves with outward impulse physics
            SpawnHalves(sliceNormal);

            // Destroy original fruit
            Destroy(gameObject);
        }

        private void HandleBombExplosion()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayExplosion();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterBombHit();
            }

            // Create explosion flash particles
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.transform.position = transform.position;
            flash.transform.localScale = Vector3.one * 1.5f;
            Renderer r = flash.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = ProceduralMeshBuilder.CreateColorMaterial(new Color(1f, 0.4f, 0f, 0.8f), "ExplosionMat");
            }
            Destroy(flash.GetComponent<Collider>());
            Destroy(flash, 0.15f);

            Destroy(gameObject);
        }

        private void SpawnHalves(Vector3 sliceNormal)
        {
            Vector3 pushDir = Vector3.Cross(sliceNormal, Vector3.up).normalized;
            if (pushDir.sqrMagnitude < 0.01f) pushDir = Vector3.right;

            Vector3 inheritedVelocity = rb != null ? rb.velocity : Vector3.up * 2f;

            // Left half
            GameObject leftHalf = CreateHalf(isLeft: true);
            SetupHalfPhysics(leftHalf, -pushDir, inheritedVelocity);

            // Right half
            GameObject rightHalf = CreateHalf(isLeft: false);
            SetupHalfPhysics(rightHalf, pushDir, inheritedVelocity);
        }

        private GameObject CreateHalf(bool isLeft)
        {
            GameObject half = new GameObject(isLeft ? "Sliced_LeftHalf" : "Sliced_RightHalf");
            half.transform.position = transform.position;
            half.transform.rotation = transform.rotation;
            half.transform.localScale = transform.localScale;

            MeshFilter mf = half.AddComponent<MeshFilter>();
            mf.mesh = ProceduralMeshBuilder.CreateHemisphereMesh(0.25f, isLeft);

            MeshRenderer mr = half.AddComponent<MeshRenderer>();
            Renderer orig = GetComponent<Renderer>();
            if (orig != null && orig.sharedMaterial != null)
            {
                mr.sharedMaterial = orig.sharedMaterial;
            }
            else
            {
                mr.sharedMaterial = ProceduralMeshBuilder.CreateColorMaterial(juiceColor, "FleshMat");
            }

            return half;
        }

        private void SetupHalfPhysics(GameObject half, Vector3 pushDir, Vector3 baseVelocity)
        {
            Rigidbody halfRb = half.AddComponent<Rigidbody>();
            halfRb.velocity = baseVelocity + pushDir * separationForce + Vector3.up * 1.2f;
            halfRb.angularVelocity = new Vector3(
                Random.Range(-8f, 8f),
                Random.Range(-8f, 8f),
                Random.Range(-8f, 8f)
            );

            // Auto cleanup after falling
            Destroy(half, 2.0f);
        }

        private void SpawnJuiceParticles()
        {
            GameObject juiceGO = new GameObject("JuiceSplashEffect");
            juiceGO.transform.position = transform.position;

            ParticleSystem ps = juiceGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = juiceColor;
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.0f);
            main.startLifetime = 0.6f;
            main.gravityModifier = 1.8f;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            ParticleSystemRenderer psr = juiceGO.GetComponent<ParticleSystemRenderer>();
            psr.material = ProceduralMeshBuilder.CreateColorMaterial(juiceColor, "JuiceMat", 0.9f);

            ps.Play();
            Destroy(juiceGO, 1.2f);
        }

        private void SpawnFloatingScore()
        {
            GameObject popup = new GameObject("ScorePopup");
            popup.transform.position = transform.position + Vector3.up * 0.2f;
            var script = popup.AddComponent<UI.FloatingScore>();
            script.Initialize($"+{points}", Color.yellow);
        }

        private void Update()
        {
            // If the fruit falls below the ground / camera height (e.g. -2.5m) and wasn't sliced
            if (!isSliced && transform.position.y < -2.2f)
            {
                if (objectType != ObjectType.Bomb)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RegisterMissedFruit();
                    }
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayMiss();
                    }
                }
                Destroy(gameObject);
            }
        }
    }
}
