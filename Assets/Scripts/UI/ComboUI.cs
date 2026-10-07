using System.Collections;
using UnityEngine;
using TMPro;
using BlockBlast.Scoring;

namespace BlockBlast.UI
{
    public class ComboUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private RectTransform glow;   // optional burst behind the text

        private Coroutine popRoutine;
        private bool subscribed;

        // Top / bottom gradient colours, chosen by how big the combo is
        private static readonly Color[][] Gradients =
        {
            new[] { new Color(0.75f, 1f, 1f), new Color(0.1f, 0.75f, 1f) },     // GOOD
            new[] { new Color(0.8f, 1f, 0.8f), new Color(0.1f, 0.85f, 0.45f) }, // GREAT / DOUBLE
            new[] { new Color(1f, 0.97f, 0.6f), new Color(1f, 0.55f, 0.05f) },  // COMBO x3+
            new[] { new Color(1f, 0.8f, 1f), new Color(1f, 0.1f, 0.55f) },      // UNSTOPPABLE
        };

        private void OnEnable()
        {
            TrySubscribe();
            if (comboText != null) comboText.gameObject.SetActive(false);
            if (glow != null) glow.gameObject.SetActive(false);
        }

        private void Start() => TrySubscribe();

        private void TrySubscribe()
        {
            if (subscribed || ComboManager.Instance == null) return;
            ComboManager.Instance.OnComboTriggered += ShowComboMessage;
            subscribed = true;
        }

        private void OnDisable()
        {
            if (subscribed && ComboManager.Instance != null)
                ComboManager.Instance.OnComboTriggered -= ShowComboMessage;
            subscribed = false;
        }

        public void ShowComboMessage(int streak, string message)
        {
            if (comboText == null) return;

            int tier = 0;
            if (message.StartsWith("UNSTOPPABLE")) tier = 3;
            else if (message.StartsWith("COMBO")) tier = 2;
            else if (message.StartsWith("GREAT") || message.StartsWith("DOUBLE")) tier = 1;

            if (popRoutine != null) StopCoroutine(popRoutine);
            popRoutine = StartCoroutine(AnimateComboText(message, tier));
        }

        private IEnumerator AnimateComboText(string message, int tier)
        {
            comboText.text = message;
            comboText.enableVertexGradient = true;
            Color[] g = Gradients[Mathf.Clamp(tier, 0, Gradients.Length - 1)];
            comboText.colorGradient = new VertexGradient(g[0], g[0], g[1], g[1]);
            comboText.alpha = 1f;
            comboText.gameObject.SetActive(true);

            Transform t = comboText.transform;
            RectTransform rt = (RectTransform)t;
            Vector2 basePos = Vector2.zero;
            rt.anchoredPosition = basePos;
            float tilt = Random.Range(-6f, 6f);

            if (glow != null)
            {
                glow.gameObject.SetActive(true);
                StartCoroutine(Tween.Run(0.6f, k =>
                {
                    glow.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.6f + tier * 0.2f, Ease.OutCubic(k));
                    var img = glow.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) { Color c = img.color; c.a = (1f - k) * 0.8f; img.color = c; }
                    if (k >= 1f) glow.gameObject.SetActive(false);
                }));
            }

            // Pop in with an elastic overshoot and a little tilt
            float targetScale = 1f + tier * 0.12f;
            yield return Tween.Run(0.45f, k =>
            {
                t.localScale = Vector3.one * targetScale * Ease.OutElastic(k);
                t.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(tilt * 3f, tilt, Ease.OutCubic(k)));
            });

            // Hold with a gentle wiggle
            float hold = 0.55f + tier * 0.1f;
            float e = 0f;
            while (e < hold)
            {
                e += Time.unscaledDeltaTime;
                t.localScale = Vector3.one * targetScale * (1f + 0.04f * Mathf.Sin(e * 18f));
                yield return null;
            }

            // Float up and fade out
            yield return Tween.Run(0.3f, k =>
            {
                rt.anchoredPosition = basePos + new Vector2(0f, 120f * Ease.OutCubic(k));
                comboText.alpha = 1f - k;
                t.localScale = Vector3.one * targetScale * Mathf.Lerp(1f, 0.85f, k);
            });

            comboText.gameObject.SetActive(false);
            rt.anchoredPosition = basePos;
            popRoutine = null;
        }
    }
}
