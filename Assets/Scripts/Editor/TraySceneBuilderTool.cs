using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using BlockBlast.Blocks;
using BlockBlast.Core;

namespace BlockBlast.EditorTools
{
    // Put this file in Assets/Scripts/Editor
    public static class TraySceneBuilderTool
    {
        private const float SlotSpacing = 2.7f;
        private const float SlotSize = 2.5f;

        [MenuItem("Tools/Block Blast/Build Tray In Scene")]
        public static void BuildTrayInScene()
        {
            BlockTray tray = Object.FindObjectOfType<BlockTray>();
            if (tray == null)
            {
                Debug.LogError("[TraySceneBuilderTool] BlockTray not found in the active scene.");
                return;
            }

            GameConfig config = GameConfig.Default;
            int count = Mathf.Max(1, config.traySlotCount);

            // 1. Remove old slots so the tool can be run again safely
            for (int i = tray.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = tray.transform.GetChild(i);
                if (child.name.StartsWith("Slot_"))
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            // 2. Create the slots (BlockSlot builds its own SlotShadow and SlotFrame)
            List<BlockSlot> createdSlots = new List<BlockSlot>();
            for (int i = 0; i < count; i++)
            {
                GameObject slotObj = new GameObject($"Slot_{i}");
                Undo.RegisterCreatedObjectUndo(slotObj, "Build Tray In Scene");
                slotObj.transform.SetParent(tray.transform, false);
                slotObj.transform.localPosition = new Vector3((i - (count - 1) * 0.5f) * SlotSpacing, 0f, 0f);

                BlockSlot slot = slotObj.AddComponent<BlockSlot>();
                slot.Initialize(i, config, SlotSize, SlotSize);
                createdSlots.Add(slot);
            }

            // 3. Link the slots to BlockTray
            SerializedObject trayObject = new SerializedObject(tray);
            SerializedProperty slotsProp = trayObject.FindProperty("slots");
            slotsProp.arraySize = createdSlots.Count;
            for (int i = 0; i < createdSlots.Count; i++)
            {
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = createdSlots[i];
            }
            trayObject.ApplyModifiedProperties();

            // 4. Make sure a BlockSpawner exists (blocks are built from BlockShapeLibrary, no prefabs needed)
            BlockSpawner spawner = Object.FindObjectOfType<BlockSpawner>();
            if (spawner == null)
            {
                GameObject spawnerObj = new GameObject("BlockSpawner");
                Undo.RegisterCreatedObjectUndo(spawnerObj, "Build Tray In Scene");
                spawnerObj.AddComponent<BlockSpawner>();
            }

            EditorSceneManager.MarkSceneDirty(tray.gameObject.scene);
            Debug.Log($"[TraySceneBuilderTool] Built {createdSlots.Count} tray slots. Save the scene (Ctrl+S).");
        }
    }
}