using UnityEngine;
using UnityEngine.UI;
using BlockBlast.Core;

namespace BlockBlast.UI
{
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button homeButton;

        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            else Debug.LogError("[PauseUI] Pause Panel field khali hai!");

            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
            else Debug.LogError("[PauseUI] Pause Button field khali hai!");

            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            else Debug.LogError("[PauseUI] Resume Button field khali hai!");

            if (homeButton != null) homeButton.onClick.AddListener(OnHomeClicked);
            else Debug.LogWarning("[PauseUI] Home Button field khali hai.");

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        // Pause ke ilawa koi bhi state aaye to panel chhup jaye
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState != GameState.Paused && pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnPauseClicked()
        {
            GameStateManager sm = GameStateManager.Instance;
            if (sm == null) return;

            // Sirf tab pause hone dein jab game normal chal raha ho
            if (sm.CurrentState != GameState.Playing) return;

            PlayClick();
            sm.ChangeState(GameState.Paused);

            if (pausePanel != null) pausePanel.SetActive(true);
        }

        private void OnResumeClicked()
        {
            PlayClick();

            if (pausePanel != null) pausePanel.SetActive(false);

            GameStateManager sm = GameStateManager.Instance;
            if (sm != null && sm.CurrentState == GameState.Paused)
                sm.ChangeState(GameState.Playing);
        }

        private void OnHomeClicked()
        {
            PlayClick();
            ScreenFader.LoadScene(mainMenuSceneName);
        }

        private void PlayClick()
        {
            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.PlayButtonClickSound();
        }
    }
}
