using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BlockBlast.Board;
using BlockBlast.Blocks;
using BlockBlast.Core;
using BlockBlast.Visuals;

namespace BlockBlast.EditorTools
{
    public static class PrefabGeneratorTool
    {
        private const string PREFAB_REL_DIR = "Assets/Prefabs/";
        private const string BLOCK_PREFAB_REL_DIR = "Assets/Prefabs/Blocks/";

        [MenuItem("Tools/Block Blast/Generate Block Prefabs")]
        public static void GenerateAllPrefabs()
        {
            SpriteAssetGenerator.GenerateAllSpriteAssets();

            string prefabPhysicalDir = Path.Combine(Application.dataPath, "Prefabs");
            string blockPhysicalDir = Path.Combine(Application.dataPath, "Prefabs/Blocks");

            if (!Directory.Exists(prefabPhysicalDir)) Directory.CreateDirectory(prefabPhysicalDir);
            if (!Directory.Exists(blockPhysicalDir)) Directory.CreateDirectory(blockPhysicalDir);

            AssetDatabase.Refresh();

            GameConfig config = GameConfig.Default;
            Sprite roundedSprite = SpriteAssetGenerator.GetSpriteAsset("RoundedSquare");
            Sprite glossySprite = SpriteAssetGenerator.GetSpriteAsset("GlossyTile");
            Sprite borderSprite = SpriteAssetGenerator.GetSpriteAsset("CellBorder");
            Sprite shadowSprite = SpriteAssetGenerator.GetSpriteAsset("SoftShadow");

            // 1. BoardCell Prefab
            GameObject cellObj = new GameObject("BoardCell");
            SpriteRenderer cellSr = cellObj.AddComponent<SpriteRenderer>();
            cellSr.sprite = roundedSprite;
            cellSr.color = config.emptyCellColor;
            cellSr.sortingOrder = 0;

            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(cellObj.transform, false);
            SpriteRenderer borderSr = borderObj.AddComponent<SpriteRenderer>();
            borderSr.sprite = borderSprite;
            borderSr.color = config.emptyCellBorderColor;
            borderSr.sortingOrder = 1;

            GameObject overlayObj = new GameObject("Overlay");
            overlayObj.transform.SetParent(cellObj.transform, false);
            SpriteRenderer overlaySr = overlayObj.AddComponent<SpriteRenderer>();
            overlaySr.sprite = roundedSprite;
            overlaySr.color = Color.clear;
            overlaySr.sortingOrder = 5;

            BoardCell cellComp = cellObj.AddComponent<BoardCell>();

            string boardCellPath = PREFAB_REL_DIR + "BoardCell.prefab";
            PrefabUtility.SaveAsPrefabAsset(cellObj, boardCellPath);
            Object.DestroyImmediate(cellObj);

            // 2. BlockPiece Prefab
            GameObject pieceObj = new GameObject("BlockPiece");
            SpriteRenderer pieceSr = pieceObj.AddComponent<SpriteRenderer>();
            pieceSr.sprite = glossySprite;
            pieceSr.color = Color.white;
            pieceSr.sortingOrder = 10;

            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(pieceObj.transform, false);
            shadowObj.transform.localPosition = config.shadowOffset;
            shadowObj.transform.localScale = Vector3.one * 1.05f;
            SpriteRenderer shadowSr = shadowObj.AddComponent<SpriteRenderer>();
            shadowSr.sprite = shadowSprite;
            shadowSr.color = config.shadowColor;
            shadowSr.sortingOrder = 8;

            BlockPiece pieceComp = pieceObj.AddComponent<BlockPiece>();

            string blockPiecePath = PREFAB_REL_DIR + "BlockPiece.prefab";
            PrefabUtility.SaveAsPrefabAsset(pieceObj, blockPiecePath);
            Object.DestroyImmediate(pieceObj);

            // 3. Block Shape Prefabs (Assets/Prefabs/Blocks/*.prefab)
            var shapes = BlockShapeLibrary.GetAllShapes();
            float step = config.cellSize + config.cellSpacing;
            float renderSize = config.cellSize * config.cellGapRatio;

            for (int i = 0; i < shapes.Count; i++)
            {
                BlockShapeData shape = shapes[i];
                Color color = config.blockColors[i % config.blockColors.Length];

                GameObject blockObj = new GameObject($"Block_{shape.shapeName}");
                BlockController controller = blockObj.AddComponent<BlockController>();

                GameObject piecesParent = new GameObject("PiecesParent");
                piecesParent.transform.SetParent(blockObj.transform, false);

                Vector2 centerOffset = shape.GetCenterOffset();

                foreach (var offset in shape.offsets)
                {
                    GameObject pObj = new GameObject($"Piece_{offset.x}_{offset.y}");
                    pObj.transform.SetParent(piecesParent.transform, false);

                    float localX = (offset.x - centerOffset.x) * step;
                    float localY = (offset.y - centerOffset.y) * step;
                    pObj.transform.localPosition = new Vector3(localX, localY, 0f);
                    pObj.transform.localScale = Vector3.one * renderSize;

                    SpriteRenderer pSr = pObj.AddComponent<SpriteRenderer>();
                    pSr.sprite = glossySprite;
                    pSr.color = color;
                    pSr.sortingOrder = 10;

                    GameObject pShadow = new GameObject("Shadow");
                    pShadow.transform.SetParent(pObj.transform, false);
                    pShadow.transform.localPosition = config.shadowOffset;
                    pShadow.transform.localScale = Vector3.one * 1.05f;
                    SpriteRenderer pShadowSr = pShadow.AddComponent<SpriteRenderer>();
                    pShadowSr.sprite = shadowSprite;
                    pShadowSr.color = config.shadowColor;
                    pShadowSr.sortingOrder = 8;

                    BlockPiece bp = pObj.AddComponent<BlockPiece>();
                }

                string shapePrefabPath = BLOCK_PREFAB_REL_DIR + $"Block_{shape.shapeName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(blockObj, shapePrefabPath);
                Object.DestroyImmediate(blockObj);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PrefabGeneratorTool] Saved BoardCell, BlockPiece, and {shapes.Count} Block prefabs to {BLOCK_PREFAB_REL_DIR}");
        }

        public static List<GameObject> LoadAllBlockPrefabs()
        {
            List<GameObject> list = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("Block_ t:Prefab", new[] { BLOCK_PREFAB_REL_DIR });
            foreach (string g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) list.Add(prefab);
            }
            return list;
        }
    }
}
