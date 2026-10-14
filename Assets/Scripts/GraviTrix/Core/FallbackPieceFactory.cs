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
            public bool Rotatable;

            public Template(Vector2Int pivot, BlockCellState[] cells, bool rotatable = true)
            {
                Pivot = pivot;
                Cells = cells;
                Rotatable = rotatable;
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
                }, rotatable: false), // a square looks the same rotated; the corner pivot would only shift it
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
                }, rotatable: false),
            // Special: metal single cell
            new Template(
                new Vector2Int(0, 0),
                new[]
                {
                    Cell(0, 0, BlockKind.Metal, BlockVisualType.Metal)
                })
        };

        public static PieceInstance CreateRandom(Vector2Int origin, bool forceNormal = false)
        {
            if (Templates.Length == 0)
            {
                return null;
            }

            int index;
            if (forceNormal)
            {
                // Only select basic tetrominos (indices 0-6)
                index = Random.Range(0, 7);
            }
            else
            {
                index = Random.Range(0, Templates.Length);
            }
            
            Template selected = Templates[index];

            // 15% chance for a normal piece (indices 0-6) to become a Slippery piece
            if (!forceNormal && index <= 6 && Random.value < 0.15f)
            {
                BlockCellState[] slipperyCells = new BlockCellState[selected.Cells.Length];
                for (int i = 0; i < selected.Cells.Length; i++)
                {
                    slipperyCells[i] = new BlockCellState(
                        selected.Cells[i].LocalPosition,
                        BlockKind.Slippery,
                        BlockVisualType.Slippery
                    );
                }
                return Build(selected, origin, slipperyCells);
            }

            return Build(selected, origin, selected.Cells);
        }

        public static PieceInstance CreateLava(Vector2Int origin)
        {
            return CreateFromTemplate(8, origin); // Index 8 is the 2x2 Lava piece
        }

        public static PieceInstance CreateMetal(Vector2Int origin)
        {
            return CreateFromTemplate(9, origin); // Index 9 is the metal single cell
        }

        // Also used by the EditMode tests to build a specific piece.
        private static PieceInstance CreateFromTemplate(int index, Vector2Int origin)
        {
            if (index >= Templates.Length) return null;
            return Build(Templates[index], origin, Templates[index].Cells);
        }

        private static PieceInstance Build(Template template, Vector2Int origin, BlockCellState[] cells)
        {
            return new PieceInstance(origin, template.Pivot, cells, template.Rotatable);
        }

        private static BlockCellState Cell(int x, int y, BlockKind kind, BlockVisualType visual)
        {
            return new BlockCellState(new Vector2Int(x, y), kind, visual);
        }
    }
}