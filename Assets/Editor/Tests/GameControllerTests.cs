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

        private void Set(string field, object value) => typeof(GameController).GetField(field, Private).SetValue(controller, value);

        private void Call(string method) => typeof(GameController).GetMethod(method, Private).Invoke(controller, null);
    }
}
