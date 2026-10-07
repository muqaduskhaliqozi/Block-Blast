using System.Collections;
using UnityEngine;
using TMPro;
using BlockBlast.Scoring;

namespace BlockBlast.UI
{
    public class ComboUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI comboText;

        private Coroutine popRoutine;

        private void OnEnable()
        {
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.OnComboTriggered += ShowComboMessage;
            }
            if (comboText != null) comboText.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.OnComboTriggered -= ShowComboMessage;
            }
        }

        public void ShowComboMessage(int streak, string message)
        {
            if (comboText == null) return;

            if (popRoutine != null) StopCoroutine(popRoutine);
            popRoutine = StartCoroutine(AnimateComboText(message));
        }

        private IEnumerator AnimateComboText(string message)
        {
            comboText.text = message;
            comboText.gameObject.SetActive(true);

            Transform t = comboText.transform;
            t.localScale = Vector3.zero;

            float duration = 0.4f;
            float elapsed = 0f;

            // Scale up with overshoot
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float scale = Mathf.Sin(progress * Mathf.PI * 0.75f) * 1.3f;
                t.localScale = Vector3.one * scale;
                yield return null;
            }

            yield return new WaitForSeconds(0.4f);

            // Scale down
            elapsed = 0f;
            duration = 0.2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                t.localScale = Vector3.one * (1.3f * (1f - progress));
                yield return null;
            }

            comboText.gameObject.SetActive(false);
        }
    }
}
