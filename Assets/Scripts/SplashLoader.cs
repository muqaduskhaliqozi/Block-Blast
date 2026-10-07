using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace BlockBlast.UI
{
    public class SplashLoader : MonoBehaviour
    {
        [Header("Loading Bar (Slider ya Image mein se ek)")]
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private Image fillImage;          // Image Type = Filled ho to

        [Header("Optional")]
        [SerializeField] private TextMeshProUGUI percentText;   // jaise "45%"
        [SerializeField] private TextMeshProUGUI statusText;    // "LOADING..." with animated dots
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private string[] tips =
        {
            "Clear rows AND columns at once for big combos!",
            "Plan ahead - keep space for the big 3x3 blocks.",
            "Chain clears back to back to keep your combo alive.",
            "Fill the bar to complete each level!"
        };

        [Header("Settings")]
        [SerializeField] private float loadDuration = 6f;
        [SerializeField] private string nextSceneName = "MainMenu";

        private IEnumerator Start()
        {
            SetProgress(0f);
            if (tipText != null && tips != null && tips.Length > 0) tipText.text = tips[Random.Range(0, tips.Length)];

            // Agle scene ko background mein load karna shuru karein, lekin abhi on na karein
            AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);

            if (op == null)
            {
                Debug.LogError("[SplashLoader] Scene '" + nextSceneName +
                               "' load nahi ho saka. Naam sahi hai? Build Settings mein add hai?");
                yield break;
            }

            op.allowSceneActivation = false;

            float t = 0f;
            while (t < loadDuration)
            {
                t += Time.deltaTime;
                // ease so the bar feels like real loading (fast start, gentle finish)
                SetProgress(Ease.InOutSine(Mathf.Clamp01(t / loadDuration)));
                AnimateStatus();
                yield return null;
            }

            SetProgress(1f);

            // Scene tayyar hone ka intezar (Unity 0.9 par ruk kar activation ka intezar karta hai)
            while (op.progress < 0.9f)
            {
                AnimateStatus();
                yield return null;
            }

            yield return new WaitForSeconds(0.15f);
            ScreenFader.Instance.FadeOut(() => op.allowSceneActivation = true, 0.35f);
        }

        private void AnimateStatus()
        {
            if (statusText == null) return;
            int dots = Mathf.FloorToInt(Time.time * 3f) % 4;
            statusText.text = "LOADING" + new string('.', dots);
        }

        private void SetProgress(float value)
        {
            if (loadingSlider != null) loadingSlider.value = value;
            if (fillImage != null) fillImage.fillAmount = value;
            if (percentText != null) percentText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }
}
