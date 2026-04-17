using System.Collections.Generic;
using UnityEngine;

namespace GraviTrix.Core
{
    [CreateAssetMenu(menuName = "GraviTrix/Piece Definition", fileName = "PieceDefinition")]
    public sealed class PieceDefinition : ScriptableObject
    {
        [SerializeField] private string pieceName = "New Piece";
        [SerializeField] private Vector2Int pivot;
        [SerializeField] private List<BlockCellDefinition> cells = new List<BlockCellDefinition>();

        public string PieceName => pieceName;
        public Vector2Int Pivot => pivot;
        public IReadOnlyList<BlockCellDefinition> Cells => cells;

        public IEnumerable<BlockCellState> CreateRuntimeCells()
        {
            for (int index = 0; index < cells.Count; index++)
            {
                BlockCellDefinition definition = cells[index];
                BlockVisualType visualType = BlockVisualTypeResolver.Resolve(definition.Kind, definition.VisualType);
                yield return new BlockCellState(definition.LocalPosition, definition.Kind, visualType);
            }
        }
    }
}