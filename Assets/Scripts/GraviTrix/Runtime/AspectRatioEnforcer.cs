using UnityEngine;

namespace GraviTrix.Runtime
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-100)]
    public class AspectRatioEnforcer : MonoBehaviour
    {
        // Strictly enforce 9:16 aspect ratio
        public float targetAspect = 9.0f / 16.0f;

        private Camera cam;
        private float lastAspect = 0f;

        void Awake()
        {
            cam = GetComponent<Camera>();
            UpdateAspect();
        }

        void Update()
        {
            float currentAspect = (float)Screen.width / (float)Screen.height;
            if (Mathf.Abs(currentAspect - lastAspect) > 0.001f)
            {
                UpdateAspect();
                lastAspect = currentAspect;
            }
        }

        void UpdateAspect()
        {
            if (cam == null) return;

            float windowAspect = (float)Screen.width / (float)Screen.height;
            float scaleHeight = windowAspect / targetAspect;

            if (scaleHeight < 1.0f)
            {
                Rect rect = cam.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
                cam.rect = rect;
            }
            else
            {
                float scaleWidth = 1.0f / scaleHeight;
                Rect rect = cam.rect;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
                cam.rect = rect;
            }
        }
    }
}
