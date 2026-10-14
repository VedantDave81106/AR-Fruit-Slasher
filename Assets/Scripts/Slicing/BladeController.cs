using UnityEngine;
using ARFruitSlasher.Core;

namespace ARFruitSlasher.Slicing
{
    public class BladeController : MonoBehaviour
    {
        [Header("Blade Configuration")]
        [SerializeField] private float minSliceVelocity = 100f; // Screen pixels per second
        [SerializeField] private float bladeDepthFromCamera = 2.0f; // Distance in front of camera
        [SerializeField] private Color bladeColor = new Color(0.2f, 0.9f, 1f, 0.9f); // Cyan neon
        [SerializeField] private LayerMask sliceableLayer = ~0; // All layers by default

        private Camera mainCamera;
        private TrailRenderer trailRenderer;
        private SphereCollider bladeCollider;

        private Vector2 previousScreenPos;
        private Vector3 currentWorldPos;
        private bool isSlicing = false;

        public bool IsSlicing => isSlicing;

        private void Awake()
        {
            mainCamera = Camera.main;
            SetupTrail();
        }

        private void SetupTrail()
        {
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }

            trailRenderer.time = 0.18f;
            trailRenderer.startWidth = 0.08f;
            trailRenderer.endWidth = 0.005f;

            // Simple glowing material
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlit == null) unlit = Shader.Find("Unlit/Color");
            if (unlit == null) unlit = Shader.Find("Mobile/Particles/Additive");

            Material bladeMat = new Material(unlit) { name = "BladeTrailMat" };
            bladeMat.color = bladeColor;
            if (bladeMat.HasProperty("_BaseColor")) bladeMat.SetColor("_BaseColor", bladeColor);
            trailRenderer.material = bladeMat;

            trailRenderer.startColor = bladeColor;
            trailRenderer.endColor = new Color(bladeColor.r, bladeColor.g, bladeColor.b, 0f);
            trailRenderer.emitting = false;
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            Vector2 currentScreenPos = Vector2.zero;
            bool isDown = false;
            bool isHeld = false;
            bool isUp = false;

            // Touch on Mobile
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                currentScreenPos = touch.position;
                if (touch.phase == TouchPhase.Began) isDown = true;
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) isHeld = true;
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) isUp = true;
            }
            // Mouse on PC / Editor
            else
            {
                currentScreenPos = Input.mousePosition;
                if (Input.GetMouseButtonDown(0)) isDown = true;
                if (Input.GetMouseButton(0)) isHeld = true;
                if (Input.GetMouseButtonUp(0)) isUp = true;
            }

            if (isDown)
            {
                StartSlice(currentScreenPos);
            }
            else if (isHeld && isSlicing)
            {
                ContinueSlice(currentScreenPos);
            }
            else if (isUp)
            {
                StopSlice();
            }
        }

        private void StartSlice(Vector2 screenPos)
        {
            isSlicing = true;
            previousScreenPos = screenPos;
            UpdateBladePosition(screenPos);

            trailRenderer.Clear();
            trailRenderer.emitting = true;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySwish();
            }
        }

        private void ContinueSlice(Vector2 screenPos)
        {
            float deltaDistance = (screenPos - previousScreenPos).magnitude;
            float velocity = deltaDistance / Mathf.Max(Time.deltaTime, 0.001f);

            UpdateBladePosition(screenPos);

            if (velocity >= minSliceVelocity)
            {
                // Perform raycast / spherecast from camera through slice line
                PerformSliceCast(previousScreenPos, screenPos);
            }

            previousScreenPos = screenPos;
        }

        private void StopSlice()
        {
            isSlicing = false;
            trailRenderer.emitting = false;
        }

        private void UpdateBladePosition(Vector2 screenPos)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            currentWorldPos = ray.GetPoint(bladeDepthFromCamera);
            transform.position = currentWorldPos;
        }

        private void PerformSliceCast(Vector2 startScreen, Vector2 endScreen)
        {
            int steps = 5;
            for (int i = 0; i <= steps; i++)
            {
                Vector2 samplePoint = Vector2.Lerp(startScreen, endScreen, (float)i / steps);
                Ray ray = mainCamera.ScreenPointToRay(samplePoint);

                // Raycast in 3D AR space
                RaycastHit[] hits = Physics.RaycastAll(ray, 10f, sliceableLayer);
                for (int h = 0; h < hits.Length; h++)
                {
                    RaycastHit hit = hits[h];
                    SlicableObject slicable = hit.collider.GetComponentInParent<SlicableObject>();
                    if (slicable != null && !slicable.IsSliced)
                    {
                        Vector3 sliceDir = (mainCamera.ScreenToWorldPoint(new Vector3(endScreen.x, endScreen.y, bladeDepthFromCamera)) -
                                            mainCamera.ScreenToWorldPoint(new Vector3(startScreen.x, startScreen.y, bladeDepthFromCamera))).normalized;

                        slicable.Slice(sliceDir, hit.point);
                    }
                }
            }
        }
    }
}
