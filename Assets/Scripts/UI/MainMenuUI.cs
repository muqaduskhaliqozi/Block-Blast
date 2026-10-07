using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlockBlast.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;   // optional, khali chhor sakte hain

        [Header("Info (optional)")]
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Sound Toggle (optional)")]
        [SerializeField] private Button soundButton;
        [SerializeField] private Image soundIcon;
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite soundOffSprite;

        [SerializeField] private string gameSceneName = "BlockBlastGameplay";

        private const string MuteKey = "BlockBlast_Muted";
        private AudioSource clickSource;
        private AudioClip clickClip;

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
            if (soundButton != null) soundButton.onClick.AddListener(OnSoundClicked);

            if (bestScoreText != null) bestScoreText.text = PlayerPrefs.GetInt("BlockBlast_BestScore", 0).ToString();
            if (levelText != null) levelText.text = "LEVEL " + (PlayerPrefs.GetInt("CurrentLevel", 0) + 1);

            clickSource = gameObject.AddComponent<AudioSource>();
            clickClip = Audio.AudioSynth.CreateButtonClickClip();
            RefreshSound();
        }

        private void Click()
        {
            if (PlayerPrefs.GetInt(MuteKey, 0) == 1 || clickSource == null || clickClip == null) return;
            clickSource.PlayOneShot(clickClip);
        }

        private void OnPlayClicked()
        {
            Click();
            ScreenFader.LoadScene(gameSceneName);
        }

        private void OnSoundClicked()
        {
            bool muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            PlayerPrefs.SetInt(MuteKey, muted ? 0 : 1);
            PlayerPrefs.Save();
            RefreshSound();
            Click();
        }

        private void RefreshSound()
        {
            if (soundIcon == null) return;
            bool muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            if (soundOnSprite != null && soundOffSprite != null) soundIcon.sprite = muted ? soundOffSprite : soundOnSprite;
        }

        private void OnQuitClicked()
        {
            Click();
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
