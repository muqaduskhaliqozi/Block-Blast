using UnityEngine;
using UnityEditor;

namespace BlockBlast.EditorTools
{
    public static class BuildAllAssetsAndScene
    {
        [MenuItem("Tools/Block Blast/Build Everything (Assets + Prefabs + Scene)")]
        public static void Execute()
        {
            Debug.Log("[BuildAllAssetsAndScene] Starting full asset & scene build...");

            // 1. Generate PNG Sprites in Assets/Art/Sprites/
            SpriteAssetGenerator.GenerateAllSpriteAssets();

            // 2. Generate Prefabs in Assets/Prefabs/ & Assets/Prefabs/Blocks/
            PrefabGeneratorTool.GenerateAllPrefabs();

            // 3. Build & Save Gameplay Scene Hierarchy
            BlockBlastSceneGenerator.GenerateScene();

            // 4. Build Board Cell Prefab Instances in Scene
            BoardSceneBuilderTool.BuildBoardInScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[BuildAllAssetsAndScene] Complete build finished successfully!");
        }
    }
}
