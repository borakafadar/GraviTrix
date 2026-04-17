using GraviTrix.Core;
using UnityEngine;

namespace GraviTrix.Runtime
{
    public sealed class BlockCellView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BlockSkinLibrary skinLibrary;
        [SerializeField] private Color normalColor = new Color(0.25f, 0.75f, 1f, 1f);
        [SerializeField] private Color lavaColor = new Color(1f, 0.35f, 0.1f, 1f);
        [SerializeField] private Color metalColor = new Color(0.75f, 0.8f, 0.85f, 1f);
        [SerializeField] private Color lineColor = new Color(1f, 0.9f, 0.25f, 1f);

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

            Color color = cell.Kind switch
            {
                BlockKind.Lava => lavaColor,
                BlockKind.Metal => metalColor,
                BlockKind.Line => lineColor,
                _ => normalColor
            };

            spriteRenderer.color = tintOverride.HasValue ? tintOverride.Value : color;
        }
    }
}