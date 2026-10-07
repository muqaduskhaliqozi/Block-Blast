using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BlockBlast.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;   // optional, khali chhor sakte hain

        [SerializeField] private string gameSceneName = "BlockBlastGameplay";

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
            {
                Debug.LogError("[MainMenuUI] Scene '" + gameSceneName +
                               "' load nahi ho sakta. Naam sahi hai? Build Settings mein add hai?");
                return;
            }

            SceneManager.LoadScene(gameSceneName);
        }

        private void OnQuitClicked()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}