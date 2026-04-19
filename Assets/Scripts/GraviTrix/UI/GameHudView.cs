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

        private TextMeshProUGUI rotationArrowText;

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
                SetupUIElement(rt, new Vector2(0f, -280f), new Vector2(480f, 30f), new Vector2(0.5f, 1f));
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

                // ─── Rotation Direction Arrow ───
                CreateRotationArrow(rt);
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

        public void SetRotationProgress(float normalizedProgress, int movesRemaining, bool rotateLeft = true)
        {
            if (rotationProgressBar != null)
            {
                rotationProgressBar.value = Mathf.Clamp01(normalizedProgress);
            }

            if (rotationText != null)
            {
                rotationText.text = movesRemaining > 0 ? $"ROTATION IN {movesRemaining}" : "ROTATING...";
            }

            if (rotationArrowText != null)
            {
                rotationArrowText.text = rotateLeft ? "< < <" : "> > >";
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

        private void CreateRotationArrow(RectTransform progressBarRt)
        {
            GameObject arrowObj = new GameObject("Rotation Arrow");
            arrowObj.transform.SetParent(progressBarRt.parent, false);

            rotationArrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            rotationArrowText.text = "< < <"; // default left
            rotationArrowText.fontSize = 36;
            rotationArrowText.fontStyle = FontStyles.Bold;
            rotationArrowText.alignment = TextAlignmentOptions.Center;
            rotationArrowText.color = new Color(1f, 0.85f, 0.3f); // Golden yellow
            rotationArrowText.enableVertexGradient = true;
            rotationArrowText.colorGradient = new VertexGradient(
                new Color(1f, 0.9f, 0.4f), new Color(1f, 0.9f, 0.4f),
                new Color(1f, 0.6f, 0.2f), new Color(1f, 0.6f, 0.2f));

            RectTransform arrowRt = arrowObj.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(0.5f, 1f);
            arrowRt.anchorMax = new Vector2(0.5f, 1f);
            arrowRt.pivot = new Vector2(0.5f, 1f);
            // Between score (y=-30, h=160) and progress bar (y=-280)
            arrowRt.anchoredPosition = new Vector2(0f, -190f);
            arrowRt.sizeDelta = new Vector2(300f, 50f);
        }
    }
}