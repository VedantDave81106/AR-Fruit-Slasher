using UnityEngine;
using ARFruitSlasher.UI;

namespace ARFruitSlasher.AR
{
    public class ARWarningIndicator : MonoBehaviour
    {
        public static ARWarningIndicator Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float warningDuration = 2.0f;
        [SerializeField] private float dismissAngleThreshold = 40f; // degrees

        private Camera mainCamera;
        private Vector3 activeThreatPos;
        private float threatExpireTime = -1f;
        private bool isThreatActive = false;

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
            mainCamera = Camera.main;
        }

        public void TriggerAlert(Vector3 threatWorldPos)
        {
            mainCamera = Camera.main;
            activeThreatPos = threatWorldPos;
            threatExpireTime = Time.time + warningDuration;
            isThreatActive = true;

            if (RearAlertUI.Instance != null)
            {
                RearAlertUI.Instance.ShowAlert();
            }
        }

        private void Update()
        {
            if (!isThreatActive) return;

            if (Time.time > threatExpireTime)
            {
                DismissAlert();
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // Calculate horizontal angle between camera forward and threat
            Vector3 toThreat = activeThreatPos - mainCamera.transform.position;
            toThreat.y = 0f;

            Vector3 camForward = mainCamera.transform.forward;
            camForward.y = 0f;

            float angle = Vector3.Angle(camForward, toThreat);

            // If player turned and is now facing the threat, dismiss
            if (angle <= dismissAngleThreshold)
            {
                DismissAlert();
                return;
            }

            // Calculate whether threat is to the left or right of camera
            float sign = Mathf.Sign(Vector3.Cross(camForward, toThreat).y); // +1 = Right, -1 = Left
            bool isBehind = angle > 110f;

            if (RearAlertUI.Instance != null)
            {
                RearAlertUI.Instance.UpdateDirection(sign, isBehind, angle);
            }
        }

        private void DismissAlert()
        {
            isThreatActive = false;
            if (RearAlertUI.Instance != null)
            {
                RearAlertUI.Instance.HideAlert();
            }
        }
    }
}
