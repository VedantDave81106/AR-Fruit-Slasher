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
                new FruitTypeConfig { name = "Watermelon", icon = "🍉", type = ObjectType.Watermelon, points = 30, outerColor = new Color(0.12f, 0.65f, 0.18f), fleshColor = new Color(0.95f, 0.1f, 0.2f), scale = 0.38f, weight = 1.0f },
                new FruitTypeConfig { name = "Orange",     icon = "🍊", type = ObjectType.Orange,     points = 15, outerColor = new Color(1.0f, 0.55f, 0.0f),  fleshColor = new Color(1.0f, 0.65f, 0.1f), scale = 0.28f, weight = 0.8f },
                new FruitTypeConfig { name = "Apple",      icon = "🍎", type = ObjectType.Apple,      points = 20, outerColor = new Color(0.85f, 0.12f, 0.15f), fleshColor = new Color(0.98f, 0.95f, 0.8f), scale = 0.25f, weight = 0.7f },
                new FruitTypeConfig { name = "GoldenPineapple", icon = "🍍", type = ObjectType.GoldenPineapple, points = 50, outerColor = new Color(1.0f, 0.85f, 0.1f), fleshColor = new Color(1.0f, 0.9f, 0.2f), scale = 0.35f, weight = 0.2f }
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
                if (UI.UIManager.Instance != null) UI.UIManager.Instance.ShowFruitBadge("💣", "BOMB HAZARD");
            }
            else
            {
                FruitTypeConfig cfg = PickRandomFruitConfig();
                spawnedGO = CreateFruitObject(spawnPos, cfg);
                if (UI.UIManager.Instance != null) UI.UIManager.Instance.ShowFruitBadge(cfg.icon, cfg.name);
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
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            switch (cfg.type)
            {
                case ObjectType.Watermelon:
                    mf.mesh = ProceduralMeshBuilder.CreateSphereMesh(0.5f);
                    go.transform.localScale = new Vector3(cfg.scale * 1.25f, cfg.scale * 0.95f, cfg.scale * 0.95f);
                    Texture2D melonTex = ProceduralMeshBuilder.CreateWatermelonTexture();
                    mr.material = ProceduralMeshBuilder.CreateColorMaterial(Color.white, "Watermelon_Rind", 0.4f, melonTex);
                    break;

                case ObjectType.Apple:
                    mf.mesh = ProceduralMeshBuilder.CreateAppleMesh(0.5f);
                    mr.material = ProceduralMeshBuilder.CreateColorMaterial(cfg.outerColor, "Apple_Skin", 0.35f);
                    // Add brown stem
                    GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stem.transform.SetParent(go.transform, false);
                    stem.transform.localPosition = new Vector3(0f, 0.52f, 0f);
                    stem.transform.localScale = new Vector3(0.04f, 0.12f, 0.04f);
                    stem.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
                    Renderer stemR = stem.GetComponent<Renderer>();
                    if (stemR != null) stemR.material = ProceduralMeshBuilder.CreateColorMaterial(new Color(0.35f, 0.18f, 0.08f), "StemMat");
                    Destroy(stem.GetComponent<Collider>());
                    break;

                case ObjectType.Orange:
                    mf.mesh = ProceduralMeshBuilder.CreateSphereMesh(0.5f);
                    mr.material = ProceduralMeshBuilder.CreateColorMaterial(cfg.outerColor, "Orange_Skin", 0.6f);
                    // Add small green leaf
                    GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    leaf.transform.SetParent(go.transform, false);
                    leaf.transform.localPosition = new Vector3(0.08f, 0.5f, 0f);
                    leaf.transform.localScale = new Vector3(0.08f, 0.02f, 0.16f);
                    leaf.transform.localRotation = Quaternion.Euler(0f, 25f, -20f);
                    Renderer leafR = leaf.GetComponent<Renderer>();
                    if (leafR != null) leafR.material = ProceduralMeshBuilder.CreateColorMaterial(new Color(0.12f, 0.55f, 0.18f), "LeafMat");
                    Destroy(leaf.GetComponent<Collider>());
                    break;

                case ObjectType.GoldenPineapple:
                    mf.mesh = ProceduralMeshBuilder.CreateSphereMesh(0.5f);
                    go.transform.localScale = new Vector3(cfg.scale * 0.85f, cfg.scale * 1.35f, cfg.scale * 0.85f);
                    mr.material = ProceduralMeshBuilder.CreateColorMaterial(new Color(1.0f, 0.75f, 0.1f), "Pineapple_Gold", 0.7f);
                    // Top green spiky leaves
                    for (int l = 0; l < 5; l++)
                    {
                        GameObject pLeaf = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        pLeaf.transform.SetParent(go.transform, false);
                        float angle = l * 72f * Mathf.Deg2Rad;
                        pLeaf.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.12f, 0.62f, Mathf.Sin(angle) * 0.12f);
                        pLeaf.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);
                        pLeaf.transform.localRotation = Quaternion.Euler(Mathf.Sin(angle) * 25f, 0f, -Mathf.Cos(angle) * 25f);
                        Renderer plr = pLeaf.GetComponent<Renderer>();
                        if (plr != null) plr.material = ProceduralMeshBuilder.CreateColorMaterial(new Color(0.15f, 0.6f, 0.2f), "CrownLeafMat");
                        Destroy(pLeaf.GetComponent<Collider>());
                    }
                    break;

                default:
                    mf.mesh = ProceduralMeshBuilder.CreateSphereMesh(0.5f);
                    mr.material = ProceduralMeshBuilder.CreateColorMaterial(cfg.outerColor, $"{cfg.name}_Mat", 0.6f);
                    break;
            }

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

            // Brass cap
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.transform.SetParent(go.transform, false);
            cap.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            cap.transform.localScale = new Vector3(0.18f, 0.08f, 0.18f);
            Renderer cr = cap.GetComponent<Renderer>();
            if (cr != null) cr.material = ProceduralMeshBuilder.CreateColorMaterial(new Color(0.75f, 0.65f, 0.2f), "CapMat");
            Destroy(cap.GetComponent<Collider>());

            // Fuse cylinder on top
            GameObject fuse = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fuse.transform.SetParent(go.transform, false);
            fuse.transform.localPosition = new Vector3(0.04f, 0.60f, 0f);
            fuse.transform.localScale = new Vector3(0.06f, 0.16f, 0.06f);
            fuse.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
            Renderer fr = fuse.GetComponent<Renderer>();
            if (fr != null) fr.material = ProceduralMeshBuilder.CreateColorMaterial(new Color(0.85f, 0.75f, 0.5f), "FuseMat");
            Destroy(fuse.GetComponent<Collider>());

            // Glowing flame spark
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.transform.SetParent(go.transform, false);
            spark.transform.localPosition = new Vector3(0.08f, 0.70f, 0f);
            spark.transform.localScale = Vector3.one * 0.1f;
            Renderer sr = spark.GetComponent<Renderer>();
            if (sr != null) sr.material = ProceduralMeshBuilder.CreateColorMaterial(Color.red, "SparkMat");
            Destroy(spark.GetComponent<Collider>());

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
