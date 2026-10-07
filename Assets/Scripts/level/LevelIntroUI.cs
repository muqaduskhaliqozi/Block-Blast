using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockBlast.Core;
using BlockBlast.Game;

namespace BlockBlast.UI
{
    public class LevelIntroUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;   // ye kabhi band nahi hoga
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private float showSeconds = 5f;
        [SerializeField] private float fadeSeconds = 0.4f;

        private Image[] images;
        private float[] imageAlpha;
        private float titleAlpha = 1f;
        private float descAlpha = 1f;
        private Coroutine routine;

        private void Start()
        {
            if (panel != null)
            {
                // Panel hamesha on rahe (pehle band tha to bhi on kar dein)
                panel.SetActive(true);

                // Panel par touch/drag na rukay
                CanvasGroup group = panel.GetComponent<CanvasGroup>();
                if (group == null) group = panel.AddComponent<CanvasGroup>();
                group.blocksRaycasts = false;
                group.interactable = false;

                // Panel ki saari background images yaad rakhein
                images = panel.GetComponentsInChildren<Image>(true);
                imageAlpha = new float[images.Length];
                for (int i = 0; i < images.Length; i++) imageAlpha[i] = images[i].color.a;
            }

            if (titleText != null) titleAlpha = titleText.color.a;
            if (descriptionText != null) descAlpha = descriptionText.color.a;

            SetVisible(false);   // shuru mein intro chhupa rahe

            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;

            ShowIntro();   // game shuru hote hi
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        // Retry ya Next Level ke baad dobara dikhayein
        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            bool wasEnded = oldState == GameState.GameOver || oldState == GameState.LevelComplete;
            bool restarted = newState == GameState.Restarting || newState == GameState.Playing;

            if (wasEnded && restarted) ShowIntro();
        }

        public void ShowIntro()
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null || lm.CurrentLevel == null || panel == null) return;

            LevelData lvl = lm.CurrentLevel;

            if (titleText != null) titleText.text = "LEVEL " + lvl.levelNumber;
            if (descriptionText != null) descriptionText.text = lvl.description;

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(IntroRoutine());
        }

        private IEnumerator IntroRoutine()
        {
            SetVisible(true);
            SetAlpha(1f);

            yield return new WaitForSeconds(showSeconds);

            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(1f - Mathf.Clamp01(t / fadeSeconds));
                yield return null;
            }

            SetVisible(false);
            SetAlpha(1f);
            routine = null;
        }

        // Sirf intro ki cheezein on/off hoti hain, panel nahi
        private void SetVisible(bool visible)
        {
            if (images != null)
            {
                for (int i = 0; i < images.Length; i++)
                    if (images[i] != null) images[i].enabled = visible;
            }

            if (titleText != null) titleText.enabled = visible;
            if (descriptionText != null) descriptionText.enabled = visible;
        }

        private void SetAlpha(float k)
        {
            if (images != null)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] == null) continue;
                    Color c = images[i].color;
                    c.a = imageAlpha[i] * k;
                    images[i].color = c;
                }
            }

            if (titleText != null)
            {
                Color c = titleText.color;
                c.a = titleAlpha * k;
                titleText.color = c;
            }

            if (descriptionText != null)
            {
                Color c = descriptionText.color;
                c.a = descAlpha * k;
                descriptionText.color = c;
            }
        }
    }
}