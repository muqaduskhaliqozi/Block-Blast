using UnityEngine;
using BlockBlast.Board;
using BlockBlast.Blocks;
using BlockBlast.Scoring;

namespace BlockBlast.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameConfig config;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (config == null) config = GameConfig.Default;
            InitializeGame();
        }

        public void InitializeGame()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Initializing);
            }

            // 1. Initialize Board
            if (BoardManager.Instance != null)
            {
                BoardManager.Instance.InitializeBoard(config);
            }

            // 2. Initialize Block Tray
            if (BlockTray.Instance != null)
            {
                BlockTray.Instance.InitializeTray(config);
            }

            // 3. Reset Scores & Combos
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }

            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.ResetCombo();
            }

            // 4. Start Gameplay State
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }

            Debug.Log("[GameManager] Game Initialized & Started!");
        }

        public void RestartGame()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Restarting);
            }

            if (BoardManager.Instance != null)
            {
                BoardManager.Instance.ResetBoard();
            }

            if (BlockTray.Instance != null)
            {
                BlockTray.Instance.InitializeTray(config);
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }

            if (ComboManager.Instance != null)
            {
                ComboManager.Instance.ResetCombo();
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }

            Debug.Log("[GameManager] Game Restarted Successfully!");
        }
    }
}
