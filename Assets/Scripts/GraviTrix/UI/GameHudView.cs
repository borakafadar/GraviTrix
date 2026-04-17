using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraviTrix.UI
{
    public sealed class GameHudView : MonoBehaviour
    {
        [SerializeField] private Slider rotationProgressBar;
        [SerializeField] private TextMeshProUGUI rotationText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI stateText;

        public void SetRotationProgress(float normalizedProgress, int movesRemaining)
        {
            if (rotationProgressBar != null)
            {
                rotationProgressBar.value = Mathf.Clamp01(normalizedProgress);
            }

            if (rotationText != null)
            {
                rotationText.text = movesRemaining > 0 ? $"Rotation in {movesRemaining} moves" : "Rotating board";
            }
        }

        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void SetState(string state)
        {
            if (stateText != null)
            {
                stateText.text = state;
            }
        }
    }
}