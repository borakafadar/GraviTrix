using System.Collections;
using GraviTrix.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraviTrix.UI
{
    /// <summary>
    /// Programmatically builds and shows a Game Over popup with Retry and Main Menu buttons.
    /// Attach to any GameObject in the game scene, or let GameController spawn it.
    /// </summary>
    public sealed class GameOverPopup : MonoBehaviour
    {
        private const string HighScoreKey = "HighScore";
        private const string HomeSceneName = "HomeScene";

        [SerializeField] private GameController gameController;

        private Canvas popupCanvas;
        private CanvasGroup canvasGroup;
        private GameObject popupRoot;
        private Text scoreValueText;
        private Text newRecordText;

        /// <summary>
        /// Show the game over popup with the given final score.
        /// </summary>
        public void Show(int finalScore)
        {
            if (popupRoot == null)
            {
                BuildPopup();
            }

            // Update & check high score
            int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            bool isNewRecord = finalScore > highScore;
            if (isNewRecord)
            {
                PlayerPrefs.SetInt(HighScoreKey, finalScore);
                PlayerPrefs.Save();
            }

            if (scoreValueText != null)
            {
                scoreValueText.text = finalScore.ToString("N0");
            }

            if (newRecordText != null)
            {
                newRecordText.gameObject.SetActive(isNewRecord);
            }

            popupRoot.SetActive(true);
            StartCoroutine(FadeIn(0.5f));
        }

        /// <summary>
        /// Hide the popup (used before retry).
        /// </summary>
        public void Hide()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        private void BuildPopup()
        {
            // ─── Canvas ───
            GameObject canvasObj = new GameObject("GameOverPopup Canvas");
            canvasObj.transform.SetParent(transform, false);

            popupCanvas = canvasObj.AddComponent<Canvas>();
            popupCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            popupCanvas.worldCamera = Camera.main;
            popupCanvas.planeDistance = 5f;
            popupCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            popupRoot = canvasObj;

            // ─── Dark Overlay ───
            GameObject overlayObj = new GameObject("Dark Overlay");
            overlayObj.transform.SetParent(canvasObj.transform, false);

            Image overlay = overlayObj.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.65f);
            overlay.raycastTarget = true;

            RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            // ─── Navy Blue Rounded Border Frame ───
            GameObject borderObj = new GameObject("Blue Border");
            borderObj.transform.SetParent(canvasObj.transform, false);

            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.sprite = CreateRoundedPanelSprite(740, 870, 46);
            borderImage.type = Image.Type.Simple;
            borderImage.color = new Color32(20, 40, 120, 255);
            borderImage.raycastTarget = false;

            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0.5f, 0.5f);
            borderRect.anchorMax = new Vector2(0.5f, 0.5f);
            borderRect.pivot = new Vector2(0.5f, 0.5f);
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.sizeDelta = new Vector2(740f, 870f);

            // ─── Panel ───
            GameObject panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.sprite = CreateRoundedPanelSprite(720, 850, 42);
            panelImage.type = Image.Type.Simple;

            Shadow panelShadow = panelObj.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            panelShadow.effectDistance = new Vector2(0f, -12f);

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(720f, 850f);

            // ─── "GAME OVER" Title ───
            CreateTitle(panelObj.transform);

            // ─── Score Label ───
            CreateScoreLabel(panelObj.transform);

            // ─── Score Value ───
            CreateScoreValue(panelObj.transform);

            // ─── NEW RECORD text ───
            CreateNewRecordLabel(panelObj.transform);

            // ─── Buttons ───
            CreatePopupButton(panelObj.transform, "RETRY", new Vector2(0f, -170f), OnRetryClicked);
            CreatePopupButton(panelObj.transform, "MAIN MENU", new Vector2(0f, -295f), OnMainMenuClicked);
        }

        private void CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title - GAME OVER");
            titleObj.transform.SetParent(parent, false);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "GAME OVER";
            titleText.font = GetDefaultFont();
            titleText.fontSize = 82;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color32(255, 80, 80, 255);

            Shadow shadow = titleObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(4f, -4f);

            Outline outline = titleObj.AddComponent<Outline>();
            outline.effectColor = new Color32(120, 20, 20, 180);
            outline.effectDistance = new Vector2(2f, -2f);

            RectTransform rect = titleObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 310f);
            rect.sizeDelta = new Vector2(650f, 120f);
        }

        private void CreateScoreLabel(Transform parent)
        {
            GameObject labelObj = new GameObject("Score Label");
            labelObj.transform.SetParent(parent, false);

            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = "YOUR SCORE";
            labelText.font = GetDefaultFont();
            labelText.fontSize = 36;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = new Color32(200, 200, 220, 255);

            RectTransform rect = labelObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 180f);
            rect.sizeDelta = new Vector2(600f, 60f);
        }

        private void CreateScoreValue(Transform parent)
        {
            GameObject valueObj = new GameObject("Score Value");
            valueObj.transform.SetParent(parent, false);

            scoreValueText = valueObj.AddComponent<Text>();
            scoreValueText.text = "0";
            scoreValueText.font = GetDefaultFont();
            scoreValueText.fontSize = 96;
            scoreValueText.fontStyle = FontStyle.Bold;
            scoreValueText.alignment = TextAnchor.MiddleCenter;
            scoreValueText.color = new Color32(255, 225, 80, 255);

            Shadow shadow = valueObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(3f, -3f);

            RectTransform rect = valueObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 80f);
            rect.sizeDelta = new Vector2(600f, 130f);
        }

        private void CreateNewRecordLabel(Transform parent)
        {
            GameObject recordObj = new GameObject("New Record Label");
            recordObj.transform.SetParent(parent, false);

            newRecordText = recordObj.AddComponent<Text>();
            newRecordText.text = "★ NEW RECORD! ★";
            newRecordText.font = GetDefaultFont();
            newRecordText.fontSize = 42;
            newRecordText.fontStyle = FontStyle.Bold;
            newRecordText.alignment = TextAnchor.MiddleCenter;
            newRecordText.color = new Color32(255, 200, 50, 255);

            Shadow shadow = recordObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.4f, 0.2f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);

            // Pulse animation via a helper
            recordObj.AddComponent<PulseEffect>();

            RectTransform rect = recordObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(600f, 70f);

            recordObj.SetActive(false);
        }

        private void CreatePopupButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction clickAction)
        {
            GameObject buttonObj = new GameObject("Button - " + label);
            buttonObj.transform.SetParent(parent, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(520f, 100f);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.sprite = CreateRoundedButtonSprite(520, 100, 28);
            buttonImage.type = Image.Type.Simple;
            buttonImage.raycastTarget = true;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(clickAction);

            Shadow buttonShadow = buttonObj.AddComponent<Shadow>();
            buttonShadow.effectColor = new Color(0.15f, 0.07f, 0.02f, 0.35f);
            buttonShadow.effectDistance = new Vector2(0f, -6f);

            // Dark hover overlay
            GameObject darkOverlayObj = new GameObject("Hover Dark Overlay");
            darkOverlayObj.transform.SetParent(buttonObj.transform, false);

            Image darkOverlay = darkOverlayObj.AddComponent<Image>();
            darkOverlay.sprite = buttonImage.sprite;
            darkOverlay.type = Image.Type.Simple;
            darkOverlay.color = new Color(0f, 0f, 0f, 0f);
            darkOverlay.raycastTarget = false;

            RectTransform overlayRect = darkOverlayObj.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            // Button text
            GameObject textObj = new GameObject("Text - " + label);
            textObj.transform.SetParent(buttonObj.transform, false);

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = label;
            buttonText.font = GetDefaultFont();
            buttonText.fontSize = 40;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = new Color32(75, 42, 22, 255);
            buttonText.raycastTarget = false;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Add hover effect if MainMenuButtonEffect exists
            MainMenuButtonEffect effect = buttonObj.AddComponent<MainMenuButtonEffect>();
            effect.SetOverlay(darkOverlay);

            darkOverlayObj.transform.SetAsLastSibling();
            textObj.transform.SetAsLastSibling();
        }

        // ─── Button Callbacks ───

        private void OnRetryClicked()
        {
            Hide();

            if (gameController != null)
            {
                gameController.RestartGame();
            }
            else
            {
                // Fallback: find it in the scene
                GameController controller = FindObjectOfType<GameController>();
                if (controller != null)
                {
                    controller.RestartGame();
                }
                else
                {
                    // Last resort: reload the current scene
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(HomeSceneName);
        }

        // ─── Fade-in Animation ───

        private IEnumerator FadeIn(float duration)
        {
            if (canvasGroup == null) yield break;

            canvasGroup.alpha = 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Ease-out cubic
                canvasGroup.alpha = 1f - Mathf.Pow(1f - t, 3f);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        // ─── Sprite Generation (matching MainMenuBuilder style) ───

        private Sprite CreateRoundedPanelSprite(int width, int height, int radius)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Bilinear;

            Color centerColor = new Color32(30, 50, 110, 250);
            Color edgeColor = new Color32(18, 30, 75, 250);
            Color borderColor = new Color32(50, 90, 180, 255);

            Vector2 center = new Vector2(width / 2f, height / 2f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!IsInsideRoundedRect(x, y, width, height, radius))
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    bool isBorder = !IsInsideRoundedRect(x, y, width, height, radius - 3);
                    if (isBorder)
                    {
                        texture.SetPixel(x, y, borderColor);
                        continue;
                    }

                    float distFromCenter = Vector2.Distance(new Vector2(x, y), center) / (width * 0.5f);
                    Color bgColor = Color.Lerp(centerColor, edgeColor, distFromCenter * 0.8f);

                    // Subtle highlight at top
                    float topHighlight = Mathf.Clamp01((float)(height - y) / (height * 0.15f));
                    bgColor = Color.Lerp(bgColor, new Color32(100, 70, 170, 250), topHighlight * 0.15f);

                    texture.SetPixel(x, y, bgColor);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private Sprite CreateRoundedButtonSprite(int width, int height, int radius)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Bilinear;

            Color leftColor = new Color32(255, 220, 45, 255);
            Color rightColor = new Color32(235, 135, 22, 255);
            Color borderColor = new Color32(155, 82, 18, 255);
            Color highlightColor = new Color32(255, 238, 105, 255);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!IsInsideRoundedRect(x, y, width, height, radius))
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    bool isBorder = !IsInsideRoundedRect(x, y, width, height, radius - 3);
                    if (isBorder)
                    {
                        texture.SetPixel(x, y, borderColor);
                        continue;
                    }

                    float gradientT = (float)x / (width - 1);
                    Color baseColor = Color.Lerp(leftColor, rightColor, gradientT);

                    // Top highlight
                    float highlightT = Mathf.Clamp01((float)(height - y) / (height * 0.25f));
                    baseColor = Color.Lerp(baseColor, highlightColor, highlightT * 0.35f);

                    texture.SetPixel(x, y, baseColor);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        private bool IsInsideRoundedRect(int px, int py, int width, int height, int radius)
        {
            if (radius <= 0) return px >= 0 && px < width && py >= 0 && py < height;

            // Check corners
            if (px < radius && py < radius)
                return Vector2.Distance(new Vector2(px, py), new Vector2(radius, radius)) <= radius;
            if (px >= width - radius && py < radius)
                return Vector2.Distance(new Vector2(px, py), new Vector2(width - radius - 1, radius)) <= radius;
            if (px < radius && py >= height - radius)
                return Vector2.Distance(new Vector2(px, py), new Vector2(radius, height - radius - 1)) <= radius;
            if (px >= width - radius && py >= height - radius)
                return Vector2.Distance(new Vector2(px, py), new Vector2(width - radius - 1, height - radius - 1)) <= radius;

            return px >= 0 && px < width && py >= 0 && py < height;
        }

        private Font GetDefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

    /// <summary>
    /// Simple pulse scale animation for the "NEW RECORD" label.
    /// </summary>
    public sealed class PulseEffect : MonoBehaviour
    {
        private Vector3 baseScale;
        private float timer;

        private void Start()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            float pulse = 1f + 0.08f * Mathf.Sin(timer * 4f);
            transform.localScale = baseScale * pulse;
        }
    }
}
