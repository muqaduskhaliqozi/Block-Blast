using System.Collections;
using UnityEngine;
using TMPro;
using BlockBlast.Core;
using BlockBlast.Game;

namespace BlockBlast.UI
{
    /// <summary>
    /// "LEVEL 3" banner that sweeps in across the board at the start of a level, holds, then sweeps out.
    /// Never blocks touches.
    /// </summary>
    public class LevelIntroUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;   // banner root (needs a RectTransform)
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private float showSeconds = 1.8f;
        [SerializeField] private float fadeSeconds = 0.4f;

        private CanvasGroup group;
        private RectTransform bannerRect;
        private Coroutine routine;

        private void Start()
        {
            if (panel != null)
            {
                panel.SetActive(true);
                group = panel.GetComponent<CanvasGroup>();
                if (group == null) group = panel.AddComponent<CanvasGroup>();
                group.blocksRaycasts = false;
                group.interactable = false;
                group.alpha = 0f;
                bannerRect = panel.transform as RectTransform;
            }

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
            if (descriptionText != null)
            {
                string desc = lvl.description;
                if (string.IsNullOrEmpty(desc))
                {
                    if (lvl.requiredLinesCleared > 0) desc = "Clear " + lvl.requiredLinesCleared + " lines to win!";
                    else if (lvl.targetScore > 0) desc = "Score " + lvl.targetScore + " points to win!";
                }
                descriptionText.text = desc;
            }

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(IntroRoutine());
        }

        private IEnumerator IntroRoutine()
        {
            float width = 1400f;
            RectTransform canvasRect = panel.GetComponentInParent<Canvas>()?.rootCanvas.transform as RectTransform;
            if (canvasRect != null) width = canvasRect.rect.width + 200f;

            yield return new WaitForSecondsRealtime(0.25f);

            // Sweep in from the left
            yield return Tween.Run(0.45f, k =>
            {
                float e = Ease.OutBack(k, 1.4f);
                bannerRect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(-width, 0f, e), bannerRect.anchoredPosition.y);
                group.alpha = Mathf.Clamp01(k * 3f);
                if (titleText != null) titleText.transform.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, Ease.OutBack(k, 3f));
            });

            yield return new WaitForSecondsRealtime(showSeconds);

            // Sweep out to the right
            yield return Tween.Run(fadeSeconds, k =>
            {
                float e = Ease.InBack(k, 1.2f);
                bannerRect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(0f, width, e), bannerRect.anchoredPosition.y);
                group.alpha = 1f - Mathf.Clamp01((k - 0.5f) * 2f);
            });

            group.alpha = 0f;
            routine = null;
        }
    }
}
