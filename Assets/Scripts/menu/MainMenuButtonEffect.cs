using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtonEffect : MonoBehaviour
{
    private Image darkOverlay;
    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = new Vector3(1.035f, 1.035f, 1f);
    private Vector3 pressedScale = new Vector3(0.94f, 0.94f, 1f);

    private float normalDarkness = 0f;
    private float hoverDarkness = 0.18f;
    private float pressedDarkness = 0.35f;

    private float animationSpeed = 14f;

    public void SetOverlay(Image overlay)
    {
        darkOverlay = overlay;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (rectTransform == null || darkOverlay == null)
        {
            return;
        }

        Camera uiCamera = null;

        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = parentCanvas.worldCamera;
        }

        bool isMouseOver = RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            Input.mousePosition,
            uiCamera
        );

        float targetDarkness = normalDarkness;
        Vector3 targetScale = normalScale;

        if (isMouseOver)
        {
            targetDarkness = hoverDarkness;
            targetScale = hoverScale;

            if (Input.GetMouseButton(0))
            {
                targetDarkness = pressedDarkness;
                targetScale = pressedScale;
            }
        }

        Color currentColor = darkOverlay.color;
        Color targetColor = new Color(0f, 0f, 0f, targetDarkness);

        darkOverlay.color = Color.Lerp(
            currentColor,
            targetColor,
            Time.unscaledDeltaTime * animationSpeed
        );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );
    }
}
