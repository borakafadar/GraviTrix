using System.Collections.Generic;
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

        private Image rotationArrowImage;
        private List<GameObject> tickMarks = new List<GameObject>();
        private int lastTotalMoves = -1;

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
                rotationText.gameObject.SetActive(false); // Hide rotation text, arrow is enough
            }
            
            if (rotationProgressBar != null)
            {
                RectTransform rt = rotationProgressBar.transform as RectTransform;
                // Progress bar sits between the arrow and the grid
                SetupUIElement(rt, new Vector2(0f, -340f), new Vector2(480f, 26f), new Vector2(0.5f, 1f));
                rt.localRotation = Quaternion.identity;

                if (rotationProgressBar.fillRect != null)
                {
                    Image fillImage = rotationProgressBar.fillRect.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        // Use a solid white texture so we can color it dynamically
                        Texture2D whiteTex = new Texture2D(1, 1);
                        whiteTex.SetPixel(0, 0, Color.white);
                        whiteTex.Apply();
                        Sprite whiteSprite = Sprite.Create(whiteTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                        fillImage.sprite = whiteSprite;
                        fillImage.color = new Color(0.2f, 0.85f, 0.3f); // Initial green
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

        public void SetRotationProgress(float normalizedProgress, int movesRemaining, bool rotateLeft = true, int totalMoves = 0)
        {
            if (rotationProgressBar != null)
            {
                rotationProgressBar.value = Mathf.Clamp01(normalizedProgress);
                
                if (rotationProgressBar.fillRect != null)
                {
                    Image fillImage = rotationProgressBar.fillRect.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        Color green = new Color(0.2f, 0.85f, 0.3f);
                        Color yellow = new Color(1f, 0.9f, 0.2f);
                        Color red = new Color(1f, 0.2f, 0.15f);
                        
                        float t = Mathf.Clamp01(normalizedProgress);
                        fillImage.color = t < 0.5f 
                            ? Color.Lerp(green, yellow, t * 2f) 
                            : Color.Lerp(yellow, red, (t - 0.5f) * 2f);
                    }
                }
            }

            if (rotationText != null)
            {
                rotationText.text = movesRemaining > 0 ? $"ROTATION IN {movesRemaining}" : "ROTATING...";
            }

            if (rotationArrowImage != null)
            {
                // Flip horizontally for left rotation (sprite is drawn pointing right)
                Vector3 scale = rotationArrowImage.rectTransform.localScale;
                scale.x = rotateLeft ? -1f : 1f;
                rotationArrowImage.rectTransform.localScale = scale;
            }

            // Update tick marks on the progress bar
            if (totalMoves > 0 && totalMoves != lastTotalMoves)
            {
                RebuildTickMarks(totalMoves);
                lastTotalMoves = totalMoves;
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

        [SerializeField] private Sprite rotationArrowSprite;

        private void CreateRotationArrow(RectTransform progressBarRt)
        {
            Sprite arrowSprite = rotationArrowSprite;

            // Editor fallback: load from asset path if not assigned
            if (arrowSprite == null)
            {
#if UNITY_EDITOR
                arrowSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Buttons/rotationArrow.png");
#endif
            }

            if (arrowSprite == null) return;

            GameObject arrowObj = new GameObject("Rotation Arrow");
            arrowObj.transform.SetParent(progressBarRt.parent, false);

            rotationArrowImage = arrowObj.AddComponent<Image>();
            rotationArrowImage.sprite = arrowSprite;
            rotationArrowImage.type = Image.Type.Simple;
            rotationArrowImage.raycastTarget = false;
            rotationArrowImage.preserveAspect = true;

            RectTransform arrowRt = arrowObj.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(0.5f, 1f);
            arrowRt.anchorMax = new Vector2(0.5f, 1f);
            arrowRt.pivot = new Vector2(0.5f, 0.5f);
            // Between score bottom (~-190) and progress bar (-340)
            arrowRt.anchoredPosition = new Vector2(0f, -260f);
            arrowRt.sizeDelta = new Vector2(120f, 120f);
        }

        // ─── Progress Bar Tick Marks ───

        private void RebuildTickMarks(int totalMoves)
        {
            // Clear old tick marks
            foreach (var tick in tickMarks)
            {
                if (tick != null) Destroy(tick);
            }
            tickMarks.Clear();

            if (rotationProgressBar == null || totalMoves <= 1) return;

            RectTransform barRt = rotationProgressBar.transform as RectTransform;
            float barWidth = barRt.sizeDelta.x;
            float barHeight = barRt.sizeDelta.y;

            // Create a tick for each step (skip first at 0 and last at end)
            for (int i = 1; i < totalMoves; i++)
            {
                float t = (float)i / totalMoves;

                GameObject tickObj = new GameObject($"Tick {i}");
                tickObj.transform.SetParent(barRt, false);

                Image tickImage = tickObj.AddComponent<Image>();
                tickImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                tickImage.raycastTarget = false;

                RectTransform tickRt = tickObj.GetComponent<RectTransform>();
                tickRt.anchorMin = new Vector2(t, 0f);
                tickRt.anchorMax = new Vector2(t, 1f);
                tickRt.pivot = new Vector2(0.5f, 0.5f);
                tickRt.sizeDelta = new Vector2(4f, barHeight * 0.5f);

                tickMarks.Add(tickObj);
            }
        }
    }
}