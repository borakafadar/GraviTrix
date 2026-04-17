using UnityEngine;

namespace GraviTrix.Core
{
    [CreateAssetMenu(menuName = "GraviTrix/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Board")]
        [SerializeField] private int boardWidth = 12;
        [SerializeField] private int boardHeight = 12;
        [SerializeField] private Vector2Int spawnOrigin = new Vector2Int(5, 0);

        [Header("Timing")]
        [SerializeField] private float normalFallInterval = 0.45f;
        [SerializeField] private float fastFallInterval = 0.15f;
        [SerializeField] private float boardRotationPauseSeconds = 0.35f;

        [Header("Rotation Schedule")]
        [SerializeField] private int minMovesBeforeRotation = 3;
        [SerializeField] private int maxMovesBeforeRotation = 8;

        [Header("Scoring")]
        [SerializeField] private int singleLineScore = 100;
        [SerializeField] private int doubleLineScore = 300;
        [SerializeField] private int tripleLineScore = 500;
        [SerializeField] private int tetrisScore = 800;
        [SerializeField] private float rotationClearBonusMultiplier = 1.5f;

        public int BoardWidth => boardWidth;
        public int BoardHeight => boardHeight;
        public Vector2Int SpawnOrigin => spawnOrigin;
        public float NormalFallInterval => normalFallInterval;
        public float FastFallInterval => fastFallInterval;
        public float BoardRotationPauseSeconds => boardRotationPauseSeconds;
        public int MinMovesBeforeRotation => minMovesBeforeRotation;
        public int MaxMovesBeforeRotation => maxMovesBeforeRotation;
        public float RotationClearBonusMultiplier => rotationClearBonusMultiplier;

        public int GetRandomMovesBeforeRotation()
        {
            int min = Mathf.Max(1, minMovesBeforeRotation);
            int max = Mathf.Max(min, maxMovesBeforeRotation);
            return Random.Range(min, max + 1);
        }

        public int GetLineClearScore(int linesCleared, bool afterRotation)
        {
            if (linesCleared <= 0)
            {
                return 0;
            }

            int baseScore = linesCleared switch
            {
                1 => singleLineScore,
                2 => doubleLineScore,
                3 => tripleLineScore,
                4 => tetrisScore,
                _ => linesCleared * singleLineScore
            };

            if (afterRotation)
            {
                baseScore = Mathf.RoundToInt(baseScore * rotationClearBonusMultiplier);
            }

            return baseScore;
        }
    }
}