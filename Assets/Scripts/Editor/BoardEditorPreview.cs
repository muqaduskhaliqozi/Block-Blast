using UnityEngine;
using UnityEditor;
using BlockBlast.Core;
using BlockBlast.Board;
using BlockBlast.Blocks;
using BlockBlast.Visuals;

namespace BlockBlast.EditorTools
{
    [InitializeOnLoad]
    public class BoardEditorPreview
    {
        private const string PREVIEW_ROOT_NAME = "[Editor_Board_Preview]";

        static BoardEditorPreview()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                ClearPreview();
            }
        }

        [MenuItem("Tools/Block Blast/Refresh Preview")]
        public static void RefreshPreview()
        {
            ClearPreview();

            if (Application.isPlaying) return;

            GameConfig config = GameConfig.Default;
            GameObject root = new GameObject(PREVIEW_ROOT_NAME);
            root.hideFlags = HideFlags.DontSave;

            // 1. Render Preview Background & Board Frame
            float step = config.cellSize + config.cellSpacing;
            float totalWidth = config.boardWidth * step - config.cellSpacing;
            float totalHeight = config.boardHeight * step - config.cellSpacing;
            float bgPadding = config.boardPadding;

            Vector3 boardCenter = new Vector3(0f, 0.6f, 0f);
            Vector3 originWorld = boardCenter - new Vector3(totalWidth * 0.5f - config.cellSize * 0.5f, totalHeight * 0.5f - config.cellSize * 0.5f, 0f);

            // Shadow
            GameObject boardShadow = new GameObject("Preview_BoardShadow");
            boardShadow.transform.SetParent(root.transform, false);
            boardShadow.transform.position = boardCenter + config.shadowOffset;
            boardShadow.transform.localScale = new Vector3(totalWidth + bgPadding * 2f, totalHeight + bgPadding * 2f, 1f) * 1.08f;
            SpriteRenderer shadowSr = boardShadow.AddComponent<SpriteRenderer>();
            shadowSr.sprite = ProceduralSpriteGenerator.GetSoftShadowSprite();
            shadowSr.color = config.shadowColor;
            shadowSr.sortingOrder = -10;

            // Board Frame
            GameObject boardFrame = new GameObject("Preview_BoardFrame");
            boardFrame.transform.SetParent(root.transform, false);
            boardFrame.transform.position = boardCenter;
            boardFrame.transform.localScale = new Vector3(totalWidth + bgPadding * 2f, totalHeight + bgPadding * 2f, 1f);
            SpriteRenderer frameSr = boardFrame.AddComponent<SpriteRenderer>();
            frameSr.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite(256, 0.08f);
            frameSr.color = config.boardBackgroundColor;
            frameSr.sortingOrder = -5;

            // 8x8 Empty Cells
            float renderSize = config.cellSize * config.cellGapRatio;
            for (int y = 0; y < config.boardHeight; y++)
            {
                for (int x = 0; x < config.boardWidth; x++)
                {
                    Vector3 cellPos = originWorld + new Vector3(x * step, y * step, 0f);
                    GameObject cellObj = new GameObject($"Preview_Cell_{x}_{y}");
                    cellObj.transform.SetParent(root.transform, false);
                    cellObj.transform.position = cellPos;
                    cellObj.transform.localScale = Vector3.one * renderSize;

                    SpriteRenderer cellSr = cellObj.AddComponent<SpriteRenderer>();
                    cellSr.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite();
                    cellSr.color = config.emptyCellColor;
                    cellSr.sortingOrder = 0;

                    // Inner border
                    GameObject borderObj = new GameObject("Border");
                    borderObj.transform.SetParent(cellObj.transform, false);
                    SpriteRenderer bSr = borderObj.AddComponent<SpriteRenderer>();
                    bSr.sprite = ProceduralSpriteGenerator.GetBorderSquareSprite();
                    bSr.color = config.emptyCellBorderColor;
                    bSr.sortingOrder = 1;
                }
            }

            // 2. Render 3 Sample Tray Slots & Blocks
            Vector3 trayCenter = new Vector3(0f, -4.2f, 0f);
            float slotSpacing = 2.45f;
            float startX = -(config.traySlotCount - 1) * 0.5f * slotSpacing;

            BlockShapeData[] sampleShapes = new BlockShapeData[]
            {
                new BlockShapeData("Corner_2x2", new BoardCoordinate(0,0), new BoardCoordinate(1,0), new BoardCoordinate(0,1)),
                new BlockShapeData("Bar_3x1", new BoardCoordinate(0,0), new BoardCoordinate(1,0), new BoardCoordinate(2,0)),
                new BlockShapeData("Square_2x2", new BoardCoordinate(0,0), new BoardCoordinate(1,0), new BoardCoordinate(0,1), new BoardCoordinate(1,1))
            };

            for (int i = 0; i < config.traySlotCount; i++)
            {
                Vector3 slotPos = trayCenter + new Vector3(startX + i * slotSpacing, 0f, 0f);

                // Slot Panel
                GameObject slotPanel = new GameObject($"Preview_SlotFrame_{i}");
                slotPanel.transform.SetParent(root.transform, false);
                slotPanel.transform.position = slotPos;
                slotPanel.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

                SpriteRenderer slotSr = slotPanel.AddComponent<SpriteRenderer>();
                slotSr.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite(256, 0.12f);
                slotSr.color = config.slotPanelColor;
                slotSr.sortingOrder = -10;

                // Sample Block
                BlockShapeData shape = sampleShapes[i % sampleShapes.Length];
                Color color = config.blockColors[i % config.blockColors.Length];

                GameObject blockObj = new GameObject($"Preview_Block_{i}");
                blockObj.transform.SetParent(root.transform, false);
                blockObj.transform.position = slotPos;
                blockObj.transform.localScale = Vector3.one * 0.55f;

                Vector2 centerOffset = shape.GetCenterOffset();
                foreach (var offset in shape.offsets)
                {
                    GameObject pObj = new GameObject("Piece");
                    pObj.transform.SetParent(blockObj.transform, false);
                    float localX = (offset.x - centerOffset.x) * step;
                    float localY = (offset.y - centerOffset.y) * step;
                    pObj.transform.localPosition = new Vector3(localX, localY, 0f);
                    pObj.transform.localScale = Vector3.one * renderSize;

                    SpriteRenderer pSr = pObj.AddComponent<SpriteRenderer>();
                    pSr.sprite = ProceduralSpriteGenerator.GetGlossyBlockSprite(128, config.cornerRadiusRatio, config.highlightIntensity, config.shadowIntensity);
                    pSr.color = color;
                    pSr.sortingOrder = 10;
                }
            }

            // Set DontSave on root and all children
            SetDontSaveRecursive(root);

            Debug.Log("[BoardEditorPreview] Edit-mode preview refreshed!");
        }

        [MenuItem("Tools/Block Blast/Clear Preview")]
        public static void ClearPreview()
        {
            GameObject existing = GameObject.Find(PREVIEW_ROOT_NAME);
            while (existing != null)
            {
                Object.DestroyImmediate(existing);
                existing = GameObject.Find(PREVIEW_ROOT_NAME);
            }
        }

        private static void SetDontSaveRecursive(GameObject obj)
        {
            obj.hideFlags = HideFlags.DontSave;
            foreach (Transform child in obj.transform)
            {
                SetDontSaveRecursive(child.gameObject);
            }
        }
    }
}
