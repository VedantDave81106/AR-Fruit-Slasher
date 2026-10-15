using UnityEngine;
using ARFruitSlasher.Slicing;
using ARFruitSlasher.Core;

namespace ARFruitSlasher.AR
{
    /// <summary>
    /// Tracks optical hand and finger gestures from the device camera feed
    /// to enable touchless mid-air fruit slicing in AR/MR.
    /// </summary>
    public class ARCameraHandTracker : MonoBehaviour
    {
        public static ARCameraHandTracker Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableCameraHandTracking = true;
        [SerializeField] private float motionSensitivity = 0.18f;
        [SerializeField] private int sampleResolution = 64; // Downsampled grid for 60fps tracking

        private WebCamTexture webcamTexture;
        private Color32[] prevPixels;
        private BladeController bladeController;
        private Vector2 prevFingerScreenPos;
        private bool isTrackingHand = false;

        public bool IsTrackingHand => isTrackingHand;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            bladeController = FindObjectOfType<BladeController>();

            if (enableCameraHandTracking && WebCamTexture.devices.Length > 0)
            {
                webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, sampleResolution, sampleResolution, 30);
                webcamTexture.Play();
            }
        }

        private void OnDestroy()
        {
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
            }
        }

        private void Update()
        {
            if (!enableCameraHandTracking || webcamTexture == null || !webcamTexture.isPlaying) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            ProcessCameraMotion();
        }

        private void ProcessCameraMotion()
        {
            if (!webcamTexture.didUpdateThisFrame) return;

            Color32[] currentPixels = webcamTexture.GetPixels32();
            if (prevPixels == null || prevPixels.Length != currentPixels.Length)
            {
                prevPixels = new Color32[currentPixels.Length];
                System.Array.Copy(currentPixels, prevPixels, currentPixels.Length);
                return;
            }

            int w = webcamTexture.width;
            int h = webcamTexture.height;
            int motionCount = 0;
            float topY = 0f;
            float topX = 0f;
            float minY = 1.0f;

            for (int i = 0; i < currentPixels.Length; i += 4)
            {
                Color32 c1 = currentPixels[i];
                Color32 c2 = prevPixels[i];

                int diff = Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b);
                if (diff > motionSensitivity * 765) // 255 * 3
                {
                    int px = i % w;
                    int py = i / w;

                    float normX = 1.0f - ((float)px / w); // mirror selfie
                    float normY = (float)py / h;

                    motionCount++;
                    if (normY < minY)
                    {
                        minY = normY;
                        topX = normX;
                        topY = normY;
                    }
                }
            }

            System.Array.Copy(currentPixels, prevPixels, currentPixels.Length);

            if (motionCount > 15)
            {
                isTrackingHand = true;
                Vector2 currentScreenPos = new Vector2(topX * Screen.width, topY * Screen.height);

                if (prevFingerScreenPos != Vector2.zero)
                {
                    float dist = Vector2.Distance(currentScreenPos, prevFingerScreenPos);
                    if (dist > 30f && bladeController != null)
                    {
                        // Simulate swipe slice from finger motion in air
                        bladeController.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(currentScreenPos.x, currentScreenPos.y, 2.0f));
                    }
                }

                prevFingerScreenPos = currentScreenPos;
            }
            else
            {
                isTrackingHand = false;
                prevFingerScreenPos = Vector2.zero;
            }
        }
    }
}
