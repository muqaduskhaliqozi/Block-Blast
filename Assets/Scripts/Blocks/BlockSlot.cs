using UnityEngine;
using BlockBlast.Core;
using BlockBlast.Visuals;

namespace BlockBlast.Blocks
{
    public class BlockSlot : MonoBehaviour
    {
        public int SlotIndex { get; private set; }
        public BlockController CurrentBlock { get; private set; }
        public float FittedScale { get; private set; } = 0.55f;

        [SerializeField] private SpriteRenderer slotBackground;
        [SerializeField] private SpriteRenderer slotShadow;

        public bool IsEmpty => CurrentBlock == null;

        public void Initialize(int index, GameConfig config, float slotWidth, float slotHeight)
        {
            SlotIndex = index;
            GameConfig cfg = config != null ? config : GameConfig.Default;

            // Soft shadow behind slot panel
            if (slotShadow == null)
            {
                GameObject shadowObj = new GameObject("SlotShadow");
                shadowObj.transform.SetParent(transform, false);
                slotShadow = shadowObj.AddComponent<SpriteRenderer>();
            }
            slotShadow.sprite = ProceduralSpriteGenerator.GetSoftShadowSprite();
            slotShadow.color = cfg.shadowColor;
            slotShadow.sortingOrder = -15;
            slotShadow.transform.localPosition = cfg.shadowOffset;
            slotShadow.transform.localScale = new Vector3(slotWidth, slotHeight, 1f) * 1.06f;

            // Slot dark panel background
            if (slotBackground == null)
            {
                GameObject bgObj = new GameObject("SlotFrame");
                bgObj.transform.SetParent(transform, false);
                bgObj.transform.localPosition = Vector3.zero;
                slotBackground = bgObj.AddComponent<SpriteRenderer>();
            }
            slotBackground.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite(256, 0.12f);
            slotBackground.color = cfg.slotPanelColor;
            slotBackground.sortingOrder = -10;
            slotBackground.transform.localScale = new Vector3(slotWidth, slotHeight, 1f);
        }

        public void SetBlock(BlockController block)
        {
            CurrentBlock = block;
            if (block != null)
            {
                block.Slot = this;
                block.transform.SetParent(transform, false);
                block.transform.localPosition = Vector3.zero;

                // Calculate fitted scale so shape never overflows slot padding
                FittedScale = CalculateFittedScale(block.ShapeData);
                block.transform.localScale = Vector3.one * FittedScale;
            }
        }

        private float CalculateFittedScale(BlockShapeData shape)
        {
            if (shape == null || shape.offsets == null || shape.offsets.Count == 0) return 0.55f;

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (var c in shape.offsets)
            {
                if (c.x < minX) minX = c.x;
                if (c.x > maxX) maxX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.y > maxY) maxY = c.y;
            }

            int shapeWidthUnits = (maxX - minX + 1);
            int shapeHeightUnits = (maxY - minY + 1);
            int maxDim = Mathf.Max(shapeWidthUnits, shapeHeightUnits);

            // Scale factor to fit inside ~1.85 world unit slot area
            float targetSlotArea = 1.85f;
            float fitScale = targetSlotArea / (maxDim * 0.95f);

            return Mathf.Clamp(fitScale, 0.35f, 0.65f);
        }

        public void ClearBlock()
        {
            if (CurrentBlock != null)
            {
                CurrentBlock.Slot = null;
                CurrentBlock = null;
            }
        }
    }
}
