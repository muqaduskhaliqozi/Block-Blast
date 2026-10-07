using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockBlast.Game;
using BlockBlast.Scoring;

namespace BlockBlast.UI
{
    /// <summary>Shows the level goal ("LINES 3/20") and fills a progress bar towards it.</summary>
    public class ScoreProgressUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private string linesPrefix = "LINES  ";
        [SerializeField] private string scorePrefix = "SCORE  ";

        [Header("Progress Bar (optional)")]
        [SerializeField] private RectTransform barFill;    // stretched fill, anchorMax.x is driven 0..1
        [SerializeField] private TextMeshProUGUI levelText; // e.g. "LEVEL 3"

        private string lastShown = null;
        private float shownProgress;
        private int lastLevel = -1;

        private void Awake()
        {
            if (text == null) text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            LevelManager lm = LevelManager.Instance;
            if (lm == null || lm.CurrentLevel == null || text == null) return;

            int requiredLines = lm.CurrentLevel.requiredLinesCleared;
            int targetScore = lm.CurrentLevel.targetScore;

            string shown;
            float progress = 0f;

            if (requiredLines > 0)
            {
                // Sirf poori hui rows/columns ginti hain (0/8 se shuru, 8/8 par khatam)
                int done = Mathf.Min(lm.LinesCleared, requiredLines);
                shown = linesPrefix + done + "/" + requiredLines;
                progress = (float)done / requiredLines;
            }
            else if (targetScore > 0 && ScoreManager.Instance != null)
            {
                // Lines ki shart nahi hai to score dikhayein
                int current = Mathf.Min(ScoreManager.Instance.CurrentScore, targetScore);
                shown = scorePrefix + current + "/" + targetScore;
                progress = (float)current / targetScore;
            }
            else
            {
                shown = "";
            }

            if (shown != lastShown)
            {
                text.text = shown;
                lastShown = shown;
            }

            if (levelText != null && lm.CurrentLevel.levelNumber != lastLevel)
            {
                lastLevel = lm.CurrentLevel.levelNumber;
                levelText.text = "LEVEL " + lastLevel;
            }

            if (barFill != null)
            {
                // Smoothly chase the real value (snaps down instantly on restart)
                shownProgress = progress < shownProgress ? progress : Mathf.Lerp(shownProgress, progress, Time.unscaledDeltaTime * 6f);
                Vector2 max = barFill.anchorMax;
                max.x = Mathf.Max(0.001f, shownProgress);
                barFill.anchorMax = max;
                barFill.gameObject.SetActive(shownProgress > 0.01f);
            }
        }
    }
}
