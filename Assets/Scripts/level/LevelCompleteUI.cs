using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
            if (panel != null) panel.SetActive(true);

            LevelData lvl = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;
            if (titleText != null && lvl != null) titleText.text = "LEVEL " + lvl.levelNumber + " COMPLETE!";
            if (scoreText != null && ScoreManager.Instance != null) scoreText.text = ScoreManager.Instance.CurrentScore.ToString();
        }

        private void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void OnNextClicked()
        {
            if (LevelManager.Instance != null) LevelManager.Instance.NextLevel();
        }

        private void OnHomeClicked()
        {
            Debug.Log("[LevelCompleteUI] Home clicked.");

            if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                Debug.LogError("[LevelCompleteUI] Scene '" + mainMenuSceneName +
                               "' load nahi ho sakta. Naam sahi hai? Build Settings mein add hai?");
                return;
            }

            // Progress CompleteLevel() mein pehle hi save ho chuka hai, to Home par jane se level nahi khoye ga
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}