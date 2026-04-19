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
        
        private float targetScaleX = 1f;

        private void Awake()
        {
            // Auto-configure Canvas Scaler for responsive UI
            Canvas localCanvas = GetComponent<Canvas>();
            if (localCanvas == null) localCanvas = GetComponentInChildren<Canvas>();
            if (localCanvas == null) localCanvas = GetComponentInParent<Canvas>();

            if (localCanvas != null)
            {
                CanvasScaler scaler = localCanvas.GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = localCanvas.gameObject.AddComponent<CanvasScaler>();
                }
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (scoreText != null)
            {
                SetupUIElement(scoreText.transform as RectTransform, new Vector2(0f, -110f), new Vector2(600f, 160f), new Vector2(0.5f, 1f));
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
                SetupUIElement(rt, new Vector2(0f, -435f), new Vector2(480f, 26f), new Vector2(0.5f, 1f));
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
                targetScaleX = rotateLeft ? -1f : 1f;
            }

            // Update tick marks on the progress bar
            if (totalMoves > 0 && totalMoves != lastTotalMoves)
            {
                RebuildTickMarks(totalMoves);
                lastTotalMoves = totalMoves;
            }
        }

        private void Update()
        {
            if (rotationArrowImage != null)
            {
                Vector3 currentScale = rotationArrowImage.rectTransform.localScale;
                if (!Mathf.Approximately(currentScale.x, targetScaleX))
                {
                    // Animate the scale to create a horizontal flipping effect
                    currentScale.x = Mathf.Lerp(currentScale.x, targetScaleX, Time.deltaTime * 10f);
                    
                    if (Mathf.Abs(currentScale.x - targetScaleX) < 0.01f)
                    {
                        currentScale.x = targetScaleX;
                    }
                    
                    rotationArrowImage.rectTransform.localScale = currentScale;
                }
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
            arrowRt.anchoredPosition = new Vector2(0f, -340f);
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

        public void ShowComboText(int comboCount)
        {
            if (scoreText == null) return;
            
            string message = "";
            if (comboCount == 2) message = "NICE!";
            else if (comboCount == 3) message = "GREAT!";
            else if (comboCount == 4) message = "AWESOME!";
            else if (comboCount == 5) message = "PERFECT!";
            else if (comboCount >= 6) message = "AMAZING!";
            
            if (string.IsNullOrEmpty(message)) return;
            
            GameObject textObj = new GameObject("ComboText");
            textObj.transform.SetParent(scoreText.transform.parent, false);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{message}\n<size=50%>{comboCount}x COMBO!</size>";
            tmp.fontSize = 80;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableVertexGradient = true;
            tmp.colorGradient = new VertexGradient(new Color(1f, 0.8f, 0f), new Color(1f, 0.5f, 0f), new Color(1f, 0.2f, 0f), new Color(1f, 0f, 0f));
            
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(600f, 200f);
            
            // Add a little drop shadow for readability
            UnityEngine.UI.Shadow shadow = textObj.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(3f, -3f);
            
            StartCoroutine(AnimateComboText(tmp));
        }

        private System.Collections.IEnumerator AnimateComboText(TextMeshProUGUI tmp)
        {
            float duration = 1.2f;
            float elapsed = 0f;
            RectTransform rt = tmp.rectTransform;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Scale animation (pop out then settle)
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
                rt.localScale = new Vector3(scale, scale, 1f);
                
                // Float up
                rt.anchoredPosition = new Vector2(0f, t * 150f);
                
                // Fade out at the end
                if (t > 0.7f)
                {
                    float alpha = 1f - ((t - 0.7f) / 0.3f);
                    tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);
                }
                
                yield return null;
            }
            
            Destroy(tmp.gameObject);
        }

        public void ShowActionText(string message)
        {
            if (scoreText == null) return;

            GameObject textObj = new GameObject("ActionText");
            textObj.transform.SetParent(scoreText.transform.parent, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.fontSize = 96;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.92f, 0.016f); // Bright yellow
            tmp.enableVertexGradient = true;
            tmp.colorGradient = new VertexGradient(
                new Color(1f, 1f, 0.4f),   // top-left: light yellow
                new Color(1f, 1f, 0.4f),   // top-right: light yellow
                new Color(1f, 0.85f, 0f),  // bottom-left: golden yellow
                new Color(1f, 0.85f, 0f)   // bottom-right: golden yellow
            );

            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800f, 200f);

            UnityEngine.UI.Outline outline = textObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.6f, 0.4f, 0f, 0.8f);
            outline.effectDistance = new Vector2(3f, -3f);

            StartCoroutine(AnimateActionText(tmp));
        }

        private System.Collections.IEnumerator AnimateActionText(TextMeshProUGUI tmp)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            RectTransform rt = tmp.rectTransform;

            // Start small
            rt.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Pop-in with overshoot, then settle
                float scale;
                if (t < 0.2f)
                {
                    // Quick scale up with overshoot
                    float easeT = t / 0.2f;
                    scale = Mathf.LerpUnclamped(0f, 1.3f, easeT);
                }
                else if (t < 0.35f)
                {
                    // Settle back down
                    float easeT = (t - 0.2f) / 0.15f;
                    scale = Mathf.Lerp(1.3f, 1f, easeT);
                }
                else
                {
                    scale = 1f;
                }

                rt.localScale = new Vector3(scale, scale, 1f);

                // Float up slowly
                rt.anchoredPosition = new Vector2(0f, t * 100f);

                // Fade out near the end
                if (t > 0.75f)
                {
                    float alpha = 1f - ((t - 0.75f) / 0.25f);
                    tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);
                }

                yield return null;
            }

            Destroy(tmp.gameObject);
        }
    }
}