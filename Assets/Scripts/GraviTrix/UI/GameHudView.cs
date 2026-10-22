using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraviTrix.UI
{
    /// <summary>
    /// Updates the HUD laid out in MainScene: score, rotation progress bar and arrow,
    /// and spawns the combo/action text prefabs. Layout lives in the scene, not here.
    /// </summary>
    public sealed class GameHudView : MonoBehaviour
    {
        [SerializeField] private Slider rotationProgressBar;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image rotationArrowImage;

        [Header("Prefabs")]
        [SerializeField] private Image tickPrefab;
        [SerializeField] private TextMeshProUGUI comboTextPrefab;
        [SerializeField] private TextMeshProUGUI actionTextPrefab;

        private readonly List<GameObject> tickMarks = new List<GameObject>();
        private readonly List<GameObject> popupTexts = new List<GameObject>();
        private int lastTotalMoves = -1;
        private int lastScore = -1;
        private float targetScaleX = 1f;

        /// <summary>
        /// Stops the combo/action text animations and removes their texts, so the old game's
        /// "GOOD!" or combo doesn't play over a restarted one.
        /// </summary>
        public void ResetView()
        {
            StopAllCoroutines();

            foreach (GameObject text in popupTexts)
            {
                if (text != null) Destroy(text);
            }
            popupTexts.Clear();
        }

        public void SetRotationProgress(float normalizedProgress, int movesRemaining, bool rotateLeft = true, int totalMoves = 0)
        {
            float t = Mathf.Clamp01(normalizedProgress);
            rotationProgressBar.value = t;

            Image fillImage = rotationProgressBar.fillRect.GetComponent<Image>();
            Color green = new Color(0.2f, 0.85f, 0.3f);
            Color yellow = new Color(1f, 0.9f, 0.2f);
            Color red = new Color(1f, 0.2f, 0.15f);
            fillImage.color = t < 0.5f
                ? Color.Lerp(green, yellow, t * 2f)
                : Color.Lerp(yellow, red, (t - 0.5f) * 2f);

            targetScaleX = rotateLeft ? -1f : 1f;

            // Update tick marks on the progress bar
            if (totalMoves > 0 && totalMoves != lastTotalMoves)
            {
                RebuildTickMarks(totalMoves);
                lastTotalMoves = totalMoves;
            }
        }

        private void Update()
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

        public void SetScore(int score)
        {
            if (score == lastScore)
            {
                return;
            }

            lastScore = score;
            scoreText.text = $"SCORE\n<size=120%>{score}</size>";
        }

        // ─── Progress Bar Tick Marks ───

        private void RebuildTickMarks(int totalMoves)
        {
            foreach (var tick in tickMarks)
            {
                if (tick != null) Destroy(tick);
            }
            tickMarks.Clear();

            if (totalMoves <= 1) return;

            RectTransform barRt = (RectTransform)rotationProgressBar.transform;

            // Create a tick for each step (skip first at 0 and last at end)
            for (int i = 1; i < totalMoves; i++)
            {
                float t = (float)i / totalMoves;
                Image tick = Instantiate(tickPrefab, barRt);
                tick.rectTransform.anchorMin = new Vector2(t, 0f);
                tick.rectTransform.anchorMax = new Vector2(t, 1f);
                tickMarks.Add(tick.gameObject);
            }
        }

        public void ShowComboText(int comboCount)
        {
            string message = "";
            if (comboCount == 2) message = "NICE!";
            else if (comboCount == 3) message = "GREAT!";
            else if (comboCount == 4) message = "AWESOME!";
            else if (comboCount == 5) message = "PERFECT!";
            else if (comboCount >= 6) message = "AMAZING!";

            if (string.IsNullOrEmpty(message)) return;

            TextMeshProUGUI tmp = Instantiate(comboTextPrefab, scoreText.transform.parent);
            tmp.text = $"{message}\n<size=50%>{comboCount}x COMBO!</size>";
            popupTexts.Add(tmp.gameObject);
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

            popupTexts.Remove(tmp.gameObject);
            Destroy(tmp.gameObject);
        }

        public void ShowActionText(string message)
        {
            TextMeshProUGUI tmp = Instantiate(actionTextPrefab, scoreText.transform.parent);
            tmp.text = message;
            popupTexts.Add(tmp.gameObject);
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

            popupTexts.Remove(tmp.gameObject);
            Destroy(tmp.gameObject);
        }
    }
}
