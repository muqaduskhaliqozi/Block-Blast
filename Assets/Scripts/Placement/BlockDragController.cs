using BlockBlast.Blocks;
using BlockBlast.Board;
using BlockBlast.Core;
using System;
using UnityEngine;

namespace BlockBlast.Placement
{
    public class BlockDragController : MonoBehaviour
    {
        public static BlockDragController Instance { get; private set; }

        [SerializeField] private GameConfig config;
        [SerializeField] private UnityEngine.Camera mainCamera;

        private BlockController draggingBlock;
        private Vector3 dragOffset;
        private bool isDragging = false;
        private BoardCoordinate currentHoverOrigin;
        private bool isCurrentHoverValid = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
            }

            if (config == null)
            {
                config = GameConfig.Default;
            }
        }

        private void Update()
        {
            if (GameStateManager.Instance != null &&
                !GameStateManager.Instance.CanDrag() &&
                !isDragging)
            {
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            // Support both Mouse and Touch
            bool pointerDown =
                Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 &&
                 Input.GetTouch(0).phase == TouchPhase.Began);

            bool pointerHeld =
                Input.GetMouseButton(0) ||
                (Input.touchCount > 0 &&
                 (Input.GetTouch(0).phase == TouchPhase.Moved ||
                  Input.GetTouch(0).phase == TouchPhase.Stationary));

            bool pointerUp =
                Input.GetMouseButtonUp(0) ||
                (Input.touchCount > 0 &&
                 (Input.GetTouch(0).phase == TouchPhase.Ended ||
                  Input.GetTouch(0).phase == TouchPhase.Canceled));

            // Pointer screen ke bahar ho (inf / NaN) to is frame ko ignore karein
            if (!TryGetPointerWorldPosition(out Vector3 pointerWorldPos))
            {
                // Agar drag chal raha tha aur pointer bahar chala gaya, to drag khatam kar dein
                if (isDragging && (pointerUp || !pointerHeld))
                {
                    EndDragging();
                }
                return;
            }

            if (pointerDown && !isDragging)
            {
                TryPickUpBlock(pointerWorldPos);
            }
            else if (pointerHeld && isDragging)
            {
                UpdateDragging(pointerWorldPos);
            }
            else if (pointerUp && isDragging)
            {
                EndDragging();
            }
        }

        private bool TryGetPointerWorldPosition(out Vector3 world)
        {
            world = Vector3.zero;

            Vector3 screenPos = Input.mousePosition;

            if (Input.touchCount > 0)
            {
                screenPos = Input.GetTouch(0).position;
            }

            // inf / NaN ho to ignore
            if (float.IsNaN(screenPos.x) || float.IsNaN(screenPos.y) ||
                float.IsInfinity(screenPos.x) || float.IsInfinity(screenPos.y))
            {
                return false;
            }

            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
            }

            if (mainCamera == null)
            {
                return false;
            }

            screenPos.z = -mainCamera.transform.position.z;
            world = mainCamera.ScreenToWorldPoint(screenPos);
            return true;
        }

        private void TryPickUpBlock(Vector3 pointerWorldPos)
        {
            Collider2D hitCollider =
                Physics2D.OverlapPoint(pointerWorldPos);

            BlockController block = null;

            if (hitCollider != null)
            {
                block = hitCollider.GetComponentInParent<BlockController>();
            }

            if (block == null)
            {
                // Try distance check to tray blocks if overlap point misses
                if (BlockTray.Instance != null)
                {
                    foreach (var b in BlockTray.Instance.GetAvailableBlocks())
                    {
                        if (b == null)
                            continue;

                        if (Vector3.Distance(
                                b.transform.position,
                                pointerWorldPos) < 1.5f)
                        {
                            block = b;
                            break;
                        }
                    }
                }
            }

            if (block != null && block.Slot != null)
            {
                draggingBlock = block;
                isDragging = true;

                dragOffset =
                    block.transform.position - pointerWorldPos;

                // Stop any tray pop / return animation; the block grows smoothly to board size in UpdateDragging
                draggingBlock.CancelAnimations();

                draggingBlock.SetSortingOrder(50);

                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(
                        GameState.Dragging);
                }

                if (Audio.AudioManager.Instance != null)
                {
                    Audio.AudioManager.Instance.PlayPickupSound();
                }
            }
        }

        private void UpdateDragging(Vector3 pointerWorldPos)
        {
            if (draggingBlock == null)
                return;

            // Apply touch offset Y so block is visible above user's finger/cursor
            Vector3 targetPos =
                pointerWorldPos +
                new Vector3(
                    0f,
                    config.dragTouchOffsetY,
                    0f);

            draggingBlock.transform.position =
                Vector3.Lerp(
                    draggingBlock.transform.position,
                    targetPos,
                    Time.deltaTime * config.lerpSpeed);

            draggingBlock.transform.localScale =
                Vector3.Lerp(
                    draggingBlock.transform.localScale,
                    Vector3.one * config.dragBlockScale,
                    Time.deltaTime * 18f);

            // Calculate grid alignment preview
            BoardManager board = BoardManager.Instance;

            if (board != null)
            {
                // Align using first piece of the shape
                Vector3 pieceWorldPos =
                    draggingBlock.GetWorldPositionOfPiece(
                        draggingBlock.ShapeData.offsets[0]);

                if (board.WorldToCellCoordinate(
                        pieceWorldPos,
                        out BoardCoordinate firstPieceCoord))
                {
                    BoardCoordinate originCoord =
                        firstPieceCoord -
                        draggingBlock.ShapeData.offsets[0];

                    bool isValid =
                        PlacementValidator.CanPlaceBlock(
                            board,
                            draggingBlock.ShapeData,
                            originCoord);

                    currentHoverOrigin = originCoord;
                    isCurrentHoverValid = isValid;

                    if (BlockPlacementPreview.Instance != null)
                    {
                        BlockPlacementPreview.Instance.UpdatePreview(
                            board,
                            draggingBlock,
                            originCoord,
                            isValid);
                    }
                }
                else
                {
                    isCurrentHoverValid = false;

                    if (BlockPlacementPreview.Instance != null)
                    {
                        BlockPlacementPreview.Instance.ClearPreview(board);
                    }
                }
            }
        }

        private void EndDragging()
        {
            if (draggingBlock == null)
                return;

            BoardManager board = BoardManager.Instance;

            if (BlockPlacementPreview.Instance != null)
            {
                BlockPlacementPreview.Instance.ClearPreview(board);
            }

            if (isCurrentHoverValid &&
                board != null &&
                BlockPlacementSystem.Instance != null)
            {
                BlockPlacementSystem.Instance.ExecutePlacement(
                    draggingBlock,
                    currentHoverOrigin);
            }
            else
            {
                draggingBlock.ReturnToSlot();

                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.ChangeState(
                        GameState.Playing);
                }
            }

            draggingBlock = null;
            isDragging = false;
            isCurrentHoverValid = false;
        }
    }
}
