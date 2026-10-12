using System.Collections;
using GraviTrix.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraviTrix.UI
{
    /// <summary>
    /// Shows the Game Over popup authored in MainScene, with Retry and Main Menu buttons.
    /// </summary>
    public sealed class GameOverPopup : MonoBehaviour
    {
        private const string HighScoreKey = "HighScore";
        private const string HomeSceneName = "HomeScene";

        [SerializeField] private GameController gameController;

        [Header("Scene References")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text scoreValueText;
        [SerializeField] private GameObject newRecordText;

        /// <summary>
        /// Show the game over popup with the given final score.
        /// </summary>
        public void Show(int finalScore)
        {
            // Update & check high score
            int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            bool isNewRecord = finalScore > highScore;
            if (isNewRecord)
            {
                PlayerPrefs.SetInt(HighScoreKey, finalScore);
                PlayerPrefs.Save();
            }

            scoreValueText.text = finalScore.ToString("N0");
            newRecordText.SetActive(isNewRecord);

            popupRoot.SetActive(true);
            StartCoroutine(FadeIn(0.5f));
        }

        /// <summary>
        /// Hide the popup (used before retry).
        /// </summary>
        public void Hide()
        {
            popupRoot.SetActive(false);
        }

        // ─── Button Callbacks (wired in the Inspector) ───

        public void OnRetryClicked()
        {
            Hide();
            gameController.RestartGame();
        }

        public void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(HomeSceneName);
        }

        // ─── Fade-in Animation ───

        private IEnumerator FadeIn(float duration)
        {
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
    }
}
