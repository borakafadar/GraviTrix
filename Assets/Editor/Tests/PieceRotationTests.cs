using System.Collections.Generic;
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
        private static readonly string[] TetrominoNames = { "I", "O", "T", "L", "J", "S", "Z" };
        private const int O = 1;
        private const int Lava = 8;

        // GameConfig.asset spawn origin, as clamped by GameController.GetSpawnOrigin.
        private static readonly Vector2Int SpawnOrigin = new Vector2Int(5, 0);

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

        [Test]
        public void EveryTetrominoRotatesBothWaysRightAfterSpawning()
        {
            var failures = new List<string>();

            for (int template = 0; template < TetrominoNames.Length; template++)
            {
                if (template == O) continue; // a square has nothing to rotate

                foreach (bool left in new[] { true, false })
                {
                    var board = new BoardGrid(12, 12);
                    PieceInstance piece = FromTemplate(template, SpawnOrigin);
                    string before = Cells(piece);
                    bool rotated = left ? piece.TryRotateLeft(board) : piece.TryRotateRight(board);

                    if (!rotated || Cells(piece) == before || !board.CanOccupy(piece.GetWorldCells()))
                    {
                        failures.Add(TetrominoNames[template] + (left ? " left" : " right"));
                    }
                }
            }

            Assert.IsEmpty(failures, "Could not rotate at the spawn row: " + string.Join(", ", failures));
        }

        [Test]
        public void RotationNeverKicksInOpenSpaceAndNeverLiftsAPiece()
        {
            for (int template = 0; template < TetrominoNames.Length; template++)
            {
                if (template == O) continue;

                // Open space: no kick is needed, so left then right must land exactly where it started.
                var open = new BoardGrid(12, 12);
                PieceInstance piece = FromTemplate(template, new Vector2Int(5, 5));
                string start = Cells(piece);
                Assert.IsTrue(piece.TryRotateLeft(open));
                Assert.AreEqual(new Vector2Int(5, 5), piece.Origin, TetrominoNames[template] + " was kicked in open space");
                Assert.IsTrue(piece.TryRotateRight(open));
                Assert.AreEqual(start, Cells(piece), TetrominoNames[template] + " did not return to its start after left + right");

                // Spawn row: kicks may push the piece down, never up.
                foreach (bool left in new[] { true, false })
                {
                    PieceInstance spawned = FromTemplate(template, SpawnOrigin);
                    if (left) spawned.TryRotateLeft(open); else spawned.TryRotateRight(open);
                    Assert.GreaterOrEqual(spawned.Origin.y, SpawnOrigin.y, TetrominoNames[template] + " was lifted by a rotation");
                }
            }
        }
    }
}
