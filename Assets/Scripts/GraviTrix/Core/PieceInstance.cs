using System.Collections.Generic;
using UnityEngine;

namespace GraviTrix.Core
{
    public sealed class PieceInstance
    {
        private readonly Vector2Int pivot;
        private readonly List<BlockCellState> cells;
        private readonly List<BlockCellState> originalCells;
        private readonly bool rotatable = true;

        public PieceInstance(Vector2Int origin, Vector2Int pivot, IEnumerable<BlockCellState> runtimeCells, bool rotatable = true)
        {
            Origin = origin;
            this.pivot = pivot;
            this.rotatable = rotatable;
            cells = new List<BlockCellState>();
            originalCells = new List<BlockCellState>();

            if (runtimeCells == null)
            {
                return;
            }

            foreach (BlockCellState cell in runtimeCells)
            {
                cells.Add(cell);
                originalCells.Add(cell);
            }
        }

        public void Reset(Vector2Int newOrigin)
        {
            Origin = newOrigin;
            cells.Clear();
            foreach (BlockCellState cell in originalCells)
            {
                cells.Add(cell);
            }
        }

        public Vector2Int Origin { get; private set; }

        public IEnumerable<BlockCellInfo> GetWorldCells()
        {
            for (int index = 0; index < cells.Count; index++)
            {
                BlockCellState cell = cells[index];
                yield return new BlockCellInfo(Origin + cell.LocalPosition, cell.Kind, cell.VisualType);
            }
        }

        public bool ContainsKind(BlockKind kind)
        {
            for (int index = 0; index < cells.Count; index++)
            {
                if (cells[index].Kind == kind)
                {
                    return true;
                }
            }

            return false;
        }

        public float GetFallInterval(GameConfig config)
        {
            if (ContainsKind(BlockKind.Lava) || ContainsKind(BlockKind.Metal))
            {
                return config.FastFallInterval;
            }

            return config.NormalFallInterval;
        }

        public bool TryMove(BoardGrid board, Vector2Int delta)
        {
            Vector2Int nextOrigin = Origin + delta;

            if (!board.CanOccupy(GetWorldCellsAtOrigin(nextOrigin)))
            {
                return false;
            }

            Origin = nextOrigin;
            return true;
        }

        public bool TryRotateLeft(BoardGrid board)
        {
            return TryRotate(board, true);
        }

        public bool TryRotateRight(BoardGrid board)
        {
            return TryRotate(board, false);
        }

        private bool TryRotate(BoardGrid board, bool rotateLeft)
        {
            if (!rotatable)
            {
                return false;
            }

            List<BlockCellState> rotated = new List<BlockCellState>(cells.Count);

            for (int index = 0; index < cells.Count; index++)
            {
                BlockCellState cell = cells[index];
                cell.LocalPosition = rotateLeft
                    ? RotateLeftAroundPivot(cell.LocalPosition, pivot)
                    : RotateRightAroundPivot(cell.LocalPosition, pivot);
                rotated.Add(cell);
            }

            // If the rotated piece doesn't fit where it is, try nudging it sideways or down.
            foreach (Vector2Int kick in RotationKicks)
            {
                if (board.CanOccupy(CellsAt(rotated, Origin + kick)))
                {
                    cells.Clear();
                    cells.AddRange(rotated);
                    Origin += kick;
                    return true;
                }
            }

            return false;
        }

        // Tried in order. Downward kicks let pieces rotate on the spawn row, where turning pushes
        // cells above the board (I needs two rows). There are no upward kicks, so rotating can never
        // lift a piece or keep it from falling.
        private static readonly Vector2Int[] RotationKicks =
        {
            new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)
        };

        private static IEnumerable<BlockCellInfo> CellsAt(List<BlockCellState> localCells, Vector2Int origin)
        {
            foreach (BlockCellState cell in localCells)
            {
                yield return new BlockCellInfo(origin + cell.LocalPosition, cell.Kind, cell.VisualType);
            }
        }

        public IEnumerable<BlockCellInfo> GetWorldCellsAtOrigin(Vector2Int origin)
        {
            for (int index = 0; index < cells.Count; index++)
            {
                BlockCellState cell = cells[index];
                yield return new BlockCellInfo(origin + cell.LocalPosition, cell.Kind, cell.VisualType);
            }
        }

        private static Vector2Int RotateLeftAroundPivot(Vector2Int value, Vector2Int pivotPoint)
        {
            Vector2Int relative = value - pivotPoint;
            Vector2Int rotated = new Vector2Int(relative.y, -relative.x);
            return pivotPoint + rotated;
        }

        private static Vector2Int RotateRightAroundPivot(Vector2Int value, Vector2Int pivotPoint)
        {
            Vector2Int relative = value - pivotPoint;
            Vector2Int rotated = new Vector2Int(-relative.y, relative.x);
            return pivotPoint + rotated;
        }
    }
}