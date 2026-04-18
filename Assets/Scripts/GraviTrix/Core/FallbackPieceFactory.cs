using System.Collections.Generic;
using UnityEngine;

namespace GraviTrix.Core
{
    public static class FallbackPieceFactory
    {
        private struct Template
        {
            public Vector2Int Pivot;
            public BlockCellState[] Cells;

            public Template(Vector2Int pivot, BlockCellState[] cells)
            {
                Pivot = pivot;
                Cells = cells;
            }
        }

        private static readonly Template[] Templates =
        {
            // I
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(-1, 0, BlockKind.Normal, BlockVisualType.Cyan),
                    Cell(0, 0, BlockKind.Normal, BlockVisualType.Cyan),
                    Cell(1, 0, BlockKind.Normal, BlockVisualType.Cyan),
                    Cell(2, 0, BlockKind.Normal, BlockVisualType.Cyan)
                }),
            // O
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(0, 0, BlockKind.Normal, BlockVisualType.Yellow),
                    Cell(1, 0, BlockKind.Normal, BlockVisualType.Yellow),
                    Cell(0, 1, BlockKind.Normal, BlockVisualType.Yellow),
                    Cell(1, 1, BlockKind.Normal, BlockVisualType.Yellow)
                }),
            // T
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(-1, 0, BlockKind.Normal, BlockVisualType.Blue),
                    Cell(0, 0, BlockKind.Normal, BlockVisualType.Blue),
                    Cell(1, 0, BlockKind.Normal, BlockVisualType.Blue),
                    Cell(0, 1, BlockKind.Normal, BlockVisualType.Blue)
                }),
            // L
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(-1, 0, BlockKind.Normal, BlockVisualType.Green),
                    Cell(0, 0, BlockKind.Normal, BlockVisualType.Green),
                    Cell(1, 0, BlockKind.Normal, BlockVisualType.Green),
                    Cell(1, 1, BlockKind.Normal, BlockVisualType.Green)
                }),
            // J
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(-1, 0, BlockKind.Normal, BlockVisualType.DeepBlue),
                    Cell(0, 0, BlockKind.Normal, BlockVisualType.DeepBlue),
                    Cell(1, 0, BlockKind.Normal, BlockVisualType.DeepBlue),
                    Cell(-1, 1, BlockKind.Normal, BlockVisualType.DeepBlue)
                }),
            // S
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(0, 0, BlockKind.Normal, BlockVisualType.Pink),
                    Cell(1, 0, BlockKind.Normal, BlockVisualType.Pink),
                    Cell(-1, 1, BlockKind.Normal, BlockVisualType.Pink),
                    Cell(0, 1, BlockKind.Normal, BlockVisualType.Pink)
                }),
            // Z
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(-1, 0, BlockKind.Normal, BlockVisualType.Red),
                    Cell(0, 0, BlockKind.Normal, BlockVisualType.Red),
                    Cell(0, 1, BlockKind.Normal, BlockVisualType.Red),
                    Cell(1, 1, BlockKind.Normal, BlockVisualType.Red)
                }),
            // Special: line clear single cell
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(0, 0, BlockKind.Line, BlockVisualType.Line)
                }),
            // Special: lava 2x2 cell
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(0, 0, BlockKind.Lava, BlockVisualType.Lava),
                    Cell(1, 0, BlockKind.Lava, BlockVisualType.Lava),
                    Cell(0, 1, BlockKind.Lava, BlockVisualType.Lava),
                    Cell(1, 1, BlockKind.Lava, BlockVisualType.Lava)
                }),
            // Special: metal single cell
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(0, 0, BlockKind.Metal, BlockVisualType.Metal)
                })
        };

        public static PieceInstance CreateRandom(Vector2Int origin)
        {
            if (Templates.Length == 0)
            {
                return null;
            }

            Template selected = Templates[Random.Range(0, Templates.Length)];
            return new PieceInstance(origin, selected.Pivot, selected.Cells);
        }

        private static BlockCellState Cell(int x, int y, BlockKind kind, BlockVisualType visual)
        {
            return new BlockCellState(new Vector2Int(x, y), kind, visual);
        }
    }
}