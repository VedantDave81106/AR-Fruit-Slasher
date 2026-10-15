using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ARFruitSlasher.Core;

namespace ARFruitSlasher.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Text References")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text livesText;
        [SerializeField] private Text arcText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text announcementText;

        [Header("Game Over Screen")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Button restartButton;

        private Coroutine comboCoroutine;
        private Coroutine announcementCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureCanvasAndElements();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnLivesChanged += UpdateLives;
                GameManager.Instance.OnComboTriggered += ShowCombo;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.OnDifficultyChanged += ShowDifficultyCheckpoint;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnLivesChanged -= UpdateLives;
                GameManager.Instance.OnComboTriggered -= ShowCombo;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }

            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.OnDifficultyChanged -= ShowDifficultyCheckpoint;
            }
        }

        private void EnsureCanvasAndElements()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                gameObject.AddComponent<GraphicRaycaster>();
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Top HUD Bar
            if (scoreText == null)
            {
                scoreText = CreateTextElement("ScoreText", new Vector2(0.05f, 0.95f), new Vector2(0.45f, 0.98f), "SCORE: 0", 38, TextAnchor.MiddleLeft, defaultFont);
            }

            if (livesText == null)
            {
                livesText = CreateTextElement("LivesText", new Vector2(0.55f, 0.95f), new Vector2(0.95f, 0.98f), "❤️❤️❤️", 38, TextAnchor.MiddleRight, defaultFont);
            }

            if (arcText == null)
            {
                arcText = CreateTextElement("ArcText", new Vector2(0.05f, 0.91f), new Vector2(0.95f, 0.94f), "ARC: 100° (Level 1)", 28, TextAnchor.MiddleCenter, defaultFont);
                arcText.color = new Color(0.3f, 0.9f, 1f);
            }

            if (comboText == null)
            {
                comboText = CreateTextElement("ComboText", new Vector2(0.1f, 0.70f), new Vector2(0.9f, 0.78f), "", 46, TextAnchor.MiddleCenter, defaultFont);
                comboText.color = Color.yellow;
            }

            if (announcementText == null)
            {
                announcementText = CreateTextElement("AnnouncementText", new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.55f), "", 44, TextAnchor.MiddleCenter, defaultFont);
                announcementText.color = new Color(1f, 0.8f, 0.2f);
            }

            // Game Over Panel
            if (gameOverPanel == null)
            {
                CreateGameOverUI(defaultFont);
            }
        }

        private Text CreateTextElement(string name, Vector2 anchorMin, Vector2 anchorMax, string initialText, int fontSize, TextAnchor anchor, Font font)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Text txt = obj.AddComponent<Text>();
            txt.text = initialText;
            txt.fontSize = fontSize;
            txt.font = font;
            txt.alignment = anchor;
            txt.color = Color.white;
            txt.fontStyle = FontStyle.Bold;

            // Subtle drop shadow
            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.8f);
            shadow.effectDistance = new Vector2(2, -2);

            return txt;
        }

        private void CreateGameOverUI(Font font)
        {
            gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(transform, false);

            RectTransform rect = gameOverPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = gameOverPanel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.08f, 0.88f);

            Text title = CreateTextElement("GOTitle", new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.75f), "GAME OVER", 54, TextAnchor.MiddleCenter, font);
            title.color = new Color(1f, 0.2f, 0.2f);
            title.transform.SetParent(gameOverPanel.transform, false);

            finalScoreText = CreateTextElement("FinalScoreText", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.62f), "Score: 0", 38, TextAnchor.MiddleCenter, font);
            finalScoreText.transform.SetParent(gameOverPanel.transform, false);

            bestScoreText = CreateTextElement("BestScoreText", new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.54f), "Best: 0", 32, TextAnchor.MiddleCenter, font);
            bestScoreText.color = Color.yellow;
            bestScoreText.transform.SetParent(gameOverPanel.transform, false);

            // Restart Button
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(gameOverPanel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.25f, 0.32f);
            btnRect.anchorMax = new Vector2(0.75f, 0.40f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.75f, 0.35f);

            restartButton = btnObj.AddComponent<Button>();

            Text btnText = CreateTextElement("BtnText", Vector2.zero, Vector2.one, "PLAY AGAIN", 36, TextAnchor.MiddleCenter, font);
            btnText.transform.SetParent(btnObj.transform, false);
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"SCORE: {score}";
        }

        private void UpdateLives(int lives)
        {
            if (livesText != null)
            {
                string hearts = "";
                for (int i = 0; i < lives; i++) hearts += "❤️ ";
                if (lives <= 0) hearts = "💀";
                livesText.text = hearts;
            }
        }

        private void ShowCombo(int comboCount, int bonusPoints)
        {
            if (comboText == null) return;
            if (comboCoroutine != null) StopCoroutine(comboCoroutine);

            string title = comboCount >= 4 ? "ULTRA COMBO!" : comboCount == 3 ? "TRIPLE COMBO!" : "DOUBLE SLICE!";
            comboText.text = $"{title}\n+{bonusPoints} PTS";
            comboCoroutine = StartCoroutine(AnimateTextPulse(comboText, 1.2f));
        }

        private void ShowDifficultyCheckpoint(DifficultyTier tier)
        {
            if (arcText != null)
            {
                arcText.text = $"{tier.tierName}";
            }

            if (tier.minScore > 0)
            {
                if (announcementText != null)
                {
                    if (announcementCoroutine != null) StopCoroutine(announcementCoroutine);
                    announcementText.text = $"🔥 CHECKPOINT! 🔥\n{tier.arcDegrees}° ARC UNLOCKED!";
                    announcementCoroutine = StartCoroutine(AnimateTextPulse(announcementText, 2.0f));
                }

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayCheckpoint();
                }
            }
        }

        private IEnumerator AnimateTextPulse(Text txt, float duration)
        {
            float elapsed = 0f;
            Vector3 origScale = Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float s = 1.0f + Mathf.Sin(elapsed * Mathf.PI * 4f) * 0.15f;
                txt.transform.localScale = origScale * s;
                yield return null;
            }

            txt.transform.localScale = origScale;
            txt.text = "";
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(true);
                    if (finalScoreText != null && GameManager.Instance != null)
                    {
                        finalScoreText.text = $"FINAL SCORE: {GameManager.Instance.Score}";
                    }
                    if (bestScoreText != null && GameManager.Instance != null)
                    {
                        bestScoreText.text = $"BEST SCORE: {GameManager.Instance.HighScore}";
                    }
                }
            }
            else
            {
                if (gameOverPanel != null)
                {
                    gameOverPanel.SetActive(false);
                }
            }
        }

        public void ShowFruitBadge(string icon, string fruitName)
        {
            if (announcementText != null && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                announcementText.text = $"{icon} {fruitName}";
                if (announcementCoroutine != null) StopCoroutine(announcementCoroutine);
                announcementCoroutine = StartCoroutine(AnimateFruitBadge(announcementText, 1.2f));
            }
        }

        private IEnumerator AnimateFruitBadge(Text txt, float duration)
        {
            float elapsed = 0f;
            Vector3 origScale = Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float s = 1.0f + Mathf.Sin(progress * Mathf.PI) * 0.25f;
                txt.transform.localScale = origScale * s;
                yield return null;
            }

            txt.transform.localScale = origScale;
            txt.text = "";
        }

        public void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
    }
}
