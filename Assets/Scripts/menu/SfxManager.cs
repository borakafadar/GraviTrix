using UnityEngine;

/// <summary>
/// Singleton SFX manager that persists across scenes.
/// Lives on the AudioManagers prefab; the AudioSource and clips are assigned in the Inspector.
/// Volume is controlled via the Options SFX slider (PlayerPrefs).
/// </summary>
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    private const string SfxVolumeKey = "SfxVolume";

    [SerializeField] private AudioSource audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip blockDropClip;
    [SerializeField] private AudioClip gravityChangeClip;
    [SerializeField] private AudioClip metalBlockClip;
    [SerializeField] private AudioClip lavaMeltClip;
    [SerializeField] private AudioClip lineClearClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip bombExplosionClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource.volume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f);
    }

    // ==================== Public play methods ====================

    /// <summary>
    /// Play the normal block drop sound.
    /// Called when a normal piece locks into place.
    /// </summary>
    public void PlayBlockDrop()
    {
        PlayClip(blockDropClip);
    }

    /// <summary>
    /// Play the gravity/board rotation sound.
    /// Called when the board rotates.
    /// </summary>
    public void PlayGravityChange()
    {
        PlayClip(gravityChangeClip);
    }

    /// <summary>
    /// Play the metal block landing sound.
    /// Called when a metal piece locks into place.
    /// </summary>
    public void PlayMetalBlockDrop()
    {
        PlayClip(metalBlockClip);
    }

    /// <summary>
    /// Play the lava melt sound.
    /// Called when lava burns cells underneath it.
    /// </summary>
    public void PlayLavaMelt()
    {
        PlayClip(lavaMeltClip);
    }

    /// <summary>
    /// Play the line clear sound.
    /// Called when one or more rows are cleared.
    /// </summary>
    public void PlayLineClear()
    {
        PlayClip(lineClearClip);
    }

    /// <summary>
    /// Play the game over sound.
    /// </summary>
    public void PlayGameOver()
    {
        PlayClip(gameOverClip);
    }

    /// <summary>
    /// Play the massive bomb explosion sound.
    /// </summary>
    public void PlayBombExplosion()
    {
        PlayClip(bombExplosionClip);
    }

    // ==================== Volume control ====================

    /// <summary>
    /// Sets the SFX volume (0-1).
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
    /// Returns the current SFX volume.
    /// </summary>
    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 0.8f;
    }

    // ==================== Internal ====================

    private void PlayClip(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}
