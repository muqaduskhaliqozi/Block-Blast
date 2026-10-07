using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BlockBlast.Board;
using BlockBlast.Blocks;
using BlockBlast.Core;
using BlockBlast.Scoring;
using BlockBlast.Game;
using BlockBlast.Audio;

namespace BlockBlast.Placement
{
    public class BlockPlacementSystem : MonoBehaviour
    {
        public static BlockPlacementSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ExecutePlacement(BlockController block, BoardCoordinate origin)
        {
            if (block == null || BoardManager.Instance == null)
                return;

            BoardManager board = BoardManager.Instance;
            BlockSlot slot = block.Slot;

            // Remember where the shape lands and the score before, for the "+points" popup
            Vector3 placedCenter = Vector3.zero;
            foreach (var offset in block.ShapeData.offsets)
                placedCenter += board.CellToWorldPosition(origin + offset);
            placedCenter /= Mathf.Max(1, block.ShapeData.offsets.Count);
            int scoreBefore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;

            // --------------------------------------------------
            // 1. PLACE BLOCK ON BOARD
            // --------------------------------------------------

            board.PlaceShape(
                block.ShapeData.offsets,
                origin,
                block.BlockColor
            );

            // --------------------------------------------------
            // 2. PLACE SOUND
            // --------------------------------------------------

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPlaceSound();
            }

            // --------------------------------------------------
            // 3. CLEAR TRAY SLOT
            // --------------------------------------------------

            if (slot != null)
            {
                slot.ClearBlock();
            }

            // Store score information before destroying block.
            int cellCount = block.ShapeData.CellCount;

            // --------------------------------------------------
            // 4. DESTROY PLACED BLOCK
            // --------------------------------------------------

            Destroy(block.gameObject);

            // --------------------------------------------------
            // 5. PLACEMENT SCORE
            // --------------------------------------------------

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPlacementScore(cellCount);
            }

            // --------------------------------------------------
            // 6. CHECK COMPLETED LINES
            // --------------------------------------------------

            board.CheckCompletedLines(
                out List<int> fullRows,
                out List<int> fullCols
            );

            int totalLines = fullRows.Count + fullCols.Count;

            // --------------------------------------------------
            // 7. HANDLE LINE CLEAR
            // --------------------------------------------------

            if (totalLines > 0)
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(
                        GameState.Clearing
                    );
                }

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddLineClearScore(totalLines);
                }

                if (ComboManager.Instance != null)
                {
                    ComboManager.Instance.RegisterLineClear(totalLines);
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLineClearSound(totalLines);
                }

                ShowScorePopup(placedCenter, scoreBefore, true);

                StartCoroutine(
                    board.ClearLinesRoutine(
                        fullRows,
                        fullCols,
                        PostPlacementSequence
                    )
                );
            }
            else
            {
                if (ComboManager.Instance != null)
                {
                    ComboManager.Instance.ResetCombo();
                }

                ShowScorePopup(placedCenter, scoreBefore, false);

                PostPlacementSequence();
            }
        }

        private void ShowScorePopup(Vector3 worldPos, int scoreBefore, bool big)
        {
            if (ScoreManager.Instance == null || BlockBlast.FX.GameFX.Instance == null) return;
            int gained = ScoreManager.Instance.CurrentScore - scoreBefore;
            BlockBlast.FX.GameFX.Instance.ShowScorePopup(worldPos, gained, big);
        }

        // ======================================================
        // POST PLACEMENT
        // ======================================================

        private void PostPlacementSequence()
        {
            Debug.Log("[BlockPlacementSystem] PostPlacementSequence started.");

            // --------------------------------------------------
            // 1. REFILL TRAY
            // --------------------------------------------------

            if (BlockTray.Instance != null)
            {
                BlockTray.Instance.CheckAndRefillIfEmpty();
            }
            else
            {
                Debug.LogWarning(
                    "[BlockPlacementSystem] BlockTray.Instance is null."
                );
            }

            // --------------------------------------------------
            // 2. CHECK GAME OVER
            // --------------------------------------------------

            if (GameOverChecker.Instance != null)
            {
                Debug.Log(
                    "[BlockPlacementSystem] Checking Game Over..."
                );

                bool isGameOver =
                    GameOverChecker.Instance.CheckGameOver();

                if (isGameOver)
                {
                    Debug.Log(
                        "[BlockPlacementSystem] GAME OVER detected."
                    );

                    // GameOverChecker is responsible for changing
                    // the state to GameOver.
                    //
                    // Do NOT change it back to Playing here.
                    return;
                }
            }
            else
            {
                Debug.LogWarning(
                    "[BlockPlacementSystem] GameOverChecker.Instance is null."
                );
            }

            // --------------------------------------------------
            // 3. RETURN TO PLAYING
            // --------------------------------------------------

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(
                    GameState.Playing
                );
            }

            Debug.Log(
                "[BlockPlacementSystem] Returned to Playing state."
            );
        }
    }
}