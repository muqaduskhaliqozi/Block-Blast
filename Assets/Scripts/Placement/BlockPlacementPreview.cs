using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Board;
using BlockBlast.Blocks;
using BlockBlast.Core;

namespace BlockBlast.Placement
{
    public class BlockPlacementPreview : MonoBehaviour
    {
        public static BlockPlacementPreview Instance { get; private set; }

        [SerializeField] private GameConfig config;
        private BoardCoordinate lastOrigin;
        private bool isShowingPreview = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void UpdatePreview(BoardManager board, BlockController block, BoardCoordinate origin, bool isValid)
        {
            if (config == null) config = GameConfig.Default;

            ClearPreview(board);

            if (board == null || block == null) return;

            Color highlightColor = isValid ? config.previewValidColor : config.previewInvalidColor;

            foreach (var offset in block.ShapeData.offsets)
            {
                BoardCoordinate targetCoord = origin + offset;
                BoardCell cell = board.GetCell(targetCoord);
                if (cell != null)
                {
                    if (isValid)
                    {
                        cell.SetHighlightValid(highlightColor);
                    }
                    else
                    {
                        cell.SetHighlightInvalid(highlightColor);
                    }
                }
            }

            lastOrigin = origin;
            isShowingPreview = true;
        }

        public void ClearPreview(BoardManager board)
        {
            if (!isShowingPreview) return;
            if (board != null)
            {
                board.ClearAllHighlights();
            }
            isShowingPreview = false;
        }
    }
}
