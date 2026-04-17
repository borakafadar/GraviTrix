using System.Collections.Generic;
using GraviTrix.Core;
using UnityEngine;

namespace GraviTrix.Runtime
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private Transform boardRoot;
        [SerializeField] private BlockCellView cellPrefab;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Color activePieceTint = new Color(1f, 1f, 1f, 0.9f);

        private readonly List<BlockCellView> spawnedViews = new List<BlockCellView>();

        public void Render(BoardGrid board, PieceInstance activePiece)
        {
            ClearViews();

            if (board != null)
            {
                foreach (BlockCellInfo cell in board.GetOccupiedCells())
                {
                    SpawnCell(cell, null);
                }
            }

            if (activePiece != null)
            {
                foreach (BlockCellInfo cell in activePiece.GetWorldCells())
                {
                    SpawnCell(cell, activePieceTint);
                }
            }
        }

        private void SpawnCell(BlockCellInfo cell, Color? tintOverride)
        {
            if (cellPrefab == null)
            {
                return;
            }

            Transform root = boardRoot != null ? boardRoot : transform;
            BlockCellView view = Instantiate(cellPrefab, root);
            view.transform.localPosition = new Vector3(cell.Position.x * cellSize, -cell.Position.y * cellSize, 0f);
            view.SetCell(cell, tintOverride);
            spawnedViews.Add(view);
        }

        private void ClearViews()
        {
            for (int index = 0; index < spawnedViews.Count; index++)
            {
                if (spawnedViews[index] != null)
                {
                    Destroy(spawnedViews[index].gameObject);
                }
            }

            spawnedViews.Clear();
        }
    }
}