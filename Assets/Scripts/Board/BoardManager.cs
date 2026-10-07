using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlockBlast.Core;
using BlockBlast.Visuals;

namespace BlockBlast.Board
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        [SerializeField] private GameConfig config;
        [SerializeField] private Transform cellsParent;
        [SerializeField] private SpriteRenderer boardBackground;
        [SerializeField] private SpriteRenderer boardShadow;
        [SerializeField] private SpriteRenderer backgroundGradient;
        [SerializeField] private List<SpriteRenderer> backgroundDecorations = new List<SpriteRenderer>();

        private BoardCell[,] grid;
        private int width = 8;
        private int height = 8;
        private Vector3 boardOriginWorld;

        public int Width => width;
        public int Height => height;

        public event Action<int> OnLinesCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeBoard();
        }

        public void InitializeBoard(GameConfig cfg = null)
        {
            if (cfg != null) config = cfg;
            if (config == null) config = GameConfig.Default;

            width = config.boardWidth;
            height = config.boardHeight;
            var lm = BlockBlast.Game.LevelManager.Instance;
            if (lm != null && lm.CurrentLevel != null)
            {
                width = lm.CurrentLevel.boardWidth;
                height = lm.CurrentLevel.boardHeight;
            }

            SetupBackgroundGradientAndDeco();

            float step = config.cellSize + config.cellSpacing;
            float totalWidth = width * step - config.cellSpacing;
            float totalHeight = height * step - config.cellSpacing;

            boardOriginWorld = transform.position - new Vector3(totalWidth * 0.5f - config.cellSize * 0.5f, totalHeight * 0.5f - config.cellSize * 0.5f, 0f);

            SetupBoardBackground(totalWidth, totalHeight);

            grid = new BoardCell[width, height];

            // Uses the cells built in the scene, or builds them now if they are missing
            EnsureCells();

            float renderSize = config.cellSize * config.cellGapRatio;

            // Cells are mapped by their order inside the container (row by row).
            // Unity does not save the Coordinate property, so it must not be read back from the cell.
            int loadedCount = 0;
            int index = 0;
            foreach (Transform child in cellsParent)
            {
                BoardCell cell = child.GetComponent<BoardCell>();
                if (cell != null)
                {
                    int x = index % width;
                    int y = index / width;
                    if (y >= height) break;

                    BoardCoordinate coord = new BoardCoordinate(x, y);

                    child.position = CellToWorldPosition(coord);
                    child.localScale = Vector3.one * renderSize;

                    cell.Initialize(coord, config);
                    grid[x, y] = cell;
                    loadedCount++;
                }
                index++;
            }

            Debug.Log($"[BoardManager] Mapped {loadedCount} board cells ({width}x{height}).");
        }

        // Makes sure BoardCellsContainer exists and holds exactly width * height cells.
        private void EnsureCells()
        {
            if (cellsParent == null)
            {
                cellsParent = transform.Find("BoardCellsContainer");
            }

            if (cellsParent != null && cellsParent.childCount == width * height)
            {
                return;
            }

            if (cellsParent != null)
            {
                Debug.LogWarning($"[BoardManager] Container has {cellsParent.childCount} cells, expected {width * height}. Rebuilding the cells now.");

                cellsParent.gameObject.name = "BoardCellsContainer_Old";
                cellsParent.gameObject.SetActive(false);

                if (Application.isPlaying) Destroy(cellsParent.gameObject);
                else DestroyImmediate(cellsParent.gameObject);
            }

            GameObject container = new GameObject("BoardCellsContainer");
            container.transform.SetParent(transform, false);
            cellsParent = container.transform;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    GameObject cellObj = new GameObject($"Cell_{x}_{y}");
                    cellObj.transform.SetParent(cellsParent, false);
                    cellObj.AddComponent<BoardCell>();
                }
            }
        }

        private void SetupBackgroundGradientAndDeco()
        {
            if (backgroundGradient == null)
            {
                Transform t = transform.Find("BackgroundGradient");
                if (t != null) backgroundGradient = t.GetComponent<SpriteRenderer>();
            }

            if (backgroundGradient != null)
            {
                backgroundGradient.sprite = ProceduralSpriteGenerator.GetVerticalGradientSprite(config.bgGradientTop, config.bgGradientBottom);
            }

            if (backgroundDecorations.Count == 0)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.StartsWith("DecoCircle_"))
                    {
                        SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                        if (sr != null) backgroundDecorations.Add(sr);
                    }
                }
            }
        }

        private void SetupBoardBackground(float totalWidth, float totalHeight)
        {
            if (boardShadow == null)
            {
                Transform t = transform.Find("BoardShadow");
                if (t != null) boardShadow = t.GetComponent<SpriteRenderer>();
            }
            if (boardShadow != null)
            {
                boardShadow.sprite = ProceduralSpriteGenerator.GetSoftShadowSprite();
                boardShadow.color = config.shadowColor;
            }

            if (boardBackground == null)
            {
                Transform t = transform.Find("BoardFrame");
                if (t != null) boardBackground = t.GetComponent<SpriteRenderer>();
            }
            if (boardBackground != null)
            {
                boardBackground.sprite = ProceduralSpriteGenerator.GetRoundedSquareSprite(256, 0.08f);
                boardBackground.color = config.boardBackgroundColor;
            }
            if (boardBackground != null && boardBackground.sprite != null)
            {
                Vector2 sz = boardBackground.sprite.bounds.size;
                boardBackground.transform.position = transform.position;
                boardBackground.transform.localScale = new Vector3((totalWidth + 0.5f) / sz.x, (totalHeight + 0.5f) / sz.y, 1f);
            }

            if (boardShadow != null && boardShadow.sprite != null)
            {
                Vector2 sz = boardShadow.sprite.bounds.size;
                boardShadow.transform.position = transform.position;
                boardShadow.transform.localScale = new Vector3((totalWidth + 1.2f) / sz.x, (totalHeight + 1.2f) / sz.y, 1f);
            }
        }

        public Vector3 CellToWorldPosition(BoardCoordinate coord)
        {
            float step = config.cellSize + config.cellSpacing;
            return boardOriginWorld + new Vector3(coord.x * step, coord.y * step, 0f);
        }

        public bool WorldToCellCoordinate(Vector3 worldPos, out BoardCoordinate coord)
        {
            coord = new BoardCoordinate(0, 0);
            float step = config.cellSize + config.cellSpacing;
            Vector3 diff = worldPos - boardOriginWorld;

            int x = Mathf.RoundToInt(diff.x / step);
            int y = Mathf.RoundToInt(diff.y / step);

            if (IsValidCoordinate(x, y))
            {
                coord = new BoardCoordinate(x, y);
                return true;
            }
            return false;
        }

        public bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public bool IsValidCoordinate(BoardCoordinate coord)
        {
            return IsValidCoordinate(coord.x, coord.y);
        }

        public bool IsCellEmpty(BoardCoordinate coord)
        {
            if (!IsValidCoordinate(coord)) return false;
            return grid != null && grid[coord.x, coord.y] != null && !grid[coord.x, coord.y].IsOccupied;
        }

        public BoardCell GetCell(BoardCoordinate coord)
        {
            if (!IsValidCoordinate(coord) || grid == null) return null;
            return grid[coord.x, coord.y];
        }

        public bool CanPlaceShape(List<BoardCoordinate> shapeOffsets, BoardCoordinate originCoord)
        {
            foreach (var offset in shapeOffsets)
            {
                BoardCoordinate target = originCoord + offset;
                if (!IsValidCoordinate(target) || !IsCellEmpty(target))
                {
                    return false;
                }
            }
            return true;
        }

        public void PlaceShape(List<BoardCoordinate> shapeOffsets, BoardCoordinate originCoord, Color color)
        {
            foreach (var offset in shapeOffsets)
            {
                BoardCoordinate target = originCoord + offset;
                if (IsValidCoordinate(target) && grid[target.x, target.y] != null)
                {
                    grid[target.x, target.y].SetOccupied(color);
                }
            }
        }

        public void CheckCompletedLines(out List<int> fullRows, out List<int> fullCols)
        {
            fullRows = new List<int>();
            fullCols = new List<int>();

            if (grid == null) return;

            // Check rows
            for (int y = 0; y < height; y++)
            {
                bool rowFull = true;
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y] == null || !grid[x, y].IsOccupied)
                    {
                        rowFull = false;
                        break;
                    }
                }
                if (rowFull) fullRows.Add(y);
            }

            // Check columns
            for (int x = 0; x < width; x++)
            {
                bool colFull = true;
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] == null || !grid[x, y].IsOccupied)
                    {
                        colFull = false;
                        break;
                    }
                }
                if (colFull) fullCols.Add(x);
            }
        }

        public IEnumerator ClearLinesRoutine(List<int> rows, List<int> cols, Action onComplete)
        {
            HashSet<BoardCell> cellsToClear = new HashSet<BoardCell>();

            foreach (int r in rows)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, r] != null) cellsToClear.Add(grid[x, r]);
                }
            }

            foreach (int c in cols)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[c, y] != null) cellsToClear.Add(grid[c, y]);
                }
            }

            int totalLines = rows.Count + cols.Count;
            if (totalLines > 0)
            {
                OnLinesCleared?.Invoke(totalLines);

                List<Coroutine> animRoutines = new List<Coroutine>();
                foreach (BoardCell cell in cellsToClear)
                {
                    animRoutines.Add(StartCoroutine(cell.AnimateClear(config.lineClearDuration)));
                }

                yield return new WaitForSeconds(config.lineClearDuration);
            }

            onComplete?.Invoke();
        }

        public void ClearAllHighlights()
        {
            if (grid == null) return;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[x, y]?.ClearHighlight();
                }
            }
        }

        public void ResetBoard()
        {
            if (grid == null) return;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[x, y]?.SetEmpty();
                }
            }
        }
    }
}