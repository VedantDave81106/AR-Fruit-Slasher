using UnityEngine;
using UnityEngine.UI;

namespace ARFruitSlasher.UI
{
    public class RearAlertUI : MonoBehaviour
    {
        public static RearAlertUI Instance { get; private set; }

        [Header("UI Elements (Optional - built dynamically if unassigned)")]
        [SerializeField] private GameObject alertPanel;
        [SerializeField] private Text alertText;
        [SerializeField] private Text directionArrowText;

        private bool isActive = false;
        private float pulseTimer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureUIExists();
            HideAlert();
        }

        private void EnsureUIExists()
        {
            if (alertPanel == null)
            {
                // Find or create dynamically inside canvas
                Transform panelTransform = transform.Find("AlertPanel");
                if (panelTransform != null)
                {
                    alertPanel = panelTransform.gameObject;
                    alertText = alertPanel.GetComponentInChildren<Text>();
                }
                else
                {
                    CreateDynamicAlertUI();
                }
            }
        }

        private void CreateDynamicAlertUI()
        {
            alertPanel = new GameObject("AlertPanel");
            alertPanel.transform.SetParent(transform, false);

            RectTransform rect = alertPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.82f);
            rect.anchorMax = new Vector2(0.5f, 0.82f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360, 70);

            Image bg = alertPanel.AddComponent<Image>();
            bg.color = new Color(0.85f, 0.1f, 0.1f, 0.65f);

            GameObject textObj = new GameObject("AlertText");
            textObj.transform.SetParent(alertPanel.transform, false);
            RectTransform tr = textObj.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.sizeDelta = Vector2.zero;

            alertText = textObj.AddComponent<Text>();
            alertText.text = "⚠️ BEHIND YOU! ⚠️";
            alertText.alignment = TextAnchor.MiddleCenter;
            alertText.fontSize = 26;
            alertText.fontStyle = FontStyle.Bold;
            alertText.color = Color.yellow;
            alertText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (alertText.font == null) alertText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        public void ShowAlert()
        {
            isActive = true;
            if (alertPanel != null) alertPanel.SetActive(true);
        }

        public void HideAlert()
        {
            isActive = false;
            if (alertPanel != null) alertPanel.SetActive(false);
        }

        public void UpdateDirection(float sign, bool isBehind, float angleDegrees)
        {
            if (!isActive || alertText == null) return;

            pulseTimer += Time.deltaTime * 8f;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;

            string arrow;
            if (isBehind)
            {
                arrow = sign > 0 ? "⚠️ TURN RIGHT (BEHIND!) ↪️" : "⚠️ TURN LEFT (BEHIND!) ↩️";
            }
            else
            {
                arrow = sign > 0 ? "⚠️ LOOK RIGHT ➡️" : "⚠️ LOOK LEFT ⬅️";
            }

            alertText.text = $"{arrow}\n[{Mathf.RoundToInt(angleDegrees)}°]";
            alertText.color = Color.Lerp(Color.yellow, Color.white, pulse);
        }
    }
}
