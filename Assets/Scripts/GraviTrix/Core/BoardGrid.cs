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

        public List<BlockCellInfo> LastExtinguished = new List<BlockCellInfo>();

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

            ExtinguishLavaAtBottom();

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

        public List<int> GetFullRows()
        {
            List<int> fullRows = new List<int>();

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
                    fullRows.Add(y);
                }
            }

            return fullRows;
        }

        public List<BlockCellInfo> GetCellsInRows(IEnumerable<int> rows)
        {
            List<BlockCellInfo> cells = new List<BlockCellInfo>();
            HashSet<int> rowSet = new HashSet<int>(rows);

            for (int y = 0; y < height; y++)
            {
                if (!rowSet.Contains(y))
                {
                    continue;
                }

                for (int x = 0; x < width; x++)
                {
                    if (occupied[x, y])
                    {
                        CellOccupant occupant = occupants[x, y];
                        cells.Add(new BlockCellInfo(new Vector2Int(x, y), occupant.Kind, occupant.VisualType));
                    }
                }
            }

            return cells;
        }

        public List<BlockCellInfo> GetAllCellsInRows(IEnumerable<int> rows)
        {
            List<BlockCellInfo> cells = new List<BlockCellInfo>();
            HashSet<int> rowSet = new HashSet<int>(rows);

            for (int y = 0; y < height; y++)
            {
                if (!rowSet.Contains(y))
                {
                    continue;
                }

                for (int x = 0; x < width; x++)
                {
                    cells.Add(new BlockCellInfo(new Vector2Int(x, y), BlockKind.Normal, BlockVisualType.Auto));
                }
            }

            return cells;
        }

        public List<BlockCellInfo> GetCells(IEnumerable<Vector2Int> positions)
        {
            List<BlockCellInfo> cells = new List<BlockCellInfo>();
            foreach (Vector2Int pos in positions)
            {
                if (IsOccupied(pos))
                {
                    CellOccupant occupant = occupants[pos.x, pos.y];
                    cells.Add(new BlockCellInfo(pos, occupant.Kind, occupant.VisualType));
                }
            }
            return cells;
        }

        public int ClearFullRows()
        {
            return ClearRows(GetFullRows());
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

            ExtinguishLavaAtBottom();

            return rowsToRemove.Count;
        }

        public int RotateLeftAndSettle()
        {
            List<BlockCellInfo> cells = new List<BlockCellInfo>();

            foreach (BlockCellInfo cell in GetOccupiedCells())
            {
                cells.Add(cell);
            }

            Clear();

            if (cells.Count == 0)
            {
                return 0;
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
            
            ExtinguishLavaAtBottom();
            return dropAmount;
        }

        public Vector2Int RotateBoardPositionLeft(Vector2Int position)
        {
            return new Vector2Int(position.y, width - 1 - position.x);
        }

        public int RotateRightAndSettle()
        {
            List<BlockCellInfo> cells = new List<BlockCellInfo>();

            foreach (BlockCellInfo cell in GetOccupiedCells())
            {
                cells.Add(cell);
            }

            Clear();

            if (cells.Count == 0)
            {
                return 0;
            }

            List<BlockCellInfo> rotatedCells = new List<BlockCellInfo>(cells.Count);
            int maxY = int.MinValue;

            for (int index = 0; index < cells.Count; index++)
            {
                BlockCellInfo cell = cells[index];
                Vector2Int rotatedPosition = RotateBoardPositionRight(cell.Position);
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

            ExtinguishLavaAtBottom();
            return dropAmount;
        }

        public Vector2Int RotateBoardPositionRight(Vector2Int position)
        {
            return new Vector2Int(height - 1 - position.y, position.x);
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

        private void ExtinguishLavaAtBottom()
        {
            LastExtinguished.Clear();
            int bottomY = height - 1;
            List<Vector2Int> lavaToExtinguish = new List<Vector2Int>();
            
            for (int x = 0; x < width; x++)
            {
                if (occupied[x, bottomY] && occupants[x, bottomY].Kind == BlockKind.Lava)
                {
                    lavaToExtinguish.Add(new Vector2Int(x, bottomY));
                }
            }

            if (lavaToExtinguish.Count > 0)
            {
                HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                
                foreach (var startNode in lavaToExtinguish)
                {
                    if (visited.Add(startNode))
                    {
                        queue.Enqueue(startNode);
                    }
                }

                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    LastExtinguished.Add(new BlockCellInfo
                    {
                        Position = current,
                        Kind = occupants[current.x, current.y].Kind,
                        VisualType = occupants[current.x, current.y].VisualType
                    });
                    occupants[current.x, current.y] = new CellOccupant(BlockKind.Normal, BlockVisualType.Obsidian);
                    
                    foreach (var dir in dirs)
                    {
                        Vector2Int neighbor = current + dir;
                        if (IsInside(neighbor) && occupied[neighbor.x, neighbor.y] && occupants[neighbor.x, neighbor.y].Kind == BlockKind.Lava)
                        {
                            if (visited.Add(neighbor))
                            {
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }
        }
        public struct SlideInfo
        {
            public Vector2Int From;
            public Vector2Int To;
        }

        public List<SlideInfo> SlideSlipperyBlocks()
        {
            List<SlideInfo> movements = new List<SlideInfo>();

            for (int y = height - 2; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (occupied[x, y] && occupants[x, y].Kind == BlockKind.Slippery)
                    {
                        int targetY = y;
                        while (targetY + 1 < height && !occupied[x, targetY + 1])
                        {
                            targetY++;
                        }

                        if (targetY != y)
                        {
                            occupied[x, targetY] = true;
                            occupants[x, targetY] = occupants[x, y];
                            
                            occupied[x, y] = false;
                            occupants[x, y] = default;

                            movements.Add(new SlideInfo { From = new Vector2Int(x, y), To = new Vector2Int(x, targetY) });
                        }
                    }
                }
            }
            return movements;
        }
        public bool HasAdjacentMetalBlocks()
        {
            // Check horizontally
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    if (occupied[x, y] && occupants[x, y].Kind == BlockKind.Metal &&
                        occupied[x + 1, y] && occupants[x + 1, y].Kind == BlockKind.Metal)
                    {
                        return true;
                    }
                }
            }

            // Check vertically
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    if (occupied[x, y] && occupants[x, y].Kind == BlockKind.Metal &&
                        occupied[x, y + 1] && occupants[x, y + 1].Kind == BlockKind.Metal)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ClearAll()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    occupied[x, y] = false;
                    occupants[x, y] = default(CellOccupant);
                }
            }
        }
    }
}