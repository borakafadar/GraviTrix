using GraviTrix.Core;
using NUnit.Framework;
using UnityEngine;

namespace GraviTrix.Tests
{
    // EditMode tests for BoardGrid. Run from Window > General > Test Runner (EditMode).
    public sealed class BoardGridTests
    {
        private static BoardGrid BoardWithExtinguishedLava()
        {
            var board = new BoardGrid(12, 12);
            board.TryPlace(new[]
            {
                new BlockCellInfo(new Vector2Int(3, 11), BlockKind.Lava, BlockVisualType.Lava),
                new BlockCellInfo(new Vector2Int(3, 10), BlockKind.Lava, BlockVisualType.Lava)
            });
            Assert.AreEqual(2, board.LastExtinguished.Count, "lava touching the floor should be extinguished");
            return board;
        }

        [Test]
        public void ClearResetsLastExtinguished()
        {
            BoardGrid board = BoardWithExtinguishedLava();
            board.Clear();
            Assert.AreEqual(0, board.LastExtinguished.Count);
        }

        [Test]
        public void ClearAllResetsLastExtinguished()
        {
            BoardGrid board = BoardWithExtinguishedLava();
            board.ClearAll();
            Assert.AreEqual(0, board.LastExtinguished.Count);
        }

        [Test]
        public void EmptyBoardRotationResetsLastExtinguished()
        {
            // Remove the cells without calling Clear(), so only the rotation can reset the list.
            BoardGrid left = BoardWithExtinguishedLava();
            left.ClearCells(new[] { new Vector2Int(3, 11), new Vector2Int(3, 10) });
            left.RotateLeftAndSettle();
            Assert.AreEqual(0, left.LastExtinguished.Count, "RotateLeftAndSettle on an empty board");

            BoardGrid right = BoardWithExtinguishedLava();
            right.ClearCells(new[] { new Vector2Int(3, 11), new Vector2Int(3, 10) });
            right.RotateRightAndSettle();
            Assert.AreEqual(0, right.LastExtinguished.Count, "RotateRightAndSettle on an empty board");
        }
    }
}
