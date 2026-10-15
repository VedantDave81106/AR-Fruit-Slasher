using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ARFruitSlasher.Core;

namespace ARFruitSlasher.AR
{
    public class ARFaceScanManager : MonoBehaviour
    {
        public static ARFaceScanManager Instance { get; private set; }

        [Header("Scan Settings")]
        [SerializeField] private float requiredScanDuration = 2.0f; // Seconds to complete face scan
        [SerializeField] private bool autoSimulateInEditor = true;

        [Header("UI References (Optional - dynamically found/built)")]
        [SerializeField] private GameObject faceScanPanel;
        [SerializeField] private Text statusText;
        [SerializeField] private Image progressBar;
        [SerializeField] private Text percentText;
        [SerializeField] private Button bypassButton;

        private float scanTimer = 0f;
        private bool isFaceDetected = false;
        private bool isScanCompleted = false;

        public event Action OnFaceScanCompleted;
        public bool IsScanCompleted => isScanCompleted;

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
            EnsureUIReferences();
            StartFaceScanning();
        }

        private void EnsureUIReferences()
        {
            if (faceScanPanel == null)
            {
                Transform panel = transform.Find("FaceScanPanel");
                if (panel != null) faceScanPanel = panel.gameObject;
            }

            if (bypassButton != null)
            {
                bypassButton.onClick.AddListener(BypassScan);
            }
        }

        public void StartFaceScanning()
        {
            isScanCompleted = false;
            scanTimer = 0f;
            isFaceDetected = false;

            if (faceScanPanel != null) faceScanPanel.SetActive(true);
            UpdateStatus("INITIALIZING AR SENSORS...", 0f);

#if UNITY_EDITOR
            if (autoSimulateInEditor)
            {
                StartCoroutine(EditorSimulateScanRoutine());
            }
#endif
        }

        public void OnARFaceUpdated(bool faceInView)
        {
            if (isScanCompleted) return;
            isFaceDetected = faceInView;
        }

        private void Update()
        {
            if (isScanCompleted) return;

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                BypassScan();
            }
#endif

            if (isFaceDetected)
            {
                scanTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(scanTimer / requiredScanDuration);

                string msg = progress < 0.4f ? "DETECTING FACIAL LANDMARKS..." :
                             progress < 0.8f ? "VERIFYING AR MESH RETICLE..." : "CONFIRMING BIOMETRIC LOCK...";

                UpdateStatus(msg, progress);

                if (scanTimer >= requiredScanDuration)
                {
                    CompleteScan();
                }
            }
        }

        private IEnumerator EditorSimulateScanRoutine()
        {
            yield return new WaitForSeconds(0.6f);
            isFaceDetected = true;
        }

        public void BypassScan()
        {
            if (isScanCompleted) return;
            isFaceDetected = true;
            scanTimer = requiredScanDuration;
            UpdateStatus("⚡ BIOMETRIC OVERRIDE CONFIRMED!", 1.0f);
            CompleteScan();
        }

        private void CompleteScan()
        {
            isScanCompleted = true;
            UpdateStatus("✅ AGENT VERIFIED: AR ARENA UNLOCKED!", 1.0f);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCheckpoint();
            }

            StartCoroutine(DismissRoutine());
        }

        private IEnumerator DismissRoutine()
        {
            yield return new WaitForSeconds(1.0f);

            if (faceScanPanel != null)
            {
                faceScanPanel.SetActive(false);
            }

            OnFaceScanCompleted?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        private void UpdateStatus(string message, float progress)
        {
            if (statusText != null) statusText.text = message;
            if (progressBar != null) progressBar.fillAmount = progress;
            if (percentText != null) percentText.text = $"{Mathf.RoundToInt(progress * 100f)}% ANALYZED";
        }
    }
}
