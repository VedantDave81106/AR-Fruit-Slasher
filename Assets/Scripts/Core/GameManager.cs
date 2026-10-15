using System;
using System.Collections;
using UnityEngine;

namespace ARFruitSlasher.Core
{
    public enum GameState
    {
        FaceScanning,
        Ready,
        Playing,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int maxLives = 3;
        [SerializeField] private float comboResetTime = 0.4f;

        [Header("State")]
        private GameState currentState = GameState.FaceScanning;
        private int currentScore = 0;
        private int highScore = 0;
        private int currentLives;

        // Combo system
        private int currentComboStreak = 0;
        private float lastSliceTime = -1f;

        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int, int> OnComboTriggered; // (comboCount, bonusPoints)
        public event Action<GameState> OnStateChanged;
        public event Action OnCheckpointReached;

        public GameState CurrentState => currentState;
        public int Score => currentScore;
        public int HighScore => highScore;
        public int Lives => currentLives;
        public int MaxLives => maxLives;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            highScore = PlayerPrefs.GetInt("AR_FRUIT_HIGHSCORE", 0);
        }

        private void Start()
        {
            // If ARFaceScanManager exists, wait for face scan; otherwise start game
            if (FindObjectOfType<AR.ARFaceScanManager>() == null)
            {
                StartGame();
            }
            else
            {
                currentState = GameState.FaceScanning;
                OnStateChanged?.Invoke(currentState);
            }
        }

        private void Update()
        {
            // Reset combo if time threshold exceeded
            if (currentComboStreak > 0 && Time.time - lastSliceTime > comboResetTime)
            {
                FinishCombo();
            }
        }

        public void StartGame()
        {
            currentScore = 0;
            currentLives = maxLives;
            currentComboStreak = 0;
            currentState = GameState.Playing;

            OnScoreChanged?.Invoke(currentScore);
            OnLivesChanged?.Invoke(currentLives);
            OnStateChanged?.Invoke(currentState);
        }

        public void RegisterFruitSlice(Vector3 worldPos, int basePoints = 10)
        {
            if (currentState != GameState.Playing) return;

            float now = Time.time;
            if (now - lastSliceTime <= comboResetTime)
            {
                currentComboStreak++;
            }
            else
            {
                FinishCombo();
                currentComboStreak = 1;
            }
            lastSliceTime = now;

            int earned = basePoints;
            currentScore += earned;

            // Trigger score update
            OnScoreChanged?.Invoke(currentScore);

            // Check high score
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("AR_FRUIT_HIGHSCORE", highScore);
            }

            // Mobile vibration feedback
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        private void FinishCombo()
        {
            if (currentComboStreak >= 2)
            {
                int bonus = currentComboStreak * 15;
                currentScore += bonus;
                OnScoreChanged?.Invoke(currentScore);
                OnComboTriggered?.Invoke(currentComboStreak, bonus);
            }
            currentComboStreak = 0;
        }

        public void RegisterMissedFruit()
        {
            if (currentState != GameState.Playing) return;

            currentLives--;
            OnLivesChanged?.Invoke(currentLives);

            if (currentLives <= 0)
            {
                TriggerGameOver("Out of Lives!");
            }
        }

        public void RegisterBombHit()
        {
            if (currentState != GameState.Playing) return;

            currentLives = 0;
            OnLivesChanged?.Invoke(currentLives);
            TriggerGameOver("BOOM! You sliced a Bomb!");
        }

        public void TriggerGameOver(string reason)
        {
            currentState = GameState.GameOver;
            OnStateChanged?.Invoke(currentState);
            Debug.Log($"[GameManager] Game Over! Reason: {reason}. Final Score: {currentScore}");
        }

        public void RestartGame()
        {
            StartGame();
        }
    }
}
