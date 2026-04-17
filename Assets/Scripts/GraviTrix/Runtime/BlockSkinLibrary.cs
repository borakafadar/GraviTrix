using UnityEngine;
using GraviTrix.Core;

namespace GraviTrix.Runtime
{
    [CreateAssetMenu(menuName = "GraviTrix/Block Skin Library", fileName = "BlockSkinLibrary")]
    public sealed class BlockSkinLibrary : ScriptableObject
    {
        [SerializeField] private Sprite yellow;
        [SerializeField] private Sprite cyan;
        [SerializeField] private Sprite green;
        [SerializeField] private Sprite blue;
        [SerializeField] private Sprite orange;
        [SerializeField] private Sprite pink;
        [SerializeField] private Sprite red;
        [SerializeField] private Sprite deepBlue;
        [SerializeField] private Sprite metal;
        [SerializeField] private Sprite lava;
        [SerializeField] private Sprite line;

        public Sprite GetSprite(BlockVisualType visualType)
        {
            return visualType switch
            {
                BlockVisualType.Yellow => yellow,
                BlockVisualType.Cyan => cyan,
                BlockVisualType.Green => green,
                BlockVisualType.Blue => blue,
                BlockVisualType.Orange => orange,
                BlockVisualType.Pink => pink,
                BlockVisualType.Red => red,
                BlockVisualType.DeepBlue => deepBlue,
                BlockVisualType.Metal => metal,
                BlockVisualType.Lava => lava,
                BlockVisualType.Line => line,
                _ => yellow
            };
        }
    }
}