using System;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Core;

namespace BlockBlast.Blocks
{
    public class BlockTray : MonoBehaviour
    {
        public static BlockTray Instance { get; private set; }

        [SerializeField] private GameConfig config;
        [SerializeField] private List<BlockSlot> slots = new List<BlockSlot>();

        [Header("Slot Layout")]
        [SerializeField] private float slotSpacing = 2.7f;
        [SerializeField] private float slotWidth = 2.5f;
        [SerializeField] private float slotHeight = 2.5f;

        public event Action OnTrayRefreshed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeTray(GameConfig cfg = null)
        {
            if (cfg != null) config = cfg;
            if (config == null) config = GameConfig.Default;

            EnsureSlots();

            // Clear any edit-mode preview block objects inside slots
            foreach (var slot in slots)
            {
                if (slot != null && slot.transform.childCount > 0)
                {
                    for (int i = slot.transform.childCount - 1; i >= 0; i--)
                    {
                        Transform child = slot.transform.GetChild(i);
                        // Skip SlotFrame or SlotShadow renderers
                        if (child.name.StartsWith("SlotFrame") || child.name.StartsWith("SlotShadow")) continue;

                        if (Application.isPlaying) Destroy(child.gameObject);
                        else DestroyImmediate(child.gameObject);
                    }
                    slot.ClearBlock();
                }
            }

            RefillTray();
        }

        // Finds the slots, creates them if the scene has none, then lays them out
        // and builds their panels (the panel sprites are procedural, so this runs every time).
        private void EnsureSlots()
        {
            slots.RemoveAll(s => s == null);

            if (slots.Count == 0)
            {
                foreach (Transform child in transform)
                {
                    BlockSlot slot = child.GetComponent<BlockSlot>();
                    if (slot != null) slots.Add(slot);
                }
            }

            if (slots.Count == 0)
            {
                int count = Mathf.Max(1, config.traySlotCount);
                for (int i = 0; i < count; i++)
                {
                    GameObject slotObj = new GameObject($"Slot_{i}");
                    slotObj.transform.SetParent(transform, false);
                    slots.Add(slotObj.AddComponent<BlockSlot>());
                }
            }

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].transform.localPosition = new Vector3((i - (slots.Count - 1) * 0.5f) * slotSpacing, 0f, 0f);
                slots[i].Initialize(i, config, slotWidth, slotHeight);
            }
        }

        public void RefillTray()
        {
            // Create a spawner on the fly if the scene does not have one
            if (BlockSpawner.Instance == null)
            {
                new GameObject("BlockSpawner").AddComponent<BlockSpawner>();
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                {
                    BlockController controller = BlockSpawner.Instance.SpawnBlockPrefab(slots[i].transform, config);
                    if (controller != null)
                    {
                        slots[i].SetBlock(controller);
                    }
                }
            }

            OnTrayRefreshed?.Invoke();
        }

        public void CheckAndRefillIfEmpty()
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty) return;
            }

            RefillTray();
        }

        public List<BlockController> GetAvailableBlocks()
        {
            List<BlockController> available = new List<BlockController>();
            foreach (var slot in slots)
            {
                if (slot != null && !slot.IsEmpty && slot.CurrentBlock != null)
                {
                    available.Add(slot.CurrentBlock);
                }
            }
            return available;
        }

        public void ClearTray()
        {
            foreach (var slot in slots)
            {
                if (slot != null && slot.CurrentBlock != null)
                {
                    if (Application.isPlaying) Destroy(slot.CurrentBlock.gameObject);
                    else DestroyImmediate(slot.CurrentBlock.gameObject);
                    slot.ClearBlock();
                }
            }
        }
    }
}