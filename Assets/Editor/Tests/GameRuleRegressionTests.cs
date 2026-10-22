using System.Linq;
using GraviTrix.Core;
using GraviTrix.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GraviTrix.Tests
{
    // Regression tests for rules and fixes that were only checked by hand in the 2026-10-16 audit.
    // GameController runs without views, so a board rotation finishes synchronously.
    // Not covered here (they need coroutines, so PlayMode tests and asmdefs): restart during an animation
    // (GT-01), game-over popup timing, the HUD arrow, BoardRoot after explosion + rotation (GTM-005),
    // and HUD texts on restart (GTM-006).
    public sealed class GameRuleRegressionTests
    {
        private GameObject host;
        private GameController controller;
        private GameInputRelay relay;

        [SetUp]
        public void SetUp()
        {
            host = EditorUtility.CreateGameObjectWithHideFlags("GameRules (test)", HideFlags.HideAndDontSave);
            controller = host.AddComponent<GameController>();
            controller.config = ScriptableObject.CreateInstance<GameConfig>();
            controller.Awake();
            controller.RestartGame();
            controller.movesUntilRotation = 5; // no rotation unless a test asks for one
            relay = host.AddComponent<GameInputRelay>();
            relay.controller = controller;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Object.DestroyImmediate(controller.config);
            Object.DestroyImmediate(host);
        }

        // GT-02
        [Test]
        public void NoInputReachesTheGameWhilePaused()
        {
            controller.activePiece = Single(BlockKind.Normal, 5, 4);
            relay.OnMoveLeft();
            Assert.AreEqual(new Vector2Int(4, 4), controller.ActivePiece.Origin, "control: the relay moves the piece while running");

            Time.timeScale = 0f;
            PieceInstance piece = controller.ActivePiece;
            relay.OnMoveLeft();
            relay.OnMoveRight();
            relay.OnRotateLeft();
            relay.OnRotateRight();
            relay.OnSoftDrop();
            relay.OnHoldPiece();
            relay.OnHardDrop();

            Assert.AreSame(piece, controller.ActivePiece);
            Assert.AreEqual(new Vector2Int(4, 4), piece.Origin);
            Assert.IsNull(controller.HeldPiece);
            Assert.AreEqual(0, CellCount());
        }

        // GT-05
        [Test]
        public void MetalBroughtTogetherByARowClearExplodesAtOnce()
        {
            // Metal at (0,9) and (0,11) with row 10 between them; the drop at x=11 completes row 10.
            Place(0, 9, BlockKind.Metal);
            Place(0, 11, BlockKind.Metal);
            for (int x = 0; x < 11; x++) Place(x, 10, BlockKind.Normal);
            for (int x = 1; x < 12; x++) if (x != 6) Place(x, 11, BlockKind.Normal);
            controller.activePiece = Single(BlockKind.Normal, 11, 0);

            controller.HardDrop();
            controller.ApplyPendingClears();

            Assert.AreEqual(GamePhase.ClearingLines, controller.Phase, "the explosion should start right after the clear");
            Assert.AreEqual(0, CellCount(), "the explosion clears the whole board");
            Assert.GreaterOrEqual(controller.Score, controller.config.BombBonus);
        }

        [Test]
        public void HoldSwapIntoABlockedSpawnEndsTheGame()
        {
            controller.heldPiece = Single(BlockKind.Normal, 5, 0);
            controller.activePiece = Single(BlockKind.Normal, 5, 4);
            Place(5, 0, BlockKind.Normal);

            controller.HoldPiece();

            Assert.AreEqual(GamePhase.GameOver, controller.Phase);
        }

        // DQ-3: lava melts everything under it and always reaches the floor, where it turns to obsidian.
        [Test]
        public void LavaDrillsToTheFloorAndScoresEveryMeltedCell()
        {
            for (int y = 6; y < 12; y++)
            {
                Place(5, y, BlockKind.Normal);
                Place(6, y, y % 2 == 0 ? BlockKind.Metal : BlockKind.Slippery);
            }
            controller.activePiece = FallbackPieceFactory.CreateLava(new Vector2Int(5, 0));

            controller.HardDrop();

            Assert.AreEqual(12 * controller.config.LavaMeltScore, controller.Score);
            BlockCellInfo[] cells = controller.board.GetOccupiedCells().ToArray();
            CollectionAssert.AreEquivalent(
                new[] { new Vector2Int(5, 10), new Vector2Int(6, 10), new Vector2Int(5, 11), new Vector2Int(6, 11) },
                cells.Select(c => c.Position).ToArray());
            Assert.IsTrue(cells.All(c => c.Kind == BlockKind.Normal && c.VisualType == BlockVisualType.Obsidian));
        }

        [Test]
        public void SlipperyCellsSlideToTheFloorAfterARotation()
        {
            // Rotating left maps (x, y) to (y, 11 - x): the normal cell at (0,5) lands on the floor at (5,11),
            // so the rigid settle doesn't move anything, and the slippery cell at (2,2) ends up at (2,9) above a gap.
            Place(0, 5, BlockKind.Normal);
            Place(2, 2, BlockKind.Slippery);
            controller.activePiece = Single(BlockKind.Normal, 8, 0);
            controller.movesUntilRotation = 1;

            controller.HardDrop();

            Assert.IsTrue(controller.isBoardRotated);
            BlockCellInfo slippery = controller.board.GetOccupiedCells().Single(c => c.Kind == BlockKind.Slippery);
            Assert.AreEqual(new Vector2Int(2, 11), slippery.Position);
        }

        [Test]
        public void LineBlockClearsItsRowEvenWhenTheRowIsNotFull()
        {
            for (int x = 0; x < 4; x++) Place(x, 11, BlockKind.Normal);
            controller.activePiece = new PieceInstance(new Vector2Int(6, 0), Vector2Int.zero,
                new[] { new BlockCellState(Vector2Int.zero, BlockKind.Line, BlockVisualType.Line) });

            controller.HardDrop();
            controller.ApplyPendingClears();

            Assert.AreEqual(0, CellCount());
            // singleLineScore (100) + 5 removed blocks * pointsPerBlock (10)
            Assert.AreEqual(100 + 5 * 10, controller.Score);
        }

        [Test]
        public void TheFirstFourPiecesAreNormalTetrominoes()
        {
            for (int run = 0; run < 50; run++)
            {
                controller.RestartGame();
                for (int spawn = 1; spawn <= 4; spawn++)
                {
                    if (spawn > 1) controller.SpawnNextPiece();
                    PieceInstance piece = controller.ActivePiece;
                    Assert.AreEqual(4, piece.GetWorldCells().Count(), "spawn " + spawn + " is not a tetromino");
                    Assert.IsTrue(piece.GetWorldCells().All(c => c.Kind == BlockKind.Normal), "spawn " + spawn + " is special or slippery");
                }
            }
        }

        [Test]
        public void RescueLavaComesNextWhenTheCentreFillsUp()
        {
            for (int x = 4; x < 7; x++)
            {
                for (int y = 4; y < 6; y++) Place(x, y, BlockKind.Normal); // 6 cells in the central 4x4
            }
            controller.spawnedPiecesCount = 10;
            controller.rescueCooldown = 0;

            controller.SpawnNextPiece();

            Assert.IsTrue(controller.NextPiece.ContainsKind(BlockKind.Lava));
            Assert.AreEqual(4, controller.rescueCooldown);
        }

        private void Place(int x, int y, BlockKind kind)
        {
            BlockVisualType visual = kind == BlockKind.Metal ? BlockVisualType.Metal : kind == BlockKind.Slippery ? BlockVisualType.Slippery : BlockVisualType.Red;
            Assert.IsTrue(controller.board.TryPlace(new[] { new BlockCellInfo(new Vector2Int(x, y), kind, visual) }), "setup cell overlaps at " + x + "," + y);
        }

        private int CellCount() => controller.board.GetOccupiedCells().Count();

        private static PieceInstance Single(BlockKind kind, int x, int y) =>
            new PieceInstance(new Vector2Int(x, y), Vector2Int.zero, new[] { new BlockCellState(Vector2Int.zero, kind, BlockVisualType.Blue) });
    }
}
