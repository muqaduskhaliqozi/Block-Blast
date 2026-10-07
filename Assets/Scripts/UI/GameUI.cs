using UnityEngine;
using UnityEngine.UI;
using BlockBlast.Core;
using BlockBlast.Audio;

namespace BlockBlast.UI
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Image soundIconImage;

        [Header("Sound Icons (optional)")]
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite soundOffSprite;

        private void Start()
        {
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (soundButton != null) soundButton.onClick.AddListener(OnSoundToggleClicked);
            RefreshSoundIcon();
        }

        private void OnRestartClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClickSound();
            if (GameManager.Instance != null) GameManager.Instance.RestartGame();
        }

        private void OnSoundToggleClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMute();
                AudioManager.Instance.PlayButtonClickSound();
            }
            RefreshSoundIcon();
        }

        private void RefreshSoundIcon()
        {
            bool muted = AudioManager.Instance != null
                ? AudioManager.Instance.IsMuted
                : PlayerPrefs.GetInt(AudioManager.MuteKey, 0) == 1;

            if (soundIconImage == null) return;

            if (soundOnSprite != null && soundOffSprite != null)
            {
                soundIconImage.sprite = muted ? soundOffSprite : soundOnSprite;
                soundIconImage.color = Color.white;
            }
            else
            {
                soundIconImage.color = muted ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
            }
        }
    }
}
