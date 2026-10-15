using System.Reflection;
using GraviTrix.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GraviTrix.Tests
{
    // EditMode tests that drive GameController without any views (BoardView, HUD, popup) attached.
    public sealed class GameControllerTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject host;
        private GameController controller;

        [SetUp]
        public void SetUp()
        {
            // HideAndDontSave keeps the object out of the open scene, so tests don't dirty it.
            host = EditorUtility.CreateGameObjectWithHideFlags("GameController (test)", HideFlags.HideAndDontSave);
            controller = host.AddComponent<GameController>();
            Set("config", ScriptableObject.CreateInstance<GameConfig>());
            Call("Awake");
            Call("Start");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
        }

        [Test]
        public void LineClearWithoutViewsDoesNotThrow()
        {
            // Fill the bottom row except the last column, then hard-drop a single block into that gap.
            var board = (BoardGrid)typeof(GameController).GetField("board", Private).GetValue(controller);
            for (int x = 0; x < board.Width - 1; x++)
            {
                board.TryPlace(new[] { new BlockCellInfo(new Vector2Int(x, board.Height - 1), BlockKind.Normal, BlockVisualType.Red) });
            }

            Set("activePiece", new PieceInstance(new Vector2Int(board.Width - 1, 0), Vector2Int.zero,
                new[] { new BlockCellState(Vector2Int.zero, BlockKind.Normal, BlockVisualType.Red) }));

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
            SetConfig("pointsPerBlock", 7);
            var board = Board();
            for (int x = 0; x < board.Width - 1; x++) Place(board, x, board.Height - 1, BlockKind.Normal);
            Set("activePiece", Single(BlockKind.Normal, board.Width - 1));

            controller.HardDrop();
            Call("ApplyPendingClears");

            // singleLineScore (100) + 12 cleared blocks * pointsPerBlock (7)
            Assert.AreEqual(100 + 12 * 7, controller.Score);
        }

        [Test]
        public void BombBonusFromConfigScoresAMetalExplosion()
        {
            SetConfig("pointsPerBlock", 7);
            SetConfig("bombBonus", 250);
            var board = Board();
            Place(board, 5, board.Height - 1, BlockKind.Metal);
            Set("activePiece", Single(BlockKind.Metal, 5));

            controller.HardDrop();

            // 2 metal blocks * pointsPerBlock (7) + bombBonus (250)
            Assert.AreEqual(GamePhase.ClearingLines, controller.Phase);
            Assert.AreEqual(2 * 7 + 250, controller.Score);
        }

        [TestCase(11, 9)]
        [TestCase(0, 1)]
        [TestCase(5, 5)]
        public void SpawnOriginStaysWherePiecesFit(int requestedX, int expectedX)
        {
            var config = (GameConfig)typeof(GameController).GetField("config", Private).GetValue(controller);
            SetConfig("spawnOrigin", new Vector2Int(requestedX, -3));
            typeof(GameConfig).GetMethod("OnValidate", Private).Invoke(config, null);
            Assert.AreEqual(new Vector2Int(expectedX, 0), config.SpawnOrigin, "OnValidate");

            // Even without OnValidate (e.g. a value set from code), the controller keeps the I piece on the board.
            SetConfig("spawnOrigin", new Vector2Int(requestedX, 0));
            var origin = (Vector2Int)typeof(GameController).GetMethod("GetSpawnOrigin", Private).Invoke(controller, null);
            Assert.AreEqual(expectedX, origin.x, "GetSpawnOrigin");
        }

        private BoardGrid Board() => (BoardGrid)typeof(GameController).GetField("board", Private).GetValue(controller);

        private static void Place(BoardGrid board, int x, int y, BlockKind kind) =>
            board.TryPlace(new[] { new BlockCellInfo(new Vector2Int(x, y), kind, kind == BlockKind.Metal ? BlockVisualType.Metal : BlockVisualType.Red) });

        private static PieceInstance Single(BlockKind kind, int x) =>
            new PieceInstance(new Vector2Int(x, 0), Vector2Int.zero,
                new[] { new BlockCellState(Vector2Int.zero, kind, kind == BlockKind.Metal ? BlockVisualType.Metal : BlockVisualType.Red) });

        private void SetConfig(string field, object value)
        {
            var config = typeof(GameController).GetField("config", Private).GetValue(controller);
            typeof(GameConfig).GetField(field, Private).SetValue(config, value);
        }

        private void Set(string field, object value) => typeof(GameController).GetField(field, Private).SetValue(controller, value);

        private void Call(string method) => typeof(GameController).GetMethod(method, Private).Invoke(controller, null);
    }
}
