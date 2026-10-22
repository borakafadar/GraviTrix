using UnityEngine;

namespace GraviTrix.Core
{
    [CreateAssetMenu(menuName = "GraviTrix/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Board")]
        [Tooltip("Where new pieces appear on the 12x12 board (y grows downward). x is kept within 1-9 so every piece fits.")]
        [SerializeField] internal Vector2Int spawnOrigin = new Vector2Int(5, 0);

        [Header("Timing")]
        [SerializeField] private float normalFallInterval = 0.45f;
        [SerializeField] private float fastFallInterval = 0.15f;

        [Header("Rotation Schedule")]
        [SerializeField] private int minMovesBeforeRotation = 4;
        [SerializeField] private int maxMovesBeforeRotation = 8;

        [Header("Scoring")]
        [SerializeField] private int singleLineScore = 100;
        [SerializeField] private int doubleLineScore = 300;
        [SerializeField] private int tripleLineScore = 500;
        [SerializeField] private int tetrisScore = 800;
        [SerializeField] private float rotationClearBonusMultiplier = 1.5f;
        [SerializeField] private int comboBonus = 50;
        [SerializeField] private int lavaMeltScore = 10;
        [Tooltip("Points per block removed by a line clear, line block or metal explosion.")]
        [SerializeField] internal int pointsPerBlock = 10;
        [Tooltip("Extra points for a metal explosion, on top of pointsPerBlock for every block it removes.")]
        [SerializeField] internal int bombBonus = 1000;

        public Vector2Int SpawnOrigin => spawnOrigin;
        public float NormalFallInterval => normalFallInterval;
        public float FastFallInterval => fastFallInterval;
        public float RotationClearBonusMultiplier => rotationClearBonusMultiplier;
        public int ComboBonus => comboBonus;
        public int LavaMeltScore => lavaMeltScore;
        public int PointsPerBlock => pointsPerBlock;
        public int BombBonus => bombBonus;

        internal void OnValidate()
        {
            // The I piece spans x-1..x+2, so x must stay within 1..9 on the 12-wide board.
            spawnOrigin = new Vector2Int(Mathf.Clamp(spawnOrigin.x, 1, 9), Mathf.Max(0, spawnOrigin.y));
        }

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