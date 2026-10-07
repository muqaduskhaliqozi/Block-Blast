using UnityEngine;

namespace BlockBlast.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource sfxSource;

        private AudioClip pickupClip;
        private AudioClip placeClip;
        private AudioClip lineClearClip;
        private AudioClip comboClip;
        private AudioClip gameOverClip;
        private AudioClip buttonClickClip;

        public bool IsMuted { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            GenerateProceduralAudio();
        }

        private void GenerateProceduralAudio()
        {
            pickupClip = AudioSynth.CreatePickupClip();
            placeClip = AudioSynth.CreatePlaceClip();
            lineClearClip = AudioSynth.CreateLineClearClip();
            comboClip = AudioSynth.CreateComboClip();
            gameOverClip = AudioSynth.CreateGameOverClip();
            buttonClickClip = AudioSynth.CreateButtonClickClip();
        }

        public void PlayPickupSound() => PlayOneShot(pickupClip);
        public void PlayPlaceSound() => PlayOneShot(placeClip);
        public void PlayLineClearSound(int lineCount = 1) => PlayOneShot(lineClearClip);
        public void PlayComboSound(int streak = 2) => PlayOneShot(comboClip);
        public void PlayGameOverSound() => PlayOneShot(gameOverClip);
        public void PlayButtonClickSound() => PlayOneShot(buttonClickClip);

        private void PlayOneShot(AudioClip clip)
        {
            if (IsMuted || clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            sfxSource.mute = IsMuted;
        }
    }
}
