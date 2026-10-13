using System.Collections;
using GraviTrix.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraviTrix.UI
{
    public sealed class PauseMenuView : MonoBehaviour
    {
        private const string HomeSceneName = "HomeScene";
        private const string SfxVolumeKey = "SfxVolume";
        private const string MusicVolumeKey = "MusicVolume";

        [SerializeField] private GameController gameController;

        [Header("Scene References")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Text musicVolumeText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Text sfxVolumeText;

        private bool isPaused;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        public void TogglePause() { if (isPaused) ResumeGame(); else PauseGame(); }

        private void PauseGame()
        {
            // Esc or the pause button during game over would open the pause popup behind the
            // game-over popup and keep it up after Retry.
            if (gameController.Phase == GamePhase.GameOver) return;

            isPaused = true; Time.timeScale = 0f;

            musicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f));
            sfxVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f));
            UpdateVolumeText(musicVolumeText, "MUSIC VOLUME: ", musicVolumeSlider.value);
            UpdateVolumeText(sfxVolumeText, "SFX VOLUME: ", sfxVolumeSlider.value);

            popupRoot.SetActive(true);
            StartCoroutine(FadeIn(0.25f));
        }

        public void ResumeGame()
        {
            isPaused = false; Time.timeScale = 1f;
            popupRoot.SetActive(false);
            PlayerPrefs.Save();
        }

        // ─── Callbacks (wired in the Inspector) ───

        public void OnMusicVolumeChanged(float v)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, v);
            UpdateVolumeText(musicVolumeText, "MUSIC VOLUME: ", v);
            if (MusicManager.Instance != null) MusicManager.Instance.SetVolume(v);
        }

        public void OnSfxVolumeChanged(float v)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, v);
            UpdateVolumeText(sfxVolumeText, "SFX VOLUME: ", v);
            if (SfxManager.Instance != null) SfxManager.Instance.SetVolume(v);
        }

        private static void UpdateVolumeText(Text label, string prefix, float value)
        {
            label.text = prefix + Mathf.RoundToInt(value * 100f) + "%";
        }

        public void OnRestartClicked()
        {
            ResumeGame();
            gameController.RestartGame();
        }

        public void OnQuitClicked()
        {
            Time.timeScale = 1f;
            PlayerPrefs.Save();
            SceneManager.LoadScene(HomeSceneName);
        }

        private IEnumerator FadeIn(float duration)
        {
            canvasGroup.alpha = 0f; float el = 0f;
            while (el < duration)
            {
                el += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(el / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }
}
