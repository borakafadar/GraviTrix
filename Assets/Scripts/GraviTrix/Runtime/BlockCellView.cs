using GraviTrix.Core;
using UnityEngine;

namespace GraviTrix.Runtime
{
    public sealed class BlockCellView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BlockSkinLibrary skinLibrary;

        public void SetCell(BlockCellInfo cell, Color? tintOverride = null)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                return;
            }

            Sprite sprite = skinLibrary != null ? skinLibrary.GetSprite(cell.VisualType) : null;
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }

            spriteRenderer.color = tintOverride ?? Color.white;
        }
    }
}