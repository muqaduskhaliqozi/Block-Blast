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

        private void Start()
        {
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (soundButton != null) soundButton.onClick.AddListener(OnSoundToggleClicked);
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
                if (soundIconImage != null)
                {
                    soundIconImage.color = AudioManager.Instance.IsMuted ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
                }
            }
        }
    }
}
