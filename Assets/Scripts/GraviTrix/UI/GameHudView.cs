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

        private void Awake()
        {
            float yOffset = -20f;
            if (scoreText != null) yOffset = FixRectTransformAnchor(scoreText.transform as RectTransform, yOffset);
            if (stateText != null) yOffset = FixRectTransformAnchor(stateText.transform as RectTransform, yOffset);
            if (rotationText != null) yOffset = FixRectTransformAnchor(rotationText.transform as RectTransform, yOffset);
            if (rotationProgressBar != null) FixRectTransformAnchor(rotationProgressBar.transform as RectTransform, yOffset);
        }

        private float FixRectTransformAnchor(RectTransform rt, float yOffset)
        {
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, yOffset);
                return yOffset - rt.rect.height - 10f;
            }
            return yOffset;
        }

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