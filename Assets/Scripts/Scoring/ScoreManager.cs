using System;
using UnityEngine;
using BlockBlast.Core;
using BlockBlast.Save;

namespace BlockBlast.Scoring
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int CurrentScore { get; private set; } = 0;
        public int BestScore { get; private set; } = 0;

        [SerializeField] private GameConfig config;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnBestScoreChanged;

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
            if (config == null) config = GameConfig.Default;
            LoadBestScore();
        }

        public void LoadBestScore()
        {
            if (SaveManager.Instance != null)
            {
                BestScore = SaveManager.Instance.GetBestScore();
            }
            else
            {
                BestScore = PlayerPrefs.GetInt("BlockBlast_BestScore", 0);
            }
            OnBestScoreChanged?.Invoke(BestScore);
        }

        public void AddPlacementScore(int cellCount)
        {
            int points = cellCount * config.scorePerCell;
            AddScore(points);
        }

        public void AddLineClearScore(int lineCount)
        {
            if (lineCount <= 0) return;

            int basePoints = config.baseLineClearScore;
            int points = 0;

            switch (lineCount)
            {
                case 1: points = basePoints; break;
                case 2: points = basePoints * 2 + 5; break;  // 25
                case 3: points = basePoints * 3 + 15; break; // 45
                case 4: points = basePoints * 4 + 30; break; // 70
                default: points = basePoints * lineCount * 2; break;
            }

            // Multiply by combo count if combo active
            if (ComboManager.Instance != null && ComboManager.Instance.CurrentStreak > 1)
            {
                points += ComboManager.Instance.CurrentStreak * config.comboBonusMultiplier;
            }

            AddScore(points);
        }

        private void AddScore(int points)
        {
            CurrentScore += points;
            OnScoreChanged?.Invoke(CurrentScore);

            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                OnBestScoreChanged?.Invoke(BestScore);

                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveBestScore(BestScore);
                }
                else
                {
                    PlayerPrefs.SetInt("BlockBlast_BestScore", BestScore);
                    PlayerPrefs.Save();
                }
            }
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
