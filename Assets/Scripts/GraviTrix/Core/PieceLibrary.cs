using System.Collections.Generic;
using UnityEngine;

namespace GraviTrix.Core
{
    [CreateAssetMenu(menuName = "GraviTrix/Piece Library", fileName = "PieceLibrary")]
    public sealed class PieceLibrary : ScriptableObject
    {
        [SerializeField] private List<PieceDefinition> pieces = new List<PieceDefinition>();

        public int Count => pieces.Count;

        public PieceDefinition GetRandomPiece()
        {
            if (pieces.Count == 0)
            {
                return null;
            }

            return pieces[Random.Range(0, pieces.Count)];
        }
    }
}