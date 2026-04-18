using UnityEngine;

/// <summary>
/// Singleton music manager that persists across scenes.
/// Plays the background music and allows volume control via PlayerPrefs.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;

    private const string MusicVolumeKey = "MusicVolume";

    [Header("Music Clip (assigned at runtime if not set)")]
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

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound

        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
        audioSource.volume = savedVolume;
    }

    private void Start()
    {
        if (audioSource.isPlaying)
        {
            return;
        }

        if (musicClip != null)
        {
            PlayMusic(musicClip);
        }
        else
        {
            // Fallback: load from Resources/Music folder
            AudioClip clip = Resources.Load<AudioClip>("Music/The_Final_Row");
            if (clip != null)
            {
                PlayMusic(clip);
            }
            else
            {
                Debug.LogWarning("[MusicManager] Could not find music clip in Resources/Music/The_Final_Row");
            }
        }
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
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Returns the current music volume.
    /// </summary>
    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 0.8f;
    }
}
