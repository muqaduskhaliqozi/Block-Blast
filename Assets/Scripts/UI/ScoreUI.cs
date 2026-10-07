using System.Collections;
using UnityEngine;
using TMPro;
using BlockBlast.Scoring;

namespace BlockBlast.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;

        [Header("Animation")]
        [SerializeField] private float countDuration = 0.45f;
        [SerializeField] private float punchScale = 1.25f;
        [SerializeField] private RectTransform bestBadge;   // optional: pulses when a new best is set

        private int shownScore;
        private int targetScore;
        private Coroutine countRoutine;
        private Coroutine bestRoutine;
        private bool subscribed;

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            // ScoreManager may wake up after this component, so try again here
            TrySubscribe();
        }

        private void TrySubscribe()
        {
            if (subscribed || ScoreManager.Instance == null) return;
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
            ScoreManager.Instance.OnBestScoreChanged += UpdateBestScore;
            subscribed = true;

            shownScore = targetScore = ScoreManager.Instance.CurrentScore;
            SetScoreText(shownScore);
            SetBestText(ScoreManager.Instance.BestScore);
        }

        private void OnDisable()
        {
            if (subscribed && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
                ScoreManager.Instance.OnBestScoreChanged -= UpdateBestScore;
            }
            subscribed = false;
        }

        private void UpdateScore(int score)
        {
            targetScore = score;

            if (!isActiveAndEnabled || score <= shownScore)
            {
                // Reset / restart: snap straight to the value
                if (countRoutine != null) StopCoroutine(countRoutine);
                countRoutine = null;
                shownScore = score;
                SetScoreText(score);
                if (scoreText != null) scoreText.transform.localScale = Vector3.one;
                return;
            }

            if (countRoutine != null) StopCoroutine(countRoutine);
            countRoutine = StartCoroutine(CountUp(shownScore, score));
        }

        private IEnumerator CountUp(int from, int to)
        {
            float t = 0f;
            Transform tr = scoreText != null ? scoreText.transform : null;
            while (t < countDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / countDuration);
                shownScore = Mathf.RoundToInt(Mathf.Lerp(from, to, Ease.OutCubic(k)));
                SetScoreText(shownScore);
                if (tr != null)
                {
                    // quick punch then settle
                    float p = k < 0.3f ? Mathf.Lerp(1f, punchScale, k / 0.3f) : Mathf.Lerp(punchScale, 1f, Ease.OutBack((k - 0.3f) / 0.7f, 2f));
                    tr.localScale = Vector3.one * p;
                }
                yield return null;
            }
            shownScore = to;
            SetScoreText(to);
            if (tr != null) tr.localScale = Vector3.one;
            countRoutine = null;
        }

        private void UpdateBestScore(int bestScore)
        {
            SetBestText(bestScore);
            if (bestBadge != null && isActiveAndEnabled && shownScore > 0)
            {
                if (bestRoutine != null) StopCoroutine(bestRoutine);
                bestRoutine = StartCoroutine(Tween.Run(0.35f, k =>
                {
                    bestBadge.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(k * Mathf.PI));
                }));
            }
        }

        private void SetScoreText(int value)
        {
            if (scoreText != null) scoreText.text = value.ToString();
        }

        private void SetBestText(int value)
        {
            if (bestScoreText != null) bestScoreText.text = value.ToString();
        }
    }
}
