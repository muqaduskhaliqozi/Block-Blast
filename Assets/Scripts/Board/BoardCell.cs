using System.Collections;
using UnityEngine;
using BlockBlast.Core;
using BlockBlast.Visuals;

namespace BlockBlast.Board
{
    public class BoardCell : MonoBehaviour
    {
        public BoardCoordinate Coordinate { get; private set; }
        public bool IsOccupied { get; private set; }
        public Color CurrentColor { get; private set; }

        [SerializeField] private SpriteRenderer cellRenderer;
        [SerializeField] private SpriteRenderer borderRenderer;
        [SerializeField] private SpriteRenderer overlayRenderer;

        private GameConfig config;

        public void Initialize(BoardCoordinate coord, GameConfig cfg)
        {
            Coordinate = coord;
            config = cfg != null ? cfg : GameConfig.Default;
            IsOccupied = false;

            // Find an existing SpriteRenderer first, only add one if missing
            if (cellRenderer == null)
            {
                cellRenderer = GetComponent<SpriteRenderer>();
            }

            if (cellRenderer == null)
            {
                cellRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            // AddComponent returns null when the object has a conflicting component
            // (for example CanvasRenderer / Image from a UI object).
            if (cellRenderer == null)
            {
                Debug.LogError(
                    $"[BoardCell] Could not add a SpriteRenderer to '{name}'. " +
                    "Board cells must be plain world objects: remove any Image, CanvasRenderer " +
                    "or RectTransform from the cell and do not parent the board under the UI Canvas.",
                    this);
                return;
            }

            cellRenderer.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite();
            cellRenderer.color = config.emptyCellColor;
            cellRenderer.sortingOrder = 0;

            // Inner subtle border (reuse the child if the prefab already has one)
            borderRenderer = GetOrCreateChildRenderer("Border");
            borderRenderer.gameObject.SetActive(true);
            borderRenderer.sprite = ProceduralSpriteGenerator.GetBorderSquareSprite();
            borderRenderer.color = config.emptyCellBorderColor;
            borderRenderer.sortingOrder = 1;

            // Secondary overlay for previews & highlight feedback
            overlayRenderer = GetOrCreateChildRenderer("Overlay");
            overlayRenderer.gameObject.SetActive(true);
            overlayRenderer.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite();
            overlayRenderer.color = Color.clear;
            overlayRenderer.sortingOrder = 5;
        }

        // Finds a child by name (or creates it) and makes sure it has a SpriteRenderer.
        // This keeps Initialize safe to call more than once (no duplicate Border/Overlay).
        private SpriteRenderer GetOrCreateChildRenderer(string childName)
        {
            Transform child = transform.Find(childName);
            if (child == null)
            {
                GameObject obj = new GameObject(childName);
                obj.transform.SetParent(transform, false);
                child = obj.transform;
            }

            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = child.gameObject.AddComponent<SpriteRenderer>();
            }

            return sr;
        }

        public void SetOccupied(Color color)
        {
            IsOccupied = true;
            CurrentColor = color;
            cellRenderer.sprite = ProceduralSpriteGenerator.GetGlossyBlockSprite();
            cellRenderer.color = color;
            borderRenderer.gameObject.SetActive(false);
            overlayRenderer.color = Color.clear;
        }

        public void SetEmpty()
        {
            IsOccupied = false;
            CurrentColor = config.emptyCellColor;
            cellRenderer.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite();
            cellRenderer.color = config.emptyCellColor;
            borderRenderer.gameObject.SetActive(true);
            borderRenderer.color = config.emptyCellBorderColor;
            overlayRenderer.color = Color.clear;
        }

        public void SetHighlightValid(Color color)
        {
            if (IsOccupied) return;
            overlayRenderer.color = color;
        }

        public void SetHighlightInvalid(Color color)
        {
            overlayRenderer.color = color;
        }

        public void ClearHighlight()
        {
            overlayRenderer.color = Color.clear;
        }

        public IEnumerator AnimateClear(float duration)
        {
            Vector3 originalScale = transform.localScale;
            Color startColor = cellRenderer.color;
            Color flashColor = Color.white;

            float halfDuration = duration * 0.5f;
            float elapsed = 0f;

            // Flash & scale up slightly
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                cellRenderer.color = Color.Lerp(startColor, flashColor, t);
                transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.15f, t);
                yield return null;
            }

            elapsed = 0f;
            // Shrink & fade to empty
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                cellRenderer.color = Color.Lerp(flashColor, config.emptyCellColor, t);
                transform.localScale = Vector3.Lerp(originalScale * 1.15f, Vector3.zero, t);
                yield return null;
            }

            SetEmpty();
            transform.localScale = originalScale;
        }
    }
}