using System;
using UnityEngine;

namespace GraviTrix.Core
{
    public enum BlockVisualType
    {
        Auto = 0,
        Yellow = 1,
        Cyan = 2,
        Green = 3,
        Blue = 4,
        Orange = 5,
        Pink = 6,
        Red = 7,
        DeepBlue = 8,
        Metal = 9,
        Lava = 10,
        Line = 11
    }

    public enum BlockKind
    {
        Normal = 0,
        Lava = 1,
        Metal = 2,
        Line = 3
    }

    public enum GamePhase
    {
        Spawning = 0,
        Falling = 1,
        RotatingBoard = 2,
        GameOver = 3,
        ClearingLines = 4
    }

    [Serializable]
    public struct BlockCellDefinition
    {
        public Vector2Int LocalPosition;
        public BlockKind Kind;
        public BlockVisualType VisualType;
    }

    [Serializable]
    public struct BlockCellState
    {
        public Vector2Int LocalPosition;
        public BlockKind Kind;
        public BlockVisualType VisualType;

        public BlockCellState(Vector2Int localPosition, BlockKind kind, BlockVisualType visualType)
        {
            LocalPosition = localPosition;
            Kind = kind;
            VisualType = visualType;
        }
    }

    [Serializable]
    public struct BlockCellInfo
    {
        public Vector2Int Position;
        public BlockKind Kind;
        public BlockVisualType VisualType;

        public BlockCellInfo(Vector2Int position, BlockKind kind, BlockVisualType visualType)
        {
            Position = position;
            Kind = kind;
            VisualType = visualType;
        }
    }

    [Serializable]
    public struct CellOccupant
    {
        public BlockKind Kind;
        public BlockVisualType VisualType;

        public CellOccupant(BlockKind kind, BlockVisualType visualType)
        {
            Kind = kind;
            VisualType = visualType;
        }
    }

    public static class BlockVisualTypeResolver
    {
        public static BlockVisualType Resolve(BlockKind kind, BlockVisualType requestedVisual)
        {
            if (requestedVisual != BlockVisualType.Auto)
            {
                return requestedVisual;
            }

            return kind switch
            {
                BlockKind.Lava => BlockVisualType.Lava,
                BlockKind.Metal => BlockVisualType.Metal,
                BlockKind.Line => BlockVisualType.Line,
                _ => BlockVisualType.Yellow
            };
        }
    }
}