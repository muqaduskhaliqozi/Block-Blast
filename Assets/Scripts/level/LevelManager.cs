using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Core;
using BlockBlast.Board;
using BlockBlast.Blocks;
using BlockBlast.Scoring;

namespace BlockBlast.Game
{
    // -100 is liye ke ye BoardManager se pehle Awake ho aur board sahi size mein bane
    [DefaultExecutionOrder(-100)]
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        [Header("Testing")]
        [Tooltip("On ho to har dafa gameplay scene shuru hone par Level 1 se shuru hoga. Final build se pehle OFF karein.")]
        [SerializeField] private bool resetProgressOnStart = false;

        private const string SaveKey = "CurrentLevel";

        private int linesClearedThisLevel;

        public int CurrentIndex { get; private set; }
        public bool HasNextLevel => CurrentIndex < levels.Count - 1;
        public int LinesCleared => linesClearedThisLevel;

        public LevelData CurrentLevel
        {
            get
            {
                if (levels == null || levels.Count == 0) return null;
                return levels[Mathf.Clamp(CurrentIndex, 0, levels.Count - 1)];
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (levels.Count == 0)
                Debug.LogError("[LevelManager] Levels ki list khali hai! Inspector mein LevelData assets daalein.");

            if (resetProgressOnStart)
            {
                PlayerPrefs.DeleteKey(SaveKey);
                Debug.Log("[LevelManager] Progress reset on start.");
            }

            CurrentIndex = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, 0), 0, Mathf.Max(0, levels.Count - 1));
        }

        private void Start()
        {
            if (BoardManager.Instance != null)
                BoardManager.Instance.OnLinesCleared += HandleLinesCleared;

            StartCoroutine(WatchRoutine());
        }

        private void OnDestroy()
        {
            if (BoardManager.Instance != null)
                BoardManager.Instance.OnLinesCleared -= HandleLinesCleared;
        }

        private void HandleLinesCleared(int lineCount)
        {
            linesClearedThisLevel += lineCount;
            Debug.Log("[LevelManager] Lines cleared this level: " + linesClearedThisLevel);
        }

        private IEnumerator WatchRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(0.2f);

            while (true)
            {
                yield return wait;

                GameStateManager sm = GameStateManager.Instance;
                ScoreManager score = ScoreManager.Instance;
                LevelData lvl = CurrentLevel;

                if (sm == null || score == null || lvl == null) continue;

                // Score 0 ka matlab naya game / retry hai, to line counter bhi zero
                if (score.CurrentScore == 0) linesClearedThisLevel = 0;

                // Sirf tab check karein jab clear animation khatam ho chuki ho
                if (sm.CurrentState != GameState.Playing) continue;

                bool scoreOk = lvl.targetScore <= 0 || score.CurrentScore >= lvl.targetScore;
                bool linesOk = lvl.requiredLinesCleared <= 0 || linesClearedThisLevel >= lvl.requiredLinesCleared;

                // Dono shartein na hon (0, 0) to level kabhi complete nahi hoga
                bool hasAnyGoal = lvl.targetScore > 0 || lvl.requiredLinesCleared > 0;

                if (hasAnyGoal && scoreOk && linesOk)
                {
                    CompleteLevel();
                }
            }
        }

        private void CompleteLevel()
        {
            Debug.Log("[LevelManager] Level complete: " + CurrentLevel.levelNumber);

            int next = Mathf.Min(CurrentIndex + 1, levels.Count - 1);
            PlayerPrefs.SetInt(SaveKey, next);
            PlayerPrefs.Save();

            GameStateManager.Instance.ChangeState(GameState.LevelComplete);
        }

        public void NextLevel()
        {
            if (HasNextLevel) CurrentIndex++;

            PlayerPrefs.SetInt(SaveKey, CurrentIndex);
            linesClearedThisLevel = 0;

            if (BoardManager.Instance != null)
                BoardManager.Instance.InitializeBoard();

            if (GameManager.Instance != null)
                GameManager.Instance.RestartGame();
        }

        // ------------------------------------------------------------
        // BlockSpawner is function se poochega ke ye shape is level mein allowed hai ya nahi
        // ------------------------------------------------------------
        public bool IsShapeAllowed(BlockShapeData shape)
        {
            LevelData lvl = CurrentLevel;
            if (lvl == null || shape == null || shape.offsets == null || shape.offsets.Count == 0) return true;

            if (lvl.maxCellsPerBlock > 0 && shape.CellCount > lvl.maxCellsPerBlock) return false;

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (var c in shape.offsets)
            {
                if (c.x < minX) minX = c.x;
                if (c.x > maxX) maxX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.y > maxY) maxY = c.y;
            }

            int shapeW = maxX - minX + 1;
            int shapeH = maxY - minY + 1;

            // Shape board se bara na ho
            return shapeW <= lvl.boardWidth && shapeH <= lvl.boardHeight;
        }

        [ContextMenu("Reset Progress (Level 1 se shuru)")]
        private void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            CurrentIndex = 0;
            linesClearedThisLevel = 0;
            Debug.Log("[LevelManager] Progress reset.");
        }
    }
}