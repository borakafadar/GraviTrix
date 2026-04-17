using System.Collections.Generic;
using GraviTrix.Runtime;
using GraviTrix.UI;
using UnityEngine;

namespace GraviTrix.Core
{
    public sealed class GameController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private GameConfig config;
        [SerializeField] private PieceLibrary pieceLibrary;

        [Header("Views")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private GameHudView hudView;

        private BoardGrid board;
        private PieceInstance activePiece;
        private GamePhase phase = GamePhase.Spawning;
        private float fallAccumulator;
        private float phaseTimer;
        private int score;
        private int movesUntilRotation;
        private int movesBeforeRotationTarget;

        public int Score => score;
        public GamePhase Phase => phase;
        public int MovesUntilRotation => movesUntilRotation;

        private void Awake()
        {
            board = new BoardGrid(config != null ? config.BoardWidth : 12, config != null ? config.BoardHeight : 12);
        }

        private void Start()
        {
            RestartGame();
        }

        private void Update()
        {
            if (phase == GamePhase.GameOver || config == null || pieceLibrary == null)
            {
                return;
            }

            if (phase == GamePhase.RotatingBoard)
            {
                phaseTimer -= Time.deltaTime;
                if (phaseTimer <= 0f)
                {
                    FinishBoardRotation();
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
            float fallInterval = activePiece.GetFallInterval(config);

            while (fallAccumulator >= fallInterval && phase == GamePhase.Falling)
            {
                fallAccumulator -= fallInterval;
                if (!TryAdvancePiece(Vector2Int.down))
                {
                    LockActivePiece();
                    break;
                }
            }

            UpdateHud();
            RefreshViews();
        }

        public void RestartGame()
        {
            board.Clear();
            score = 0;
            activePiece = null;
            phase = GamePhase.Spawning;
            fallAccumulator = 0f;
            if (config == null || pieceLibrary == null)
            {
                phase = GamePhase.GameOver;
                movesBeforeRotationTarget = 0;
                movesUntilRotation = 0;
                UpdateHud();
                RefreshViews();
                return;
            }

            movesBeforeRotationTarget = config.GetRandomMovesBeforeRotation();
            movesUntilRotation = movesBeforeRotationTarget;

            SpawnNextPiece();
            UpdateHud();
            RefreshViews();
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
                activePiece.TryRotateLeft(board);
                RefreshViews();
            }
        }

        public void RotatePieceRight()
        {
            if (CanControlActivePiece())
            {
                activePiece.TryRotateRight(board);
                RefreshViews();
            }
        }

        public void SoftDrop()
        {
            if (CanControlActivePiece())
            {
                if (!TryAdvancePiece(Vector2Int.down))
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

            while (TryAdvancePiece(Vector2Int.down))
            {
            }

            LockActivePiece();
        }

        private bool CanControlActivePiece()
        {
            return phase == GamePhase.Falling && activePiece != null;
        }

        private void SpawnNextPiece()
        {
            if (pieceLibrary.Count == 0)
            {
                phase = GamePhase.GameOver;
                UpdateHud();
                return;
            }

            PieceDefinition nextPiece = pieceLibrary.GetRandomPiece();
            if (nextPiece == null)
            {
                phase = GamePhase.GameOver;
                UpdateHud();
                return;
            }

            activePiece = new PieceInstance(nextPiece, GetSpawnOrigin());

            if (!board.CanOccupy(activePiece.GetWorldCells()))
            {
                phase = GamePhase.GameOver;
                activePiece = null;
                UpdateHud();
                RefreshViews();
                return;
            }

            phase = GamePhase.Falling;
            fallAccumulator = 0f;
            RefreshViews();
        }

        private bool TryAdvancePiece(Vector2Int delta)
        {
            if (activePiece == null)
            {
                return false;
            }

            if (delta == Vector2Int.down && activePiece.ContainsKind(BlockKind.Lava))
            {
                TryBurnCellsUnderLava();
            }

            bool moved = activePiece.TryMove(board, delta);
            if (moved)
            {
                RefreshViews();
            }

            return moved;
        }

        private void TryBurnCellsUnderLava()
        {
            List<Vector2Int> cellsToClear = new List<Vector2Int>();

            foreach (BlockCellInfo cell in activePiece.GetWorldCells())
            {
                if (cell.Kind != BlockKind.Lava)
                {
                    continue;
                }

                Vector2Int target = cell.Position + Vector2Int.down;
                if (board.IsOccupied(target))
                {
                    cellsToClear.Add(target);
                }
            }

            if (cellsToClear.Count > 0)
            {
                board.ClearCells(cellsToClear);
            }
        }

        private void LockActivePiece()
        {
            if (activePiece == null)
            {
                return;
            }

            List<int> lineRowsToClear = new List<int>();
            HashSet<int> seenRows = new HashSet<int>();

            foreach (BlockCellInfo cell in activePiece.GetWorldCells())
            {
                if (cell.Kind == BlockKind.Line && seenRows.Add(cell.Position.y))
                {
                    lineRowsToClear.Add(cell.Position.y);
                }
            }

            board.TryPlace(activePiece.GetWorldCells());

            if (lineRowsToClear.Count > 0)
            {
                int clearedByLine = board.ClearRows(lineRowsToClear);
                score += config.GetLineClearScore(clearedByLine, false);
            }

            int clearedRows = board.ClearFullRows();
            score += config.GetLineClearScore(clearedRows, false);

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
            RefreshViews();
        }

        private void BeginBoardRotation()
        {
            activePiece = null;
            phase = GamePhase.RotatingBoard;
            phaseTimer = config.BoardRotationPauseSeconds;
            UpdateHud();
        }

        private void FinishBoardRotation()
        {
            board.RotateLeftAndSettle();

            int clearedRows = board.ClearFullRows();
            score += config.GetLineClearScore(clearedRows, true);

            movesBeforeRotationTarget = config.GetRandomMovesBeforeRotation();
            movesUntilRotation = movesBeforeRotationTarget;

            phase = GamePhase.Spawning;
            SpawnNextPiece();
            UpdateHud();
            RefreshViews();
        }

        private Vector2Int GetSpawnOrigin()
        {
            if (config != null)
            {
                return config.SpawnOrigin;
            }

            return new Vector2Int(Mathf.Max(0, board.Width / 2 - 1), 0);
        }

        private void UpdateHud()
        {
            if (hudView == null)
            {
                return;
            }

            hudView.SetScore(score);

            string stateLabel = phase switch
            {
                GamePhase.Falling => "Falling",
                GamePhase.RotatingBoard => "Rotating",
                GamePhase.GameOver => "Game Over",
                _ => "Spawning"
            };

            hudView.SetState(stateLabel);

            if (phase == GamePhase.RotatingBoard)
            {
                hudView.SetRotationProgress(1f, 0);
                return;
            }

            if (movesBeforeRotationTarget <= 0)
            {
                hudView.SetRotationProgress(0f, movesUntilRotation);
                return;
            }

            float progress = 1f - Mathf.Clamp01((float)movesUntilRotation / movesBeforeRotationTarget);
            hudView.SetRotationProgress(progress, movesUntilRotation);
        }

        private void RefreshViews()
        {
            if (boardView != null)
            {
                boardView.Render(board, activePiece);
            }
        }
    }
}