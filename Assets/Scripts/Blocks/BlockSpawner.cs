using BlockBlast.Core;
using BlockBlast.Game;
using System.Collections.Generic;
using UnityEngine;

namespace BlockBlast.Blocks
{
    public class BlockSpawner : MonoBehaviour
    {
        public static BlockSpawner Instance { get; private set; }

        [SerializeField] private GameConfig config;

        // Legacy list: older editor tools still read and write it.
        // Blocks are NOT spawned from prefabs any more, they are built from BlockShapeLibrary.
        [SerializeField] private List<GameObject> blockPrefabs = new List<GameObject>();

        public List<GameObject> BlockPrefabs => blockPrefabs;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetBlockPrefabs(List<GameObject> prefabs)
        {
            blockPrefabs = new List<GameObject>(prefabs);
        }

        // Name kept so BlockTray does not need to change.
        // Builds a random block from the shape library at runtime.
        public BlockController SpawnBlockPrefab(Transform parent, GameConfig cfg = null)
        {
            if (cfg != null) config = cfg;
            if (config == null) config = GameConfig.Default;

            // Shape chunein. Agar is level mein allowed na ho to dobara chunein (zyada se zyada 50 baar)
            BlockShapeData shape = BlockShapeLibrary.GetRandomShape();

            if (LevelManager.Instance != null)
            {
                int tries = 0;
                while (!LevelManager.Instance.IsShapeAllowed(shape) && tries < 50)
                {
                    shape = BlockShapeLibrary.GetRandomShape();
                    tries++;
                }

                if (tries >= 50)
                {
                    Debug.LogWarning("[BlockSpawner] 50 koshishon ke baad bhi allowed shape nahi mili. " +
                                     "Level ki Max Cells Per Block value bohat kam to nahi?");
                }
            }

            Color color = config.blockColors[Random.Range(0, config.blockColors.Length)];

            GameObject blockObj = new GameObject($"Block_{shape.shapeName}");
            blockObj.transform.SetParent(parent, false);

            BlockController controller = blockObj.AddComponent<BlockController>();
            controller.Initialize(shape, color, config);

            AddDragCollider(blockObj, shape, config);

            return controller;
        }

        // One collider on the block root so it can be picked up by touch/mouse.
        private static void AddDragCollider(GameObject blockObj, BlockShapeData shape, GameConfig cfg)
        {
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (var c in shape.offsets)
            {
                if (c.x < minX) minX = c.x;
                if (c.x > maxX) maxX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.y > maxY) maxY = c.y;
            }

            float step = cfg.cellSize + cfg.cellSpacing;
            float renderSize = cfg.cellSize * cfg.cellGapRatio;

            // Extra padding makes small blocks easier to grab in the tray
            float width = (maxX - minX) * step + renderSize + 0.6f;
            float height = (maxY - minY) * step + renderSize + 0.6f;

            BoxCollider2D col = blockObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, height);
            col.offset = Vector2.zero;
        }

        public List<GameObject> PickRandomPrefabs(int count)
        {
            List<GameObject> result = new List<GameObject>();
            if (blockPrefabs == null || blockPrefabs.Count == 0) return result;

            for (int i = 0; i < count; i++)
            {
                result.Add(blockPrefabs[Random.Range(0, blockPrefabs.Count)]);
            }

            return result;
        }
    }
}