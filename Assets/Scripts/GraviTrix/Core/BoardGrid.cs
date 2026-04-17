using System.Collections.Generic;
using UnityEngine;

namespace GraviTrix.Core
{
    public sealed class BoardGrid
    {
        private readonly int width;
        private readonly int height;
        private readonly bool[,] occupied;
        private readonly CellOccupant[,] occupants;

        public BoardGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
            occupied = new bool[width, height];
            occupants = new CellOccupant[width, height];
        }

        public int Width => width;
        public int Height => height;

        public void Clear()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    occupied[x, y] = false;
                    occupants[x, y] = default;
                }
            }
        }

        public bool IsInside(Vector2Int position)
        {
            return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
        }

        public bool IsOccupied(Vector2Int position)
        {
            return IsInside(position) && occupied[position.x, position.y];
        }

        public bool CanOccupy(IEnumerable<BlockCellInfo> cells)
        {
            foreach (BlockCellInfo cell in cells)
            {
                if (!IsInside(cell.Position) || occupied[cell.Position.x, cell.Position.y])
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryPlace(IEnumerable<BlockCellInfo> cells)
        {
            if (!CanOccupy(cells))
            {
                return false;
            }

            foreach (BlockCellInfo cell in cells)
            {
                occupied[cell.Position.x, cell.Position.y] = true;
                occupants[cell.Position.x, cell.Position.y] = new CellOccupant(cell.Kind, cell.VisualType);
            }

            return true;
        }

        public void ClearCell(Vector2Int position)
        {
            if (!IsInside(position))
            {
                return;
            }

            occupied[position.x, position.y] = false;
            occupants[position.x, position.y] = default;
        }

        public void ClearCells(IEnumerable<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                ClearCell(position);
            }
        }

        public IEnumerable<BlockCellInfo> GetOccupiedCells()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (occupied[x, y])
                    {
                        CellOccupant occupant = occupants[x, y];
                        yield return new BlockCellInfo(new Vector2Int(x, y), occupant.Kind, occupant.VisualType);
                    }
                }
            }
        }

        public int ClearFullRows()
        {
            List<int> rowsToRemove = new List<int>();

            for (int y = 0; y < height; y++)
            {
                bool fullRow = true;

                for (int x = 0; x < width; x++)
                {
                    if (!occupied[x, y])
                    {
                        fullRow = false;
                        break;
                    }
                }

                if (fullRow)
                {
                    rowsToRemove.Add(y);
                }
            }

            return ClearRows(rowsToRemove);
        }

        public int ClearRows(IReadOnlyCollection<int> rowsToRemove)
        {
            if (rowsToRemove == null || rowsToRemove.Count == 0)
            {
                return 0;
            }

            bool[] rowsMarked = new bool[height];
            foreach (int row in rowsToRemove)
            {
                if (row >= 0 && row < height)
                {
                    rowsMarked[row] = true;
                }
            }

            int writeRow = height - 1;
            for (int readRow = height - 1; readRow >= 0; readRow--)
            {
                if (rowsMarked[readRow])
                {
                    continue;
                }

                if (writeRow != readRow)
                {
                    CopyRow(readRow, writeRow);
                }

                writeRow--;
            }

            for (int y = writeRow; y >= 0; y--)
            {
                ClearRow(y);
            }

            return rowsToRemove.Count;
        }

        public void RotateLeftAndSettle()
        {
            List<BlockCellInfo> cells = new List<BlockCellInfo>();

            foreach (BlockCellInfo cell in GetOccupiedCells())
            {
                cells.Add(cell);
            }

            Clear();

            if (cells.Count == 0)
            {
                return;
            }

            List<BlockCellInfo> rotatedCells = new List<BlockCellInfo>(cells.Count);
            int maxY = int.MinValue;

            for (int index = 0; index < cells.Count; index++)
            {
                BlockCellInfo cell = cells[index];
                Vector2Int rotatedPosition = RotateBoardPositionLeft(cell.Position);
                rotatedCells.Add(new BlockCellInfo(rotatedPosition, cell.Kind, cell.VisualType));

                if (rotatedPosition.y > maxY)
                {
                    maxY = rotatedPosition.y;
                }
            }

            int dropAmount = Mathf.Max(0, height - 1 - maxY);

            for (int index = 0; index < rotatedCells.Count; index++)
            {
                BlockCellInfo cell = rotatedCells[index];
                Vector2Int finalPosition = new Vector2Int(cell.Position.x, cell.Position.y + dropAmount);
                if (IsInside(finalPosition))
                {
                    occupied[finalPosition.x, finalPosition.y] = true;
                    occupants[finalPosition.x, finalPosition.y] = new CellOccupant(cell.Kind, cell.VisualType);
                }
            }
        }

        public Vector2Int RotateBoardPositionLeft(Vector2Int position)
        {
            return new Vector2Int(position.y, width - 1 - position.x);
        }

        private void CopyRow(int sourceRow, int destinationRow)
        {
            for (int x = 0; x < width; x++)
            {
                occupied[x, destinationRow] = occupied[x, sourceRow];
                occupants[x, destinationRow] = occupants[x, sourceRow];
            }
        }

        private void ClearRow(int row)
        {
            for (int x = 0; x < width; x++)
            {
                occupied[x, row] = false;
                occupants[x, row] = default;
            }
        }
    }
}