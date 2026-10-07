using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

        [Header("Polish")]
        [Tooltip("Seconds to wait so the player sees the board grey out before the popup")]
        [SerializeField] private float showDelay = 1.0f;
        [SerializeField] private GameObject newBestBadge;

        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private int bestBeforeRun;
        private Coroutine showRoutine;

        private void Start()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
            if (homeButton != null) homeButton.onClick.AddListener(OnHomeClicked);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            }

            RememberBest();
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void RememberBest()
        {
            bestBeforeRun = PlayerPrefs.GetInt("BlockBlast_BestScore", 0);
        }

        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.GameOver)
            {
                if (showRoutine != null) StopCoroutine(showRoutine);
                showRoutine = StartCoroutine(ShowAfterDelay());

                if (Audio.AudioManager.Instance != null)
                {
                    Audio.AudioManager.Instance.PlayGameOverSound();
                }
            }
            else if (newState == GameState.Playing || newState == GameState.Restarting)
            {
                HideGameOverPanel();
            }
        }

        private IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSecondsRealtime(showDelay);
            showRoutine = null;
            if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentState == GameState.GameOver)
                ShowGameOverPanel();
        }

        public void ShowGameOverPanel()
        {
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            int best = ScoreManager.Instance != null ? ScoreManager.Instance.BestScore : score;
            bool isNewBest = score > 0 && score > bestBeforeRun;

            if (newBestBadge != null) newBestBadge.SetActive(isNewBest);
            if (bestScoreText != null) bestScoreText.text = best.ToString();
            if (finalScoreText != null) finalScoreText.text = "0";

            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            if (finalScoreText != null) StartCoroutine(CountUp(finalScoreText, score));
        }

        private IEnumerator CountUp(TextMeshProUGUI text, int target)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            float dur = Mathf.Clamp(0.4f + target * 0.002f, 0.4f, 1.2f);
            yield return Tween.Run(dur, k =>
            {
                text.text = Mathf.RoundToInt(target * Ease.OutCubic(k)).ToString();
                text.transform.localScale = Vector3.one * (k < 1f ? 1.08f : 1f);
            });
            yield return Tween.Run(0.3f, k => text.transform.localScale = Vector3.one * Mathf.LerpUnclamped(1.3f, 1f, Ease.OutBack(k, 3f)));
        }

        public void HideGameOverPanel()
        {
            if (showRoutine != null) { StopCoroutine(showRoutine); showRoutine = null; }
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            RememberBest();
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

            ScreenFader.LoadScene(mainMenuSceneName);
        }
    }
}
