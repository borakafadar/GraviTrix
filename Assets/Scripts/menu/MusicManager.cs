using UnityEngine;

/// <summary>
/// Singleton music manager that persists across scenes.
/// Lives on the AudioManagers prefab; the AudioSource and clip are assigned in the Inspector.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private const string MusicVolumeKey = "MusicVolume";

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip musicClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource.volume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
    }

    private void Start()
    {
        PlayMusic(musicClip);
    }

    // Volume sliders only write PlayerPrefs to disk when their popup closes, so flush them if
    // the app is backgrounded or the tab loses focus while a popup is still open.
    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) PlayerPrefs.Save();
    }

    /// <summary>
    /// Starts playing the given music clip (looping).
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>
    /// Sets the music volume (0-1).
    /// Called from the Options slider.
    /// </summary>
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    /// <summary>
    /// Returns the current music volume.
    /// </summary>
    public float GetVolume()
    {
        return audioSource.volume;
    }
}
