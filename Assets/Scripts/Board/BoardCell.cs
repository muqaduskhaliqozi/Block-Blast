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
        private Vector3 baseScale = Vector3.one;
        private Coroutine bounceRoutine;

        public void Initialize(BoardCoordinate coord, GameConfig cfg)
        {
            Coordinate = coord;
            config = cfg != null ? cfg : GameConfig.Default;
            IsOccupied = false;
            baseScale = transform.localScale; // BoardManager sets the render size before calling Initialize

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

            // Little "thump" when a block lands
            if (Application.isPlaying && isActiveAndEnabled)
            {
                StopBounce();
                bounceRoutine = StartCoroutine(PlaceBounce());
            }
        }

        public void SetEmpty()
        {
            StopBounce();
            IsOccupied = false;
            CurrentColor = config.emptyCellColor;
            cellRenderer.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite();
            cellRenderer.color = config.emptyCellColor;
            borderRenderer.gameObject.SetActive(true);
            borderRenderer.color = config.emptyCellBorderColor;
            overlayRenderer.color = Color.clear;
            transform.localScale = baseScale;
        }

        /// <summary>Greys out a filled cell (game over wave).</summary>
        public void SetDimmed()
        {
            if (!IsOccupied || cellRenderer == null) return;
            float g = CurrentColor.grayscale;
            Color grey = new Color(g, g, g, 1f) * 0.55f;
            grey.a = 1f;
            cellRenderer.color = Color.Lerp(CurrentColor, grey, 0.75f);
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

        private void StopBounce()
        {
            if (bounceRoutine != null)
            {
                StopCoroutine(bounceRoutine);
                bounceRoutine = null;
                transform.localScale = baseScale;
            }
        }

        private IEnumerator PlaceBounce()
        {
            float duration = 0.22f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                // 0.82 -> overshoot 1.08 -> 1
                float s = Mathf.LerpUnclamped(0.82f, 1f, BlockBlast.UI.Ease.OutBack(k, 3.5f));
                transform.localScale = baseScale * s;
                yield return null;
            }
            transform.localScale = baseScale;
            bounceRoutine = null;
        }

        public IEnumerator AnimateClear(float duration)
        {
            return AnimateClear(duration, 0f);
        }

        public IEnumerator AnimateClear(float duration, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            StopBounce();

            Color startColor = cellRenderer.color;
            Color flashColor = Color.white;

            if (BlockBlast.FX.GameFX.Instance != null)
                BlockBlast.FX.GameFX.Instance.CellBurst(transform.position, startColor);

            float halfDuration = duration * 0.5f;
            float elapsed = 0f;

            // Flash & scale up slightly
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                cellRenderer.color = Color.Lerp(startColor, flashColor, t);
                transform.localScale = Vector3.Lerp(baseScale, baseScale * 1.2f, t);
                yield return null;
            }

            elapsed = 0f;
            // Shrink & fade to empty
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                cellRenderer.color = Color.Lerp(flashColor, config.emptyCellColor, t);
                transform.localScale = Vector3.Lerp(baseScale * 1.2f, Vector3.zero, BlockBlast.UI.Ease.InBack(t));
                yield return null;
            }

            SetEmpty();
            transform.localScale = baseScale;
        }
    }
}
