using System.Collections.Generic;
using UnityEngine;
#if AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace ARFruitSlasher.AR
{
    public class ARPlacementManager : MonoBehaviour
    {
        public static ARPlacementManager Instance { get; private set; }

        [Header("Ground Level Calibration")]
        [SerializeField] private float defaultGroundOffset = 1.3f; // Meters below camera
        private float groundHeight;
        private bool isGroundCalibrated = false;

        public float GroundHeight => groundHeight;
        public bool IsGroundCalibrated => isGroundCalibrated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Auto-calibrate based on default camera height offset
            CalibrateDefaultGround();
        }

        public void CalibrateDefaultGround()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                groundHeight = cam.transform.position.y - defaultGroundOffset;
            }
            else
            {
                groundHeight = -1.3f;
            }
            isGroundCalibrated = true;
        }

        public void SetGroundHeight(float y)
        {
            groundHeight = y;
            isGroundCalibrated = true;
            Debug.Log($"[ARPlacementManager] Ground calibrated to Y = {groundHeight:F2}");
        }

        private void Update()
        {
            // If camera moves significantly above calibrated height, re-adjust if not fixed
            Camera cam = Camera.main;
            if (cam != null && !isGroundCalibrated)
            {
                groundHeight = cam.transform.position.y - defaultGroundOffset;
            }
        }
    }
}
