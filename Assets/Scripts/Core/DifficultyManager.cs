using System;
using UnityEngine;

namespace ARFruitSlasher.Core
{
    [Serializable]
    public struct DifficultyTier
    {
        public int minScore;
        public float arcDegrees;
        public float spawnInterval;
        public float fruitSpeedMultiplier;
        public float bombChance;
        public string tierName;
    }

    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        [Header("Tiers Configuration")]
        [SerializeField]
        private DifficultyTier[] tiers = new DifficultyTier[]
        {
            new DifficultyTier { minScore = 0,   arcDegrees = 100f, spawnInterval = 2.0f, fruitSpeedMultiplier = 1.0f, bombChance = 0.00f, tierName = "Level 1: Front Arc (100°)" },
            new DifficultyTier { minScore = 100, arcDegrees = 180f, spawnInterval = 1.6f, fruitSpeedMultiplier = 1.15f, bombChance = 0.10f, tierName = "Level 2: Wide Arc (180°)" },
            new DifficultyTier { minScore = 200, arcDegrees = 270f, spawnInterval = 1.25f, fruitSpeedMultiplier = 1.3f, bombChance = 0.15f, tierName = "Level 3: Deep Arc (270°)" },
            new DifficultyTier { minScore = 300, arcDegrees = 360f, spawnInterval = 0.95f, fruitSpeedMultiplier = 1.45f, bombChance = 0.22f, tierName = "Level 4: Full 360° Surround!" }
        };

        private int currentTierIndex = 0;

        public event Action<DifficultyTier> OnDifficultyChanged;

        public DifficultyTier CurrentTier => tiers[currentTierIndex];
        public float CurrentArcDegrees => tiers[currentTierIndex].arcDegrees;
        public float CurrentSpawnInterval => tiers[currentTierIndex].spawnInterval;
        public float CurrentSpeedMultiplier => tiers[currentTierIndex].fruitSpeedMultiplier;
        public float CurrentBombChance => tiers[currentTierIndex].bombChance;

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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += HandleScoreChanged;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                currentTierIndex = 0;
                OnDifficultyChanged?.Invoke(CurrentTier);
            }
        }

        private void HandleScoreChanged(int score)
        {
            int newTierIndex = 0;
            for (int i = tiers.Length - 1; i >= 0; i--)
            {
                if (score >= tiers[i].minScore)
                {
                    newTierIndex = i;
                    break;
                }
            }

            if (newTierIndex != currentTierIndex)
            {
                currentTierIndex = newTierIndex;
                Debug.Log($"[DifficultyManager] Checkpoint reached! New tier: {CurrentTier.tierName}");
                OnDifficultyChanged?.Invoke(CurrentTier);
            }
        }

        /// <summary>
        /// Generates a random spawn angle relative to the forward direction based on current arc
        /// </summary>
        public float GetRandomSpawnAngleDegrees()
        {
            float halfArc = CurrentArcDegrees * 0.5f;
            return UnityEngine.Random.Range(-halfArc, halfArc);
        }

        /// <summary>
        /// Returns whether a spawn angle falls outside standard camera view (> 60 degrees from forward),
        /// meaning it's to the side or behind the player.
        /// </summary>
        public bool IsOffscreenOrBehind(float angleDegrees)
        {
            return Mathf.Abs(angleDegrees) > 55f;
        }
    }
}
