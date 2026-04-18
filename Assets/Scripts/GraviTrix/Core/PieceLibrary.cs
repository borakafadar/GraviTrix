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

            List<PieceDefinition> validPieces = new List<PieceDefinition>();
            for (int index = 0; index < pieces.Count; index++)
            {
                if (pieces[index] != null)
                {
                    validPieces.Add(pieces[index]);
                }
            }

            if (validPieces.Count == 0)
            {
                return null;
            }

            return validPieces[Random.Range(0, validPieces.Count)];
        }
    }
}