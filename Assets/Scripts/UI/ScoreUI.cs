using UnityEngine;
using TMPro;
using BlockBlast.Scoring;

namespace BlockBlast.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;

        private void OnEnable()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScore;
                ScoreManager.Instance.OnBestScoreChanged += UpdateBestScore;
                UpdateScore(ScoreManager.Instance.CurrentScore);
                UpdateBestScore(ScoreManager.Instance.BestScore);
            }
        }

        private void OnDisable()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
                ScoreManager.Instance.OnBestScoreChanged -= UpdateBestScore;
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }

        private void UpdateBestScore(int bestScore)
        {
            if (bestScoreText != null)
            {
                bestScoreText.text = $"👑 BEST: {bestScore}";
            }
        }
    }
}
