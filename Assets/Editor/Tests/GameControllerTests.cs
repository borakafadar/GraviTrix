using GraviTrix.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GraviTrix.Tests
{
    // EditMode tests that drive GameController without any views (BoardView, HUD, popup) attached.
    // They use the controller's internal members (InternalsVisibleTo in Assets/Scripts/AssemblyInfo.cs).
    public sealed class GameControllerTests
    {
        private GameObject host;
        private GameController controller;

        [SetUp]
        public void SetUp()
        {
            // HideAndDontSave keeps the object out of the open scene, so tests don't dirty it.
            host = EditorUtility.CreateGameObjectWithHideFlags("GameController (test)", HideFlags.HideAndDontSave);
            controller = host.AddComponent<GameController>();
            controller.config = ScriptableObject.CreateInstance<GameConfig>();
            controller.Awake();
            controller.RestartGame();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(controller.config);
            Object.DestroyImmediate(host);
        }

        [Test]
        public void LineClearWithoutViewsDoesNotThrow()
        {
            // Fill the bottom row except the last column, then hard-drop a single block into that gap.
            BoardGrid board = controller.board;
            for (int x = 0; x < board.Width - 1; x++)
            {
                board.TryPlace(new[] { new BlockCellInfo(new Vector2Int(x, board.Height - 1), BlockKind.Normal, BlockVisualType.Red) });
            }

            controller.activePiece = Single(BlockKind.Normal, board.Width - 1);

            Assert.DoesNotThrow(() => controller.HardDrop());
            Assert.AreEqual(GamePhase.ClearingLines, controller.Phase);
        }

        [Test]
        public void SecondHoldBeforeLockingDoesNothing()
        {
            // New run, empty hold slot: the first hold stores the piece and brings in NEXT.
            PieceInstance first = controller.ActivePiece;
            controller.HoldPiece();
            PieceInstance second = controller.ActivePiece;
            Assert.AreSame(first, controller.HeldPiece);
            Assert.AreNotSame(first, second);

            // A second hold before the piece locks must be ignored.
            controller.HoldPiece();
            Assert.AreSame(second, controller.ActivePiece, "second hold swapped the active piece");
            Assert.AreSame(first, controller.HeldPiece, "second hold changed the held piece");
        }

        [Test]
        public void PointsPerBlockFromConfigScoresALineClear()
        {
            controller.config.pointsPerBlock = 7;
            BoardGrid board = controller.board;
            for (int x = 0; x < board.Width - 1; x++) Place(board, x, board.Height - 1, BlockKind.Normal);
            controller.activePiece = Single(BlockKind.Normal, board.Width - 1);

            controller.HardDrop();
            controller.ApplyPendingClears();

            // singleLineScore (100) + 12 cleared blocks * pointsPerBlock (7)
            Assert.AreEqual(100 + 12 * 7, controller.Score);
        }

        [Test]
        public void BombBonusFromConfigScoresAMetalExplosion()
        {
            controller.config.pointsPerBlock = 7;
            controller.config.bombBonus = 250;
            BoardGrid board = controller.board;
            Place(board, 5, board.Height - 1, BlockKind.Metal);
            controller.activePiece = Single(BlockKind.Metal, 5);

            controller.HardDrop();

            // 2 metal blocks * pointsPerBlock (7) + bombBonus (250)
            Assert.AreEqual(GamePhase.ClearingLines, controller.Phase);
            Assert.AreEqual(2 * 7 + 250, controller.Score);
        }

        [Test]
        public void MetalExplosionOnLockStillRunsADueRotation()
        {
            BoardGrid board = controller.board;
            Place(board, 5, board.Height - 1, BlockKind.Metal);
            controller.activePiece = Single(BlockKind.Metal, 5);
            controller.movesUntilRotation = 1;
            controller.pendingIsRotation = true; // stale value left over from an earlier rotation's clear

            controller.HardDrop();
            controller.ApplyPendingClears();

            // Without views the rotation finishes at once, so check that the board turned.
            Assert.IsTrue(controller.isBoardRotated);
        }

        [TestCase(11, 9)]
        [TestCase(0, 1)]
        [TestCase(5, 5)]
        public void SpawnOriginStaysWherePiecesFit(int requestedX, int expectedX)
        {
            GameConfig config = controller.config;
            config.spawnOrigin = new Vector2Int(requestedX, -3);
            config.OnValidate();
            Assert.AreEqual(new Vector2Int(expectedX, 0), config.SpawnOrigin, "OnValidate");

            // Even without OnValidate (e.g. a value set from code), the controller keeps the I piece on the board.
            config.spawnOrigin = new Vector2Int(requestedX, 0);
            Assert.AreEqual(expectedX, controller.GetSpawnOrigin().x, "GetSpawnOrigin");
        }

        private static void Place(BoardGrid board, int x, int y, BlockKind kind) =>
            board.TryPlace(new[] { new BlockCellInfo(new Vector2Int(x, y), kind, kind == BlockKind.Metal ? BlockVisualType.Metal : BlockVisualType.Red) });

        private static PieceInstance Single(BlockKind kind, int x) =>
            new PieceInstance(new Vector2Int(x, 0), Vector2Int.zero,
                new[] { new BlockCellState(Vector2Int.zero, kind, kind == BlockKind.Metal ? BlockVisualType.Metal : BlockVisualType.Red) });
    }
}
