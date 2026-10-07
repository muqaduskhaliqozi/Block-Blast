using UnityEngine;

namespace BlockBlast.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BlockBlast/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Board Settings")]
        [Range(4, 12)]
        public int boardWidth = 8;
        [Range(4, 12)]
        public int boardHeight = 8;
        public float cellSize = 0.95f;
        public float cellSpacing = 0.08f;
        public float cellGapRatio = 0.92f; // Cell size multiplier for crisp visible gaps
        public float cornerRadiusRatio = 0.18f;
        public float boardPadding = 0.25f;

        [Header("Drag & Input Settings")]
        public float dragTouchOffsetY = 1.4f; // Offset block above finger/pointer so it's not hidden by hand
        public float trayBlockScale = 0.55f; // Scale in tray slot
        public float dragBlockScale = 1.0f;  // Scale while dragging
        public float lerpSpeed = 25f;

        [Header("Scoring Settings")]
        public int scorePerCell = 1;
        public int baseLineClearScore = 10;
        public int comboBonusMultiplier = 5;

        [Header("Animations")]
        public float lineClearDuration = 0.25f;
        public float invalidReturnDuration = 0.2f;

        [Header("Block Spawning")]
        public int traySlotCount = 3;

        [Header("Background & Panels")]
        public Color bgGradientTop = new Color(0.106f, 0.165f, 0.361f, 1.0f);    // #1B2A5C
        public Color bgGradientBottom = new Color(0.055f, 0.078f, 0.200f, 1.0f); // #0E1433
        public Color bgDecoCircleColor = new Color(1.0f, 1.0f, 1.0f, 0.035f);    // Subtle soft circle alpha
        public Color boardBackgroundColor = new Color(0.09f, 0.11f, 0.22f, 1.0f);
        public Color emptyCellColor = new Color(0.165f, 0.208f, 0.388f, 1.0f);   // #2A3563
        public Color emptyCellBorderColor = new Color(0.24f, 0.30f, 0.52f, 0.6f);
        public Color slotPanelColor = new Color(0.12f, 0.14f, 0.26f, 1.0f);
        public Color shadowColor = new Color(0.0f, 0.0f, 0.0f, 0.35f);
        public Vector3 shadowOffset = new Vector3(0.08f, -0.08f, 0f);

        [Header("Preview Highlights")]
        public Color previewValidColor = new Color(0.20f, 0.83f, 0.60f, 0.55f);  // Soft green tint #34D399
        public Color previewInvalidColor = new Color(0.97f, 0.44f, 0.44f, 0.55f); // Soft red tint #F87171

        [Header("Glossy Tile Styling")]
        public float highlightIntensity = 0.25f; // Lighter top edge highlight
        public float shadowIntensity = 0.25f;    // Darker bottom edge shadow
        [Header("Colors")]
        public Color backgroundColor = new Color(0.055f, 0.078f, 0.2f, 1f);

        [Header("Vibrant Block Palette")]
        public Color[] blockColors = new Color[]
        {
            new Color(1.00f, 0.29f, 0.36f), // Coral Red (#FF4B5C)
            new Color(0.00f, 0.82f, 1.00f), // Sky Blue (#00D2FF)
            new Color(1.00f, 0.82f, 0.40f), // Gold Yellow (#FFD166)
            new Color(0.02f, 0.84f, 0.63f), // Mint Emerald (#06D6A0)
            new Color(0.51f, 0.22f, 0.93f), // Deep Violet (#8338EC)
            new Color(1.00f, 0.62f, 0.11f), // Vibrant Orange (#FF9F1C)
            new Color(1.00f, 0.00f, 0.50f), // Electric Pink (#FF007F)
            new Color(0.00f, 0.96f, 0.83f)  // Bright Cyan (#00F5D4)
        };

        private static GameConfig _defaultInstance;
        public static GameConfig Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    _defaultInstance = CreateInstance<GameConfig>();
                }
                return _defaultInstance;
            }
        }
    }
}
