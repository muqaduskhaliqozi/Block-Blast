using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BlockBlast.Board;
using BlockBlast.Blocks;
using BlockBlast.Placement;
using BlockBlast.Core;

namespace BlockBlast.Game
{
    public class GameOverChecker : MonoBehaviour
    {
        public static GameOverChecker Instance { get; private set; }

        [Header("Auto Watch (safety net)")]
        [Tooltip("Khud baar baar check karta hai ke koi chaal bachi hai ya nahi")]
        [SerializeField] private bool autoWatch = true;
        [SerializeField] private float watchInterval = 0.4f;

        [Header("Debug")]
        [SerializeField] private bool verboseLogs = true;

        private bool gameOverTriggered;
        private int deadChecks;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (autoWatch) StartCoroutine(WatchRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        // ------------------------------------------------------------
        // PUBLIC: purana code isay call kare to bhi theek, na kare to bhi theek
        // true = Game Over ho gaya
        // ------------------------------------------------------------
        public bool CheckGameOver()
        {
            if (!NoMovesLeft(verboseLogs)) return false;

            TriggerGameOver();
            return true;
        }

        [ContextMenu("Force Check")]
        public void ForceCheck()
        {
            bool over = NoMovesLeft(true);
            Debug.Log("[GameOverChecker] Force Check result -> Game Over = " + over);
            if (over) TriggerGameOver();
        }

        // Retry ke baad agar chahein to call kar sakte hain (zaroori nahi)
        public void ResetChecker()
        {
            deadChecks = 0;
        }
        // ------------------------------------------------------------
        // Safety net: Playing state mein 2 baar lagataar "koi chaal nahi" aaye
        // to Game Over. 2 baar is liye ke clear animation ke beech galat
        // Game Over na aa jaye.
        // ------------------------------------------------------------
        private IEnumerator WatchRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(watchInterval);
            float logTimer = 0f;

            while (true)
            {
                yield return wait;

                GameStateManager sm = GameStateManager.Instance;
                if (sm == null)
                {
                    Debug.LogWarning("[GameOverChecker] GameStateManager.Instance null hai.");
                    continue;
                }

                // Har 3 second baad batayein ke checker zinda hai aur state kya hai
                logTimer += watchInterval;
                if (verboseLogs && logTimer >= 3f)
                {
                    logTimer = 0f;
                    Debug.Log("[GameOverChecker] AutoWatch zinda hai, state = " + sm.CurrentState);
                }

                if (sm.CurrentState != GameState.Playing)
                {
                    deadChecks = 0;
                    continue;
                }

                if (NoMovesLeft(false))
                {
                    deadChecks++;
                    if (deadChecks >= 2)
                    {
                        Debug.Log("[GameOverChecker] AutoWatch: koi chaal nahi bachi.");
                        TriggerGameOver();
                    }
                }
                else
                {
                    deadChecks = 0;
                }
            }
        }
        

        // ------------------------------------------------------------
        // true = tray mein blocks hain aur koi bhi kahin fit nahi hota
        // ------------------------------------------------------------
        private bool NoMovesLeft(bool log)
        {
            BoardManager board = BoardManager.Instance;
            BlockTray tray = BlockTray.Instance;

            if (board == null || tray == null)
            {
                if (log) Debug.LogWarning("[GameOverChecker] BoardManager ya BlockTray Instance null hai.");
                return false;
            }

            List<BlockController> blocks = tray.GetAvailableBlocks();

            if (blocks == null || blocks.Count == 0)
            {
                if (log) Debug.Log("[GameOverChecker] Tray khali hai, Game Over nahi.");
                return false;
            }

            int checkedCount = 0;

            foreach (BlockController block in blocks)
            {
                if (block == null || block.ShapeData == null) continue;
                checkedCount++;

                bool canFit = PlacementValidator.FindAnyValidPlacement(board, block.ShapeData, out _);

                if (log) Debug.Log("[GameOverChecker] " + block.name + " legal move = " + canFit);

                if (canFit) return false;
            }

            // Agar koi valid block hi nahi mila check karne ke liye, to Game Over mat karein
            if (checkedCount == 0) return false;

            if (log) Debug.Log("[GameOverChecker] NO LEGAL MOVES.");
            return true;
        }

        private void TriggerGameOver()
        {
            GameStateManager sm = GameStateManager.Instance;
            if (sm == null)
            {
                Debug.LogError("[GameOverChecker] GameStateManager.Instance NULL hai!");
                return;
            }

            if (sm.CurrentState == GameState.GameOver) return;

            Debug.Log("[GameOverChecker] State -> GameOver");
            sm.ChangeState(GameState.GameOver);
        }
    }
}