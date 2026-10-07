using UnityEngine;
using BlockBlast.Board;
using BlockBlast.Core;
using BlockBlast.Visuals;

namespace BlockBlast.Blocks
{
    public class BlockPiece : MonoBehaviour
    {
        public BoardCoordinate Offset { get; private set; }
        [SerializeField] private SpriteRenderer pieceRenderer;
        [SerializeField] private SpriteRenderer shadowRenderer;

        public void Initialize(BoardCoordinate offset, Color color, float renderSize, GameConfig config)
        {
            Offset = offset;
            GameConfig cfg = config != null ? config : GameConfig.Default;

            // Soft drop shadow
            if (shadowRenderer == null)
            {
                GameObject shadowObj = new GameObject("Shadow");
                shadowObj.transform.SetParent(transform, false);
                shadowObj.transform.localPosition = cfg.shadowOffset;
                shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
            }

            shadowRenderer.sprite = ProceduralSpriteGenerator.GetSoftShadowSprite();
            shadowRenderer.color = cfg.shadowColor;
            shadowRenderer.sortingOrder = 8;
            shadowRenderer.transform.localScale = Vector3.one * 1.05f;

            // Glossy Block Tile
            if (pieceRenderer == null)
            {
                pieceRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            pieceRenderer.sprite = ProceduralSpriteGenerator.GetGlossyBlockSprite(128, cfg.cornerRadiusRatio, cfg.highlightIntensity, cfg.shadowIntensity);
            pieceRenderer.color = color;
            pieceRenderer.sortingOrder = 10;
        }

        public void SetColor(Color color)
        {
            if (pieceRenderer != null)
            {
                pieceRenderer.color = color;
            }
        }

        public void SetSortingOrder(int baseOrder)
        {
            if (pieceRenderer != null) pieceRenderer.sortingOrder = baseOrder + 2;
            if (shadowRenderer != null) shadowRenderer.sortingOrder = baseOrder;
        }
    }
}
