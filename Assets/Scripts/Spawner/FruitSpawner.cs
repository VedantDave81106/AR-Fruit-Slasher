using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARFruitSlasher.Core;
using ARFruitSlasher.Slicing;
using ARFruitSlasher.Procedural;
using ARFruitSlasher.AR;

namespace ARFruitSlasher.Spawner
{
    public class FruitSpawner : MonoBehaviour
    {
        public static FruitSpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private float minSpawnRadius = 2.0f;
        [SerializeField] private float maxSpawnRadius = 3.2f;
        [SerializeField] private float groundOffsetBelowCamera = 1.3f;
        [SerializeField] private float launchUpwardSpeed = 5.2f;
        [SerializeField] private float launchInwardSpeed = 1.2f;

        [Header("Fruit Configs")]
        [SerializeField] private FruitTypeConfig[] fruitTypes;

        private Camera mainCamera;
        private Coroutine spawnLoopCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (fruitTypes == null || fruitTypes.Length == 0)
            {
                InitDefaultConfigs();
            }
        }

        private void InitDefaultConfigs()
        {
            fruitTypes = new FruitTypeConfig[]
            {
                new FruitTypeConfig { name = "Watermelon", type = ObjectType.Watermelon, points = 10, outerColor = new Color(0.12f, 0.65f, 0.18f), fleshColor = new Color(0.95f, 0.1f, 0.2f), scale = 0.38f, weight = 1.0f },
                new FruitTypeConfig { name = "Orange",     type = ObjectType.Orange,     points = 15, outerColor = new Color(1.0f, 0.55f, 0.0f),  fleshColor = new Color(1.0f, 0.65f, 0.1f), scale = 0.28f, weight = 0.8f },
                new FruitTypeConfig { name = "Apple",      type = ObjectType.Apple,      points = 20, outerColor = new Color(0.85f, 0.12f, 0.15f), fleshColor = new Color(0.98f, 0.95f, 0.8f), scale = 0.25f, weight = 0.7f },
                new FruitTypeConfig { name = "GoldenPineapple", type = ObjectType.GoldenPineapple, points = 50, outerColor = new Color(1.0f, 0.85f, 0.1f), fleshColor = new Color(1.0f, 0.9f, 0.2f), scale = 0.35f, weight = 0.2f }
            };
        }

        private void Start()
        {
            mainCamera = Camera.main;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleGameStateChanged;
            }

            StartSpawning();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
            }
            StopSpawning();
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                StartSpawning();
            }
            else
            {
                StopSpawning();
            }
        }

        public void StartSpawning()
        {
            StopSpawning();
            spawnLoopCoroutine = StartCoroutine(SpawnRoutine());
        }

        public void StopSpawning()
        {
            if (spawnLoopCoroutine != null)
            {
                StopCoroutine(spawnLoopCoroutine);
                spawnLoopCoroutine = null;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            // Initial grace delay
            yield return new WaitForSeconds(1.0f);

            while (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                float delay = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentSpawnInterval : 2.0f;

                // Spawn 1 or sometimes 2 fruits together at higher difficulties
                int countToSpawn = 1;
                if (DifficultyManager.Instance != null && DifficultyManager.Instance.CurrentArcDegrees >= 270f && Random.value < 0.35f)
                {
                    countToSpawn = 2;
                }

                for (int i = 0; i < countToSpawn; i++)
                {
                    SpawnSingleObject();
                    if (countToSpawn > 1) yield return new WaitForSeconds(0.15f);
                }

                yield return new WaitForSeconds(delay);
            }
        }

        private void SpawnSingleObject()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // Determine spawn position around camera based on Difficulty Arc
            float arcAngle = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetRandomSpawnAngleDegrees() : Random.Range(-45f, 45f);
            float radius = Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0f;
            if (cameraForward.sqrMagnitude < 0.001f) cameraForward = Vector3.forward;
            cameraForward.Normalize();

            Quaternion rotation = Quaternion.AngleAxis(arcAngle, Vector3.up);
            Vector3 direction = rotation * cameraForward;

            Vector3 spawnPos = mainCamera.transform.position + direction * radius;
            spawnPos.y = mainCamera.transform.position.y - groundOffsetBelowCamera;

            // Check if off-screen or behind the player
            bool isBehindOrOffscreen = DifficultyManager.Instance != null && DifficultyManager.Instance.IsOffscreenOrBehind(arcAngle);

            // Determine whether to spawn a Bomb or Fruit
            float bombChance = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentBombChance : 0f;
            bool spawnBomb = Random.value < bombChance;

            GameObject spawnedGO;
            if (spawnBomb)
            {
                spawnedGO = CreateBombObject(spawnPos);
            }
            else
            {
                FruitTypeConfig cfg = PickRandomFruitConfig();
                spawnedGO = CreateFruitObject(spawnPos, cfg);
            }

            // Launch velocity toward camera eye level
            Rigidbody rb = spawnedGO.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 toPlayer = (mainCamera.transform.position - spawnPos);
                toPlayer.y = 0f;
                Vector3 horizontalImpulse = toPlayer.normalized * launchInwardSpeed;

                float speedMult = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentSpeedMultiplier : 1.0f;
                Vector3 velocity = (Vector3.up * launchUpwardSpeed + horizontalImpulse) * speedMult;
                rb.velocity = velocity;

                // Add random spin
                rb.angularVelocity = new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f)
                );
            }

            // Trigger alert if behind player!
            if (isBehindOrOffscreen)
            {
                if (ARWarningIndicator.Instance != null)
                {
                    ARWarningIndicator.Instance.TriggerAlert(spawnPos);
                }
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayAlert();
                }
            }
        }

        private FruitTypeConfig PickRandomFruitConfig()
        {
            float totalWeight = 0f;
            foreach (var f in fruitTypes) totalWeight += f.weight;

            float r = Random.Range(0f, totalWeight);
            float accum = 0f;
            foreach (var f in fruitTypes)
            {
                accum += f.weight;
                if (r <= accum) return f;
            }
            return fruitTypes[0];
        }

        private GameObject CreateFruitObject(Vector3 pos, FruitTypeConfig cfg)
        {
            GameObject go = new GameObject($"Fruit_{cfg.name}");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * cfg.scale;
            go.tag = "Fruit";

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = ProceduralMeshBuilder.CreateSphereMesh(0.5f);

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = ProceduralMeshBuilder.CreateColorMaterial(cfg.outerColor, $"{cfg.name}_Mat", 0.6f);

            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.radius = 0.5f;

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 1.0f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            SlicableObject slicable = go.AddComponent<SlicableObject>();

            return go;
        }

        private GameObject CreateBombObject(Vector3 pos)
        {
            GameObject go = new GameObject("Hazard_Bomb");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.32f;
            go.tag = "Bomb";

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = ProceduralMeshBuilder.CreateSphereMesh(0.5f);

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = ProceduralMeshBuilder.CreateColorMaterial(new Color(0.12f, 0.12f, 0.14f), "BombMat", 0.8f);

            // Little fuse cylinder on top
            GameObject fuse = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fuse.transform.SetParent(go.transform, false);
            fuse.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            fuse.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
            Renderer fr = fuse.GetComponent<Renderer>();
            if (fr != null) fr.material = ProceduralMeshBuilder.CreateColorMaterial(Color.yellow, "FuseMat");
            Destroy(fuse.GetComponent<Collider>());

            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.radius = 0.5f;

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 1.2f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            SlicableObject slicable = go.AddComponent<SlicableObject>();

            return go;
        }
    }
}
