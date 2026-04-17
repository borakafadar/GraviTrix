using System.Collections.Generic;
using UnityEngine;

namespace GraviTrix.Core
{
    public sealed class PieceInstance
    {
        private readonly Vector2Int pivot;
        private readonly List<BlockCellState> cells;

        public PieceInstance(PieceDefinition definition, Vector2Int origin)
        {
            Definition = definition;
            Origin = origin;
            pivot = definition != null ? definition.Pivot : Vector2Int.zero;
            cells = new List<BlockCellState>();

            if (definition != null)
            {
                foreach (BlockCellState cell in definition.CreateRuntimeCells())
                {
                    cells.Add(cell);
                }
            }
        }

        public PieceDefinition Definition { get; }
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
            List<BlockCellInfo> rotatedCells = new List<BlockCellInfo>(cells.Count);

            for (int index = 0; index < cells.Count; index++)
            {
                BlockCellState cell = cells[index];
                Vector2Int rotatedLocalPosition = rotateLeft
                    ? RotateLeftAroundPivot(cell.LocalPosition, pivot)
                    : RotateRightAroundPivot(cell.LocalPosition, pivot);

                rotatedCells.Add(new BlockCellInfo(Origin + rotatedLocalPosition, cell.Kind, cell.VisualType));
            }

            if (!board.CanOccupy(rotatedCells))
            {
                return false;
            }

            for (int index = 0; index < cells.Count; index++)
            {
                BlockCellState cell = cells[index];
                cell.LocalPosition = rotateLeft
                    ? RotateLeftAroundPivot(cell.LocalPosition, pivot)
                    : RotateRightAroundPivot(cell.LocalPosition, pivot);
                cells[index] = cell;
            }

            return true;
        }

        private IEnumerable<BlockCellInfo> GetWorldCellsAtOrigin(Vector2Int origin)
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