using System.Collections.Generic;
using GraviTrix.Runtime;
using GraviTrix.UI;
using UnityEngine;

namespace GraviTrix.Core
{
    public sealed class GameController : MonoBehaviour
    {
        private const int FixedBoardSize = 12;
        private const float MinFallInterval = 0.05f;
        private static readonly Vector2Int DownVector = new Vector2Int(0, 1);

        [Header("Data")]
        [SerializeField] internal GameConfig config;

        [Header("Views")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private GameHudView hudView;
        [SerializeField] private GameOverPopup gameOverPopup;

        // Members marked internal are also set or called by the EditMode tests (see Assets/Scripts/AssemblyInfo.cs).
        internal BoardGrid board;
        internal PieceInstance activePiece;
        private PieceInstance nextPiece;
        internal PieceInstance heldPiece;
        private bool canHold = true;
        private GamePhase phase = GamePhase.Spawning;
        private float fallAccumulator;
        private float phaseTimer;
        private int score;
        internal int movesUntilRotation;
        private int movesBeforeRotationTarget;
        internal bool isBoardRotated;
        private int comboCount;
        internal int spawnedPiecesCount;
        private int movesUntilNextMetal;

        private List<int> pendingLineRowsToClear = new List<int>();
        private HashSet<Vector2Int> hiddenCells = new HashSet<Vector2Int>();
        internal bool pendingIsRotation;
        internal int rescueCooldown;
        private bool batchMeltEffects;
        private bool isNewRecord;
        private readonly List<BlockCellInfo> batchedMeltCells = new List<BlockCellInfo>();

        public int Score => score;
        public GamePhase Phase => phase;

        public PieceInstance ActivePiece => activePiece;
        public PieceInstance NextPiece => nextPiece;
        public PieceInstance HeldPiece => heldPiece;

        internal void Awake()
        {
            board = new BoardGrid(FixedBoardSize, FixedBoardSize);
        }

        private void Start()
        {
            RestartGame();
        }

        private void Update()
        {
            if (phase == GamePhase.GameOver || config == null)
            {
                return;
            }

            if (phase == GamePhase.Spawning && activePiece == null)
            {
                SpawnNextPiece();
                UpdateHud();
                return;
            }

            if (phase == GamePhase.RotatingBoard)
            {
                UpdateHud();
                return;
            }

            if (phase == GamePhase.ClearingLines)
            {
                phaseTimer -= Time.deltaTime;
                if (phaseTimer <= 0f)
                {
                    ApplyPendingClears();
                }
                UpdateHud();
                return;
            }

            if (phase != GamePhase.Falling || activePiece == null)
            {
                UpdateHud();
                return;
            }

            fallAccumulator += Time.deltaTime;
            float fallInterval = Mathf.Max(MinFallInterval, activePiece.GetFallInterval(config));

            while (fallAccumulator >= fallInterval && phase == GamePhase.Falling)
            {
                fallAccumulator -= fallInterval;
                if (!TryAdvancePiece(DownVector))
                {
                    LockActivePiece();
                    break;
                }
            }

            UpdateHud();
        }

        public void RestartGame()
        {
            // Stop the old game's animations first, so their completion callbacks
            // (rotation -> drop -> slide -> spawn, game over -> popup) never reach the new game.
            if (boardView != null) boardView.ResetView();
            if (hudView != null) hudView.ResetView();
            Time.timeScale = 1f;
            if (gameOverPopup != null) gameOverPopup.Hide();
            board.Clear();
            score = 0;
            activePiece = null;
            nextPiece = null;
            heldPiece = null;
            canHold = true;
            phase = GamePhase.Spawning;
            fallAccumulator = 0f;
            isBoardRotated = false;
            comboCount = 0;
            spawnedPiecesCount = 0;
            movesUntilNextMetal = UnityEngine.Random.Range(15, 21);
            pendingLineRowsToClear.Clear();
            hiddenCells.Clear();
            rescueCooldown = 0;
            if (config == null)
            {
                phase = GamePhase.GameOver;
                movesBeforeRotationTarget = 0;
                movesUntilRotation = 0;
                UpdateHud();
                if (boardView != null) boardView.Render(board, activePiece, nextPiece, heldPiece, hiddenCells);
                return;
            }

            movesBeforeRotationTarget = config.GetRandomMovesBeforeRotation();
            movesUntilRotation = movesBeforeRotationTarget;

            SpawnNextPiece();
            UpdateHud();
        }

        public void MoveLeft()
        {
            if (CanControlActivePiece())
            {
                TryAdvancePiece(Vector2Int.left);
            }
        }

        public void MoveRight()
        {
            if (CanControlActivePiece())
            {
                TryAdvancePiece(Vector2Int.right);
            }
        }

        public void RotatePieceLeft()
        {
            if (CanControlActivePiece())
            {
                if (activePiece.TryRotateLeft(board)) RefreshViews();
            }
        }

        public void RotatePieceRight()
        {
            if (CanControlActivePiece())
            {
                if (activePiece.TryRotateRight(board)) RefreshViews();
            }
        }

        public void SoftDrop()
        {
            if (CanControlActivePiece())
            {
                if (!TryAdvancePiece(DownVector))
                {
                    LockActivePiece();
                }
            }
        }

        public void HardDrop()
        {
            if (!CanControlActivePiece())
            {
                return;
            }

            // Collect lava melts over the whole drop so they play as one effect. The melt sound is
            // left to LockActivePiece, which always plays it for a lava piece, so it isn't doubled.
            batchMeltEffects = true;
            // No render per row: every path out of LockActivePiece renders the board once.
            while (TryAdvancePiece(DownVector, render: false))
            {
            }
            batchMeltEffects = false;
            PlayMeltEffects(batchedMeltCells, playSound: false);
            batchedMeltCells.Clear();

            LockActivePiece();
        }

        public void HoldPiece()
        {
            if (!CanControlActivePiece() || !canHold)
            {
                return;
            }

            canHold = false;

            if (heldPiece == null)
            {
                heldPiece = activePiece;
                heldPiece.Reset(GetSpawnOrigin());
                SpawnNextPiece();
                canHold = false; // SpawnNextPiece re-enables hold for a fresh piece; this one came from a hold
            }
            else
            {
                PieceInstance temp = activePiece;
                activePiece = heldPiece;
                heldPiece = temp;
                
                activePiece.Reset(GetSpawnOrigin());
                heldPiece.Reset(GetSpawnOrigin());
                
                if (!board.CanOccupy(activePiece.GetWorldCells()))
                {
                    TriggerGameOver();
                    return;
                }
                else
                {
                    fallAccumulator = 0f;
                }
            }

            UpdateHud();
            RefreshViews();
        }

        private bool CanControlActivePiece()
        {
            return phase == GamePhase.Falling && activePiece != null;
        }

        internal void SpawnNextPiece()
        {
            spawnedPiecesCount++;
            bool activeIsInitial = spawnedPiecesCount <= 4;
            bool nextIsInitial = (spawnedPiecesCount + 1) <= 4;

            if (nextPiece == null)
            {
                nextPiece = FallbackPieceFactory.CreateRandom(GetSpawnOrigin(), activeIsInitial);
            }

            activePiece = nextPiece;
            
            // Rescue Mechanic: Check central 4x4 area
            int centerOccupied = 0;
            int startX = (board.Width / 2) - 2;
            int startY = (board.Height / 2) - 2;
            
            for (int x = startX; x < startX + 4; x++)
            {
                for (int y = startY; y < startY + 4; y++)
                {
                    if (board.IsOccupied(new Vector2Int(x, y)))
                    {
                        centerOccupied++;
                    }
                }
            }

            if (rescueCooldown > 0)
            {
                rescueCooldown--;
            }

            bool isLavaInPlay = (activePiece != null && activePiece.ContainsKind(BlockKind.Lava)) ||
                                (heldPiece != null && heldPiece.ContainsKind(BlockKind.Lava));

            if (centerOccupied >= 6 && rescueCooldown <= 0 && !isLavaInPlay && !nextIsInitial)
            {
                nextPiece = FallbackPieceFactory.CreateLava(GetSpawnOrigin());
                rescueCooldown = 4; // No new rescue lava for the next 4 spawns (any piece, including the one a hold brings in)
            }
            else
            {
                movesUntilNextMetal--;
                if (movesUntilNextMetal <= 0 && !nextIsInitial)
                {
                    nextPiece = FallbackPieceFactory.CreateMetal(GetSpawnOrigin());
                    movesUntilNextMetal = UnityEngine.Random.Range(10, 16);
                }
                else
                {
                    nextPiece = FallbackPieceFactory.CreateRandom(GetSpawnOrigin(), nextIsInitial);
                }
            }

            if (activePiece == null)
            {
                TriggerGameOver();
                return;
            }

            if (!board.CanOccupy(activePiece.GetWorldCells()))
            {
                TriggerGameOver();
                return;
            }

            phase = GamePhase.Falling;
            fallAccumulator = 0f;
            canHold = true;
            RefreshViews();
        }

        private bool TryAdvancePiece(Vector2Int delta, bool render = true)
        {
            if (activePiece == null)
            {
                return false;
            }

            if (delta == DownVector && activePiece.ContainsKind(BlockKind.Lava))
            {
                TryBurnCellsUnderLava();
            }

            bool moved = activePiece.TryMove(board, delta);
            if (moved && render)
            {
                RefreshViews();
            }

            return moved;
        }

        private void TryBurnCellsUnderLava()
        {
            List<Vector2Int> cellsToClear = new List<Vector2Int>();
            List<BlockCellInfo> cellsToAnimate = new List<BlockCellInfo>();

            foreach (BlockCellInfo cell in activePiece.GetWorldCells())
            {
                if (cell.Kind != BlockKind.Lava)
                {
                    continue;
                }

                Vector2Int target = cell.Position + DownVector;
                if (board.IsOccupied(target))
                {
                    cellsToClear.Add(target);
                }
            }

            if (cellsToClear.Count > 0)
            {
                // Capture the cell info before clearing them
                cellsToAnimate = board.GetCells(cellsToClear);
                
                board.ClearCells(cellsToClear);
                score += cellsToClear.Count * config.LavaMeltScore;

                if (batchMeltEffects)
                {
                    batchedMeltCells.AddRange(cellsToAnimate);
                }
                else
                {
                    PlayMeltEffects(cellsToAnimate);
                }
            }
        }

        private void PlayMeltEffects(List<BlockCellInfo> meltedCells, bool playSound = true)
        {
            if (meltedCells.Count == 0)
            {
                return;
            }

            if (boardView != null)
            {
                boardView.PlayMeltAnimation(new List<BlockCellInfo>(meltedCells), 0.5f);
            }

            if (playSound && SfxManager.Instance != null)
            {
                SfxManager.Instance.PlayLavaMelt();
            }
        }

        private List<BlockCellInfo> lastPlacedPieceCells = new List<BlockCellInfo>();

        private void LockActivePiece()
        {
            if (activePiece == null)
            {
                return;
            }

            // Play the appropriate drop SFX based on block kind
            if (SfxManager.Instance != null)
            {
                if (activePiece.ContainsKind(BlockKind.Metal))
                {
                    SfxManager.Instance.PlayMetalBlockDrop();
                }
                else if (activePiece.ContainsKind(BlockKind.Lava))
                {
                    SfxManager.Instance.PlayLavaMelt();
                }
                else
                {
                    SfxManager.Instance.PlayBlockDrop();
                }
            }

            var cells = activePiece.GetWorldCells();
            lastPlacedPieceCells.Clear();
            foreach(var c in cells) lastPlacedPieceCells.Add(c);

            bool placed = board.TryPlace(lastPlacedPieceCells);
            Debug.Assert(placed, "LockActivePiece: the active piece overlaps the board; its cells were not placed.");
            activePiece = null;
            pendingIsRotation = false; // whatever this lock clears or explodes counts as a lock

            if (board.LastExtinguished.Count > 0)
            {
                if (boardView != null)
                {
                    boardView.PlayExtinguishAnimation(board.LastExtinguished, 0.6f);
                }
            }

            CheckLinesAndSettle();
        }

        private void CheckLinesAndSettle()
        {
            if (board.HasAdjacentMetalBlocks())
            {
                TriggerAllClearBomb();
                return;
            }

            List<int> lineRowsToClear = new List<int>();
            HashSet<int> seenRows = new HashSet<int>();

            foreach (BlockCellInfo cell in lastPlacedPieceCells)
            {
                if (cell.Kind == BlockKind.Line && seenRows.Add(cell.Position.y))
                {
                    lineRowsToClear.Add(cell.Position.y);
                }
            }

            List<int> fullRows = board.GetFullRows();
            if (lineRowsToClear.Count > 0 || fullRows.Count > 0)
            {
                pendingLineRowsToClear = new List<int>(lineRowsToClear);
                pendingIsRotation = false;

                HashSet<int> combinedRows = new HashSet<int>(lineRowsToClear);
                foreach (int r in fullRows) combinedRows.Add(r);
                List<int> rowsList = new List<int>(combinedRows);

                List<BlockCellInfo> cellsToAnimate = board.GetAllCellsInRows(rowsList);
                hiddenCells.Clear();
                foreach (var cell in cellsToAnimate) hiddenCells.Add(cell.Position);

                phase = GamePhase.ClearingLines;
                phaseTimer = 0.4f;
                activePiece = null;

                if (SfxManager.Instance != null)
                {
                    SfxManager.Instance.PlayLineClear();
                }

                if (fullRows.Count > 0 && hudView != null)
                {
                    hudView.ShowActionText("GOOD!");
                }

                if (boardView != null)
                {
                    boardView.PlayClearAnimation(cellsToAnimate, 0.4f);
                }
                UpdateHud();
                RefreshViews();
                return;
            }

            comboCount = 0;
            activePiece = null;
            phase = GamePhase.Spawning;

            movesUntilRotation--;
            if (movesUntilRotation <= 0)
            {
                BeginBoardRotation();
                return;
            }

            SpawnNextPiece();
            UpdateHud();
        }

        private void BeginBoardRotation()
        {
            activePiece = null;
            phase = GamePhase.RotatingBoard;
            UpdateHud();
            RefreshViews(); // show the piece that just locked before the board turns

            if (SfxManager.Instance != null)
            {
                SfxManager.Instance.PlayGravityChange();
            }
            
            bool rotateLeft = !isBoardRotated;
            if (boardView != null)
            {
                boardView.PlayBoardRotationAnimation(rotateLeft, FinishBoardRotation);
            }
            else
            {
                FinishBoardRotation();
            }
        }

        private void FinishBoardRotation()
        {
            int dropAmount = 0;
            if (!isBoardRotated)
            {
                dropAmount = board.RotateLeftAndSettle();
                isBoardRotated = true;
            }
            else
            {
                dropAmount = board.RotateRightAndSettle();
                isBoardRotated = false;
            }

            RefreshViews();

            if (dropAmount > 0 && boardView != null)
            {
                boardView.PlayBoardDropAnimation(dropAmount, OnBoardDropComplete);
            }
            else
            {
                OnBoardDropComplete();
            }
        }

        private void OnBoardDropComplete()
        {
            pendingIsRotation = true;

            var slideMovements = board.SlideSlipperyBlocks();
            if (slideMovements.Count > 0 && boardView != null)
            {
                RefreshViews(); // Temporarily update logical position behind the scenes
                boardView.PlaySlipperySlideAnimation(slideMovements, CheckLinesAndSettleAfterDrop);
            }
            else
            {
                CheckLinesAndSettleAfterDrop();
            }
        }

        private void CheckLinesAndSettleAfterDrop()
        {
            if (board.HasAdjacentMetalBlocks())
            {
                TriggerAllClearBomb();
                return;
            }

            List<int> fullRows = board.GetFullRows();
            if (fullRows.Count > 0)
            {
                pendingLineRowsToClear.Clear();
                pendingIsRotation = true;

                List<BlockCellInfo> cellsToAnimate = board.GetAllCellsInRows(fullRows);
                hiddenCells.Clear();
                foreach (var cell in cellsToAnimate) hiddenCells.Add(cell.Position);

                phase = GamePhase.ClearingLines;
                phaseTimer = 0.4f;

                if (SfxManager.Instance != null)
                {
                    SfxManager.Instance.PlayLineClear();
                }

                if (hudView != null)
                {
                    hudView.ShowActionText("GOOD!");
                }

                if (boardView != null)
                {
                    boardView.PlayClearAnimation(cellsToAnimate, 0.4f);
                }
                UpdateHud();
                RefreshViews();
                return;
            }

            comboCount = 0;
            movesBeforeRotationTarget = config.GetRandomMovesBeforeRotation();
            movesUntilRotation = movesBeforeRotationTarget;

            phase = GamePhase.Spawning;
            SpawnNextPiece();
            UpdateHud();
        }

        private void TriggerAllClearBomb()
        {
            List<BlockCellInfo> allCells = new List<BlockCellInfo>(board.GetOccupiedCells());
            
            int cellCount = allCells.Count;
            score += cellCount * config.PointsPerBlock + config.BombBonus;
            
            board.ClearAll();
            hiddenCells.Clear();

            phase = GamePhase.ClearingLines;
            phaseTimer = 1.0f; // Long animation duration

            // pendingIsRotation is left as set by the lock or rotation that caused the explosion, so ApplyPendingClears
            // counts it as that lock (rotating if due) or re-rolls the countdown after a rotation.
            pendingLineRowsToClear.Clear();

            if (SfxManager.Instance != null)
            {
                SfxManager.Instance.PlayBombExplosion();
            }

            if (boardView != null)
            {
                boardView.PlayBombAnimation(allCells, 1.0f);
            }

            if (hudView != null)
            {
                hudView.ShowActionText("EXCELLENT!");
            }
            
            UpdateHud();
            RefreshViews();
        }

        internal void ApplyPendingClears()
        {
            int totalClearedThisTurn = 0;

            if (pendingLineRowsToClear.Count > 0)
            {
                int blocksDestroyedByLine = 0;
                foreach (int rowY in pendingLineRowsToClear)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        if (board.IsOccupied(new Vector2Int(x, rowY)))
                        {
                            blocksDestroyedByLine++;
                        }
                    }
                }

                board.ClearRows(pendingLineRowsToClear);
                totalClearedThisTurn += pendingLineRowsToClear.Count;
                score += config.GetLineClearScore(pendingLineRowsToClear.Count, false);
                score += blocksDestroyedByLine * config.PointsPerBlock;
            }

            int clearedRows = board.ClearFullRows();
            if (clearedRows > 0)
            {
                totalClearedThisTurn += clearedRows;
                score += config.GetLineClearScore(clearedRows, pendingIsRotation);
                score += clearedRows * board.Width * config.PointsPerBlock;
            }

            if (totalClearedThisTurn > 0)
            {
                if (config != null) score += config.ComboBonus * comboCount;
                comboCount++;
                
                if (comboCount > 1 && hudView != null)
                {
                    hudView.ShowComboText(comboCount);
                }
            }
            // No rows cleared means this was a metal explosion: it keeps the combo going without adding a step
            // or a combo bonus (design decision DQ-1). A lock or rotation that clears nothing still ends it.

            hiddenCells.Clear();

            if (board.HasAdjacentMetalBlocks())
            {
                TriggerAllClearBomb();
                return;
            }

            if (pendingIsRotation)
            {
                movesBeforeRotationTarget = config.GetRandomMovesBeforeRotation();
                movesUntilRotation = movesBeforeRotationTarget;
                phase = GamePhase.Spawning;
                SpawnNextPiece();
            }
            else
            {
                movesUntilRotation--;
                if (movesUntilRotation <= 0)
                {
                    BeginBoardRotation();
                }
                else
                {
                    phase = GamePhase.Spawning;
                    SpawnNextPiece();
                }
            }

            UpdateHud();
            RefreshViews();
        }

        internal Vector2Int GetSpawnOrigin()
        {
            Vector2Int requested = config != null ? config.SpawnOrigin : new Vector2Int(board.Width / 2, 0);

            // Templates span x-1..x+2 (the I piece), so keep the whole piece on the board.
            int clampedX = Mathf.Clamp(requested.x, 1, board.Width - 3);
            int clampedY = Mathf.Clamp(requested.y, 0, board.Height - 1);
            return new Vector2Int(clampedX, clampedY);
        }

        private void UpdateHud()
        {
            if (hudView == null)
            {
                return;
            }

            hudView.SetScore(score);

            if (phase == GamePhase.RotatingBoard)
            {
                hudView.SetRotationProgress(1f, 0, !isBoardRotated, movesBeforeRotationTarget);
                return;
            }

            if (movesBeforeRotationTarget <= 0)
            {
                hudView.SetRotationProgress(0f, movesUntilRotation, !isBoardRotated, movesBeforeRotationTarget);
                return;
            }

            float progress = 1f - Mathf.Clamp01((float)movesUntilRotation / movesBeforeRotationTarget);
            hudView.SetRotationProgress(progress, movesUntilRotation, !isBoardRotated, movesBeforeRotationTarget);
        }

        private void TriggerGameOver()
        {
            if (phase == GamePhase.GameOver) return;

            phase = GamePhase.GameOver;
            isNewRecord = gameOverPopup != null && gameOverPopup.SaveHighScore(score);
            if (SfxManager.Instance != null) SfxManager.Instance.PlayGameOver();
            activePiece = null;
            UpdateHud();

            if (boardView != null)
            {
                boardView.Render(board, activePiece, nextPiece, heldPiece, hiddenCells);
                boardView.PlayGameOverAnimation(OnGameOverAnimationComplete);
            }
            else
            {
                OnGameOverAnimationComplete();
            }
        }

        private void OnGameOverAnimationComplete()
        {
            if (gameOverPopup != null)
            {
                gameOverPopup.Show(score, isNewRecord);
            }
        }

        private void RefreshViews()
        {
            if (phase == GamePhase.GameOver) return;

            if (boardView != null)
            {
                boardView.Render(board, activePiece, nextPiece, heldPiece, hiddenCells);
            }
        }
    }
}