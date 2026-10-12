using UnityEngine;

namespace GraviTrix.UI
{
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
