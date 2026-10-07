using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockBlast.Core;
using BlockBlast.Game;
using BlockBlast.Scoring;

namespace BlockBlast.UI
{
    public class LevelCompleteUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button homeButton;

        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Start()
        {
            if (panel != null) panel.SetActive(false);

            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
            if (homeButton != null) homeButton.onClick.AddListener(OnHomeClicked);

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.LevelComplete) Show();
            else if (newState == GameState.Playing || newState == GameState.Restarting) Hide();
        }

        private void Show()
        {
            LevelData lvl = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;
            if (titleText != null) titleText.text = lvl != null ? "LEVEL " + lvl.levelNumber + " CLEAR!" : "LEVEL CLEAR!";

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            if (scoreText != null) scoreText.text = "0";

            if (panel != null) panel.SetActive(true);
            if (scoreText != null) StartCoroutine(CountUp(scoreText, score));
        }

        private IEnumerator CountUp(TextMeshProUGUI text, int target)
        {
            yield return new WaitForSecondsRealtime(0.35f);
            yield return Tween.Run(0.7f, k => text.text = Mathf.RoundToInt(target * Ease.OutCubic(k)).ToString());
        }

        private void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void OnNextClicked()
        {
            if (Audio.AudioManager.Instance != null) Audio.AudioManager.Instance.PlayButtonClickSound();
            if (LevelManager.Instance != null) LevelManager.Instance.NextLevel();
        }

        private void OnHomeClicked()
        {
            if (Audio.AudioManager.Instance != null) Audio.AudioManager.Instance.PlayButtonClickSound();
            // Progress CompleteLevel() mein pehle hi save ho chuka hai, to Home par jane se level nahi khoye ga
            ScreenFader.LoadScene(mainMenuSceneName);
        }
    }
}
