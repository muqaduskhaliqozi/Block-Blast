using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using BlockBlast.Board;
using BlockBlast.Core;

namespace BlockBlast.EditorTools
{
    public static class BoardSceneBuilderTool
    {
        [MenuItem("Tools/Block Blast/Build Board In Scene")]
        public static void BuildBoardInScene()
        {
            BoardManager boardMgr = Object.FindObjectOfType<BoardManager>();
            if (boardMgr == null)
            {
                Debug.LogError("[BoardSceneBuilderTool] BoardManager GameObject not found in active scene!");
                return;
            }

            GameConfig config = GameConfig.Default;
            GameObject cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BoardCell.prefab");
            if (cellPrefab == null)
            {
                PrefabGeneratorTool.GenerateAllPrefabs();
                cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BoardCell.prefab");
            }

            if (cellPrefab == null)
            {
                Debug.LogError("[BoardSceneBuilderTool] Assets/Prefabs/BoardCell.prefab could not be found or generated.");
                return;
            }

            // Find or create BoardCellsContainer
            Transform container = boardMgr.transform.Find("BoardCellsContainer");
            if (container != null)
            {
                Undo.DestroyObjectImmediate(container.gameObject);
            }

            GameObject containerObj = new GameObject("BoardCellsContainer");
            Undo.RegisterCreatedObjectUndo(containerObj, "Build Board In Scene");
            containerObj.transform.SetParent(boardMgr.transform, false);

            int width = config.boardWidth;
            int height = config.boardHeight;
            float step = config.cellSize + config.cellSpacing;
            float totalWidth = width * step - config.cellSpacing;
            float totalHeight = height * step - config.cellSpacing;
            Vector3 originWorld = boardMgr.transform.position - new Vector3(totalWidth * 0.5f - config.cellSize * 0.5f, totalHeight * 0.5f - config.cellSize * 0.5f, 0f);

            float renderSize = config.cellSize * config.cellGapRatio;

            int built = 0;
            bool failed = false;

            for (int y = 0; y < height && !failed; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    try
                    {
                        Vector3 cellPos = originWorld + new Vector3(x * step, y * step, 0f);

                        GameObject cellInstance = PrefabUtility.InstantiatePrefab(cellPrefab, containerObj.transform) as GameObject;
                        if (cellInstance == null)
                        {
                            Debug.LogError($"[BoardSceneBuilderTool] Could not instantiate the cell prefab at ({x},{y}).");
                            failed = true;
                            break;
                        }

                        cellInstance.name = $"Cell_{x}_{y}";
                        cellInstance.transform.position = cellPos;
                        cellInstance.transform.localScale = Vector3.one * renderSize;

                        BoardCell cellComp = cellInstance.GetComponent<BoardCell>();
                        if (cellComp == null)
                        {
                            Debug.LogError($"[BoardSceneBuilderTool] The cell prefab has no BoardCell component (cell {x},{y}).");
                            failed = true;
                            break;
                        }

                        cellComp.Initialize(new BoardCoordinate(x, y), config);
                        built++;
                    }
                    catch (System.Exception e)
                    {
                        // Stop at the first failure and show exactly what went wrong
                        Debug.LogError($"[BoardSceneBuilderTool] Failed on cell ({x},{y}): {e}");
                        failed = true;
                        break;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(boardMgr.gameObject.scene);

            if (failed)
            {
                Debug.LogError($"[BoardSceneBuilderTool] Board build stopped. Built {built} of {width * height} cells.");
            }
            else
            {
                Debug.Log($"[BoardSceneBuilderTool] Successfully built {built} of {width * height} cells ({width}x{height}) using BoardCell prefab. Save the scene (Ctrl+S).");
            }
        }
    }
}