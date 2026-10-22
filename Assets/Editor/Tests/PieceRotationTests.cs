using System.Collections.Generic;
using System.Linq;
using GraviTrix.Core;
using NUnit.Framework;
using UnityEngine;

namespace GraviTrix.Tests
{
    public sealed class PieceRotationTests
    {
        // FallbackPieceFactory's internal templates, by the names used in test cases and failure messages.
        private static readonly Dictionary<string, FallbackPieceFactory.Template> Templates = new Dictionary<string, FallbackPieceFactory.Template>
        {
            { "I", FallbackPieceFactory.I }, { "O", FallbackPieceFactory.O }, { "T", FallbackPieceFactory.T },
            { "L", FallbackPieceFactory.L }, { "J", FallbackPieceFactory.J }, { "S", FallbackPieceFactory.S },
            { "Z", FallbackPieceFactory.Z }, { "Lava", FallbackPieceFactory.Lava }
        };

        // O is left out: a square has nothing to rotate.
        private static readonly string[] RotatingTetrominoes = { "I", "T", "L", "J", "S", "Z" };

        // GameConfig.asset spawn origin, as clamped by GameController.GetSpawnOrigin.
        private static readonly Vector2Int SpawnOrigin = new Vector2Int(5, 0);

        private static PieceInstance FromTemplate(string name, Vector2Int origin)
        {
            return FallbackPieceFactory.CreateFromTemplate(Templates[name], origin);
        }

        private static string Cells(PieceInstance piece)
        {
            return string.Join(" ", piece.GetWorldCells().Select(c => c.Position).OrderBy(p => p.y).ThenBy(p => p.x));
        }

        [TestCase("O")]
        [TestCase("Lava")]
        public void SquarePiecesDoNotMoveWhenRotated(string name)
        {
            var board = new BoardGrid(12, 12);
            PieceInstance piece = FromTemplate(name, new Vector2Int(5, 5));
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

            foreach (string name in RotatingTetrominoes)
            {
                foreach (bool left in new[] { true, false })
                {
                    var board = new BoardGrid(12, 12);
                    PieceInstance piece = FromTemplate(name, SpawnOrigin);
                    string before = Cells(piece);
                    bool rotated = left ? piece.TryRotateLeft(board) : piece.TryRotateRight(board);

                    if (!rotated || Cells(piece) == before || !board.CanOccupy(piece.GetWorldCells()))
                    {
                        failures.Add(name + (left ? " left" : " right"));
                    }
                }
            }

            Assert.IsEmpty(failures, "Could not rotate at the spawn row: " + string.Join(", ", failures));
        }

        [Test]
        public void RotationNeverKicksInOpenSpaceAndNeverLiftsAPiece()
        {
            foreach (string name in RotatingTetrominoes)
            {
                // Open space: no kick is needed, so left then right must land exactly where it started.
                var open = new BoardGrid(12, 12);
                PieceInstance piece = FromTemplate(name, new Vector2Int(5, 5));
                string start = Cells(piece);
                Assert.IsTrue(piece.TryRotateLeft(open));
                Assert.AreEqual(new Vector2Int(5, 5), piece.Origin, name + " was kicked in open space");
                Assert.IsTrue(piece.TryRotateRight(open));
                Assert.AreEqual(start, Cells(piece), name + " did not return to its start after left + right");

                // Spawn row: kicks may push the piece down, never up.
                foreach (bool left in new[] { true, false })
                {
                    PieceInstance spawned = FromTemplate(name, SpawnOrigin);
                    if (left) spawned.TryRotateLeft(open); else spawned.TryRotateRight(open);
                    Assert.GreaterOrEqual(spawned.Origin.y, SpawnOrigin.y, name + " was lifted by a rotation");
                }
            }
        }
    }
}
