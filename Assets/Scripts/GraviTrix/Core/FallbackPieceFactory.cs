using System.Collections.Generic;
using UnityEngine;

namespace GraviTrix.Core
{
    public static class FallbackPieceFactory
    {
        private sealed class Template
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

        private static readonly Template I = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(-1, 0, BlockKind.Normal, BlockVisualType.Cyan),
                Cell(0, 0, BlockKind.Normal, BlockVisualType.Cyan),
                Cell(1, 0, BlockKind.Normal, BlockVisualType.Cyan),
                Cell(2, 0, BlockKind.Normal, BlockVisualType.Cyan)
            });

        // Not rotatable: a square looks the same rotated, and the corner pivot would only shift it.
        private static readonly Template O = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(0, 0, BlockKind.Normal, BlockVisualType.Yellow),
                Cell(1, 0, BlockKind.Normal, BlockVisualType.Yellow),
                Cell(0, 1, BlockKind.Normal, BlockVisualType.Yellow),
                Cell(1, 1, BlockKind.Normal, BlockVisualType.Yellow)
            }, rotatable: false);

        private static readonly Template T = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(-1, 0, BlockKind.Normal, BlockVisualType.Blue),
                Cell(0, 0, BlockKind.Normal, BlockVisualType.Blue),
                Cell(1, 0, BlockKind.Normal, BlockVisualType.Blue),
                Cell(0, 1, BlockKind.Normal, BlockVisualType.Blue)
            });

        private static readonly Template L = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(-1, 0, BlockKind.Normal, BlockVisualType.Green),
                Cell(0, 0, BlockKind.Normal, BlockVisualType.Green),
                Cell(1, 0, BlockKind.Normal, BlockVisualType.Green),
                Cell(1, 1, BlockKind.Normal, BlockVisualType.Green)
            });

        private static readonly Template J = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(-1, 0, BlockKind.Normal, BlockVisualType.DeepBlue),
                Cell(0, 0, BlockKind.Normal, BlockVisualType.DeepBlue),
                Cell(1, 0, BlockKind.Normal, BlockVisualType.DeepBlue),
                Cell(-1, 1, BlockKind.Normal, BlockVisualType.DeepBlue)
            });

        private static readonly Template S = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(0, 0, BlockKind.Normal, BlockVisualType.Pink),
                Cell(1, 0, BlockKind.Normal, BlockVisualType.Pink),
                Cell(-1, 1, BlockKind.Normal, BlockVisualType.Pink),
                Cell(0, 1, BlockKind.Normal, BlockVisualType.Pink)
            });

        private static readonly Template Z = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(-1, 0, BlockKind.Normal, BlockVisualType.Red),
                Cell(0, 0, BlockKind.Normal, BlockVisualType.Red),
                Cell(0, 1, BlockKind.Normal, BlockVisualType.Red),
                Cell(1, 1, BlockKind.Normal, BlockVisualType.Red)
            });

        // Special: line clear single cell
        private static readonly Template Line = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(0, 0, BlockKind.Line, BlockVisualType.Line)
            });

        // Special: lava 2x2 cell (not rotatable, same reason as O)
        private static readonly Template Lava = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(0, 0, BlockKind.Lava, BlockVisualType.Lava),
                Cell(1, 0, BlockKind.Lava, BlockVisualType.Lava),
                Cell(0, 1, BlockKind.Lava, BlockVisualType.Lava),
                Cell(1, 1, BlockKind.Lava, BlockVisualType.Lava)
            }, rotatable: false);

        // Special: metal single cell
        private static readonly Template Metal = new Template(
            new Vector2Int(0, 0),
            new[]
            {
                Cell(0, 0, BlockKind.Metal, BlockVisualType.Metal)
            });

        private static readonly Template[] Tetrominoes = { I, O, T, L, J, S, Z };

        // CreateRandom picks uniformly from these: 7 tetrominoes and 3 special pieces.
        private static readonly Template[] AllPieces = { I, O, T, L, J, S, Z, Line, Lava, Metal };

        public static PieceInstance CreateRandom(Vector2Int origin, bool forceNormal = false)
        {
            Template[] pool = forceNormal ? Tetrominoes : AllPieces;
            Template selected = pool[Random.Range(0, pool.Length)];

            // 15% chance for a tetromino to become a Slippery piece
            if (!forceNormal && System.Array.IndexOf(Tetrominoes, selected) >= 0 && Random.value < 0.15f)
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
            return CreateFromTemplate(Lava, origin);
        }

        public static PieceInstance CreateMetal(Vector2Int origin)
        {
            return CreateFromTemplate(Metal, origin);
        }

        // Also used by the EditMode tests to build a specific piece by template name.
        private static PieceInstance CreateFromTemplate(Template template, Vector2Int origin)
        {
            return Build(template, origin, template.Cells);
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