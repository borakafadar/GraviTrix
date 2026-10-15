using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuBuilder : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    [Header("Scene Settings")]
    [SerializeField] private string playSceneName = "MainScene";
    [SerializeField] private string recordPlayerPrefsKey = "HighScore";

    [Header("Scene References")]
    [SerializeField] private Text recordText;
    [SerializeField] private GameObject optionsPopup;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Text musicVolumeText;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Text sfxVolumeText;
    [SerializeField] private GameObject exitButton;

    private void Start()
    {
#if UNITY_WEBGL
        exitButton.SetActive(false); // Application.Quit does nothing in a browser
#endif


        recordText.text = "TOP RECORD: " + PlayerPrefs.GetInt(recordPlayerPrefsKey, 0);

        musicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f));
        sfxVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f));
        UpdateVolumeText(musicVolumeText, "MUSIC VOLUME: ", musicVolumeSlider.value);
        UpdateVolumeText(sfxVolumeText, "SFX VOLUME: ", sfxVolumeSlider.value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        UpdateVolumeText(musicVolumeText, "MUSIC VOLUME: ", value);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(value);
        }
    }

    public void OnSfxVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        UpdateVolumeText(sfxVolumeText, "SFX VOLUME: ", value);

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.SetVolume(value);
        }
    }

    private static void UpdateVolumeText(Text label, string prefix, float value)
    {
        label.text = prefix + Mathf.RoundToInt(value * 100f) + "%";
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadScene(playSceneName);
    }

    public void OnOptionsClicked()
    {
        optionsPopup.SetActive(true);
    }

    public void CloseOptionsPopup()
    {
        optionsPopup.SetActive(false);
        PlayerPrefs.Save();
    }

    public void OnExitClicked()
    {
        Debug.Log("Exit clicked.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
