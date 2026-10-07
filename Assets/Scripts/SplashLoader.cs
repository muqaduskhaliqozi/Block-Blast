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

        [Header("Settings")]
        [SerializeField] private float loadDuration = 6f;
        [SerializeField] private string nextSceneName = "MainMenu";

        private IEnumerator Start()
        {
            SetProgress(0f);

            // Agle scene ko background mein load karna shuru karein, lekin abhi on na karein
            AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);

            if (op == null)
            {
                Debug.LogError("[SplashLoader] Scene '" + nextSceneName +
                               "' load nahi ho saka. Naam sahi hai? Build Settings mein add hai?");
                yield break;
            }

            op.allowSceneActivation = false;

            // 6 second mein bar bharein
            float t = 0f;
            while (t < loadDuration)
            {
                t += Time.deltaTime;
                SetProgress(Mathf.Clamp01(t / loadDuration));
                yield return null;
            }

            SetProgress(1f);

            // Scene tayyar hone ka intezar (Unity 0.9 par ruk kar activation ka intezar karta hai)
            while (op.progress < 0.9f)
            {
                yield return null;
            }

            op.allowSceneActivation = true;
        }

        private void SetProgress(float value)
        {
            if (loadingSlider != null) loadingSlider.value = value;
            if (fillImage != null) fillImage.fillAmount = value;
            if (percentText != null) percentText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }
}