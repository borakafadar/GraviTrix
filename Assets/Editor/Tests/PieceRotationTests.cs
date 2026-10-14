using System.Linq;
using System.Reflection;
using GraviTrix.Core;
using NUnit.Framework;
using UnityEngine;

namespace GraviTrix.Tests
{
    public sealed class PieceRotationTests
    {
        // Template order in FallbackPieceFactory: I, O, T, L, J, S, Z, Line, Lava, Metal.
        private const int O = 1;
        private const int Lava = 8;

        private static PieceInstance FromTemplate(int index, Vector2Int origin)
        {
            MethodInfo create = typeof(FallbackPieceFactory).GetMethod("CreateFromTemplate", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(create, "FallbackPieceFactory.CreateFromTemplate(int, Vector2Int) not found");
            return (PieceInstance)create.Invoke(null, new object[] { index, origin });
        }

        private static string Cells(PieceInstance piece)
        {
            return string.Join(" ", piece.GetWorldCells().Select(c => c.Position).OrderBy(p => p.y).ThenBy(p => p.x));
        }

        [TestCase(O, "O")]
        [TestCase(Lava, "Lava")]
        public void SquarePiecesDoNotMoveWhenRotated(int template, string name)
        {
            var board = new BoardGrid(12, 12);
            PieceInstance piece = FromTemplate(template, new Vector2Int(5, 5));
            string start = Cells(piece);

            for (int i = 0; i < 4; i++)
            {
                piece.TryRotateLeft(board);
                Assert.AreEqual(start, Cells(piece), name + " moved after rotate-left #" + (i + 1));
            }

            for (int i = 0; i < 4; i++)
            {
                piece.TryRotateRight(board);
                Assert.AreEqual(start, Cells(piece), name + " moved after rotate-right #" + (i + 1));
            }
        }
    }
}
