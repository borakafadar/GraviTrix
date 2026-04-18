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
            if (scoreText != null)
            {
                SetupUIElement(scoreText.transform as RectTransform, new Vector2(0f, -30f), new Vector2(600f, 160f), new Vector2(0.5f, 1f));
                scoreText.fontSize = 52;
                scoreText.fontStyle = FontStyles.Bold;
                scoreText.enableVertexGradient = true;
                scoreText.colorGradient = new VertexGradient(Color.white, Color.white, new Color(0.4f, 0.8f, 1f), new Color(0.2f, 0.6f, 1f));
                scoreText.alignment = TextAlignmentOptions.Center;
                scoreText.lineSpacing = -10f;
            }
            
            if (stateText != null)
            {
                stateText.gameObject.SetActive(false); // Hide the falling state text
            }
            
            if (rotationText != null)
            {
                RectTransform rt = rotationText.transform as RectTransform;
                SetupUIElement(rt, new Vector2(0f, -220f), new Vector2(600f, 50f), new Vector2(0.5f, 1f));
                rotationText.fontSize = 24;
                rotationText.fontStyle = FontStyles.Bold;
                rotationText.enableVertexGradient = true;
                rotationText.colorGradient = new VertexGradient(Color.white, Color.white, new Color(1f, 0.4f, 0.8f), new Color(0.8f, 0.2f, 0.6f));
                rotationText.alignment = TextAlignmentOptions.Center;
            }
            
            if (rotationProgressBar != null)
            {
                RectTransform rt = rotationProgressBar.transform as RectTransform;
                // Make it horizontal and roughly grid width (e.g. 480f)
                SetupUIElement(rt, new Vector2(0f, -260f), new Vector2(480f, 20f), new Vector2(0.5f, 1f));
                rt.localRotation = Quaternion.identity; // Reset rotation

                if (rotationProgressBar.fillRect != null)
                {
                    Image fillImage = rotationProgressBar.fillRect.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        // Generate a static gradient texture (Blue to Pink)
                        Texture2D gradTex = new Texture2D(2, 1);
                        gradTex.wrapMode = TextureWrapMode.Clamp;
                        gradTex.SetPixel(0, 0, new Color(0.2f, 0.6f, 1f)); // Light Blue
                        gradTex.SetPixel(1, 0, new Color(1f, 0.4f, 0.8f)); // Pink
                        gradTex.Apply();
                        
                        Sprite gradSprite = Sprite.Create(gradTex, new Rect(0, 0, 2, 1), new Vector2(0.5f, 0.5f));
                        fillImage.sprite = gradSprite;
                        fillImage.color = Color.white;
                        fillImage.type = Image.Type.Simple;
                    }
                }
            }
        }

        private void SetupUIElement(RectTransform rt, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 anchor)
        {
            if (rt != null)
            {
                rt.anchorMin = anchor;
                rt.anchorMax = anchor;
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = anchoredPosition;
                rt.sizeDelta = sizeDelta;
            }
        }

        public void SetRotationProgress(float normalizedProgress, int movesRemaining)
        {
            if (rotationProgressBar != null)
            {
                rotationProgressBar.value = Mathf.Clamp01(normalizedProgress);
            }

            if (rotationText != null)
            {
                rotationText.text = movesRemaining > 0 ? $"ROTATION IN {movesRemaining}" : "ROTATING...";
            }
        }

        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE\n<size=120%>{score}</size>";
            }
        }

        public void SetState(string state)
        {
            // State text is disabled, do nothing
        }
    }
}