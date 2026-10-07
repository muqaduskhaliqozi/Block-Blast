using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    /// <summary>
    /// Smooth fade transition between scenes. Creates itself on first use (no scene setup needed).
    /// Usage: ScreenFader.LoadScene("MainMenu");
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        private static ScreenFader instance;

        private CanvasGroup group;
        private bool busy;

        public static readonly Color FadeColor = new Color(0.04f, 0.06f, 0.18f, 1f);

        public static ScreenFader Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("[ScreenFader]");
                    instance = go.AddComponent<ScreenFader>();
                    DontDestroyOnLoad(go);
                    instance.Build();
                }
                return instance;
            }
        }

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            gameObject.AddComponent<GraphicRaycaster>();

            GameObject img = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            img.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)img.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            img.GetComponent<Image>().color = FadeColor;

            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public static void LoadScene(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError("[ScreenFader] Scene '" + sceneName + "' is not in Build Settings.");
                return;
            }
            Instance.FadeOut(() => SceneManager.LoadScene(sceneName));
        }

        /// <summary>Fades to the fade color and then runs the action (e.g. scene activation).</summary>
        public void FadeOut(Action then, float duration = 0.3f)
        {
            if (busy) return;
            StartCoroutine(FadeOutRoutine(then, duration));
        }

        private IEnumerator FadeOutRoutine(Action then, float duration)
        {
            busy = true;
            group.blocksRaycasts = true;
            float start = group.alpha;
            yield return Tween.Run(duration, t => group.alpha = Mathf.Lerp(start, 1f, Ease.InOutSine(t)));
            then?.Invoke();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (group == null || group.alpha <= 0f) { busy = false; return; }
            StopAllCoroutines();
            StartCoroutine(FadeInRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            yield return null; // let the new scene render one frame first
            yield return Tween.Run(0.35f, t => group.alpha = 1f - Ease.InOutSine(t));
            group.blocksRaycasts = false;
            busy = false;
        }
    }
}
