using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using BlockBlast.Core;
using BlockBlast.Scoring;

namespace BlockBlast.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button homeButton;

        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Start()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
            if (homeButton != null) homeButton.onClick.AddListener(OnHomeClicked);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.GameOver)
            {
                ShowGameOverPanel();
            }
            else if (newState == GameState.Playing || newState == GameState.Restarting)
            {
                HideGameOverPanel();
            }
        }

        public void ShowGameOverPanel()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            if (ScoreManager.Instance != null)
            {
                if (finalScoreText != null) finalScoreText.text = ScoreManager.Instance.CurrentScore.ToString();
                if (bestScoreText != null) bestScoreText.text = $"BEST: {ScoreManager.Instance.BestScore}";
            }

            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlayGameOverSound();
            }
        }

        public void HideGameOverPanel()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        private void OnRetryClicked()
        {
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlayButtonClickSound();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        private void OnHomeClicked()
        {
            if (Audio.AudioManager.Instance != null)
            {
                Audio.AudioManager.Instance.PlayButtonClickSound();
            }

            Debug.Log("[GameOverUI] Home button clicked.");

            if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                Debug.LogError("[GameOverUI] Scene '" + mainMenuSceneName +
                               "' load nahi ho sakta. Naam sahi hai? Build Settings mein add hai?");
                return;
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}