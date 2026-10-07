using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Board;
using BlockBlast.Core;

namespace BlockBlast.Blocks
{
    public class BlockController : MonoBehaviour
    {
        public BlockShapeData ShapeData { get; private set; }
        public Color BlockColor { get; private set; }
        public BlockSlot Slot { get; set; }
        public List<BlockPiece> Pieces { get; private set; } = new List<BlockPiece>();

        private Transform piecesParent;
        private GameConfig config;
        private Coroutine animRoutine;

        /// <summary>Stops any running tray animation (called when the player picks the block up).</summary>
        public void CancelAnimations()
        {
            if (animRoutine != null)
            {
                StopCoroutine(animRoutine);
                animRoutine = null;
            }
        }

        /// <summary>Scales the block in from nothing with a springy overshoot.</summary>
        public void PlaySpawnPop(float targetScale, float delay)
        {
            CancelAnimations();
            if (!isActiveAndEnabled) return;
            animRoutine = StartCoroutine(SpawnPopRoutine(targetScale, delay));
        }

        private System.Collections.IEnumerator SpawnPopRoutine(float targetScale, float delay)
        {
            transform.localScale = Vector3.zero;
            if (delay > 0f) yield return new WaitForSeconds(delay);
            float duration = 0.35f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.one * targetScale * BlockBlast.UI.Ease.OutBack(k, 2.6f);
                yield return null;
            }
            transform.localScale = Vector3.one * targetScale;
            animRoutine = null;
        }

        public void Initialize(BlockShapeData shapeData, Color color, GameConfig cfg = null)
        {
            ShapeData = shapeData;
            BlockColor = color;
            config = cfg != null ? cfg : GameConfig.Default;

            ClearPieces();

            GameObject parentObj = new GameObject("PiecesParent");
            parentObj.transform.SetParent(transform, false);
            piecesParent = parentObj.transform;

            float step = config.cellSize + config.cellSpacing;
            float renderSize = config.cellSize * config.cellGapRatio;
            Vector2 centerOffset = shapeData.GetCenterOffset();

            foreach (var offset in shapeData.offsets)
            {
                GameObject pieceObj = new GameObject($"Piece_{offset.x}_{offset.y}");
                pieceObj.transform.SetParent(piecesParent, false);

                // Local position relative to shape center
                float localX = (offset.x - centerOffset.x) * step;
                float localY = (offset.y - centerOffset.y) * step;
                pieceObj.transform.localPosition = new Vector3(localX, localY, 0f);
                pieceObj.transform.localScale = Vector3.one * renderSize;

                BlockPiece piece = pieceObj.AddComponent<BlockPiece>();
                piece.Initialize(offset, color, renderSize, config);
                Pieces.Add(piece);
            }
        }

        private void ClearPieces()
        {
            if (piecesParent != null)
            {
                if (Application.isPlaying) Destroy(piecesParent.gameObject);
                else DestroyImmediate(piecesParent.gameObject);
                piecesParent = null;
            }
            Pieces.Clear();
        }

        public Vector3 GetWorldPositionOfPiece(BoardCoordinate offset)
        {
            float step = config.cellSize + config.cellSpacing;
            Vector2 centerOffset = ShapeData.GetCenterOffset();
            float localX = (offset.x - centerOffset.x) * step;
            float localY = (offset.y - centerOffset.y) * step;

            return transform.TransformPoint(new Vector3(localX, localY, 0f));
        }

        public void SetSortingOrder(int order)
        {
            foreach (var piece in Pieces)
            {
                piece.SetSortingOrder(order);
            }
        }

        public void ReturnToSlot()
        {
            if (Slot != null)
            {
                transform.SetParent(Slot.transform, true);
                CancelAnimations();
                animRoutine = StartCoroutine(SmoothReturnRoutine());
            }
        }

        private System.Collections.IEnumerator SmoothReturnRoutine()
        {
            Vector3 startPos = transform.localPosition;
            Vector3 targetPos = Vector3.zero;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.one * (Slot != null ? Slot.FittedScale : config.trayBlockScale);

            float duration = config.invalidReturnDuration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = BlockBlast.UI.Ease.OutCubic(Mathf.Clamp01(elapsed / duration));
                transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localPosition = targetPos;
            transform.localScale = targetScale;
            SetSortingOrder(10);
            animRoutine = null;
        }
    }
}
