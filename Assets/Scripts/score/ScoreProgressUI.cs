using UnityEngine;
using TMPro;
using BlockBlast.Game;
using BlockBlast.Scoring;

namespace BlockBlast.UI
{
    public class ScoreProgressUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private string linesPrefix = "LINES  ";
        [SerializeField] private string scorePrefix = "SCORE  ";

        private string lastShown = null;

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

            if (requiredLines > 0)
            {
                // Sirf poori hui rows/columns ginti hain (0/8 se shuru, 8/8 par khatam)
                int done = Mathf.Min(lm.LinesCleared, requiredLines);
                shown = linesPrefix + done + "/" + requiredLines;
            }
            else if (targetScore > 0 && ScoreManager.Instance != null)
            {
                // Lines ki shart nahi hai to score dikhayein
                int current = Mathf.Min(ScoreManager.Instance.CurrentScore, targetScore);
                shown = scorePrefix + current + "/" + targetScore;
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
        }
    }
}