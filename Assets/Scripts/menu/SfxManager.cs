using UnityEngine;

/// <summary>
/// Singleton SFX manager that persists across scenes.
/// Loads sound effect clips from Resources/SoundEffects and plays them on demand.
/// Volume is controlled via the Options SFX slider (PlayerPrefs).
/// </summary>
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    private AudioSource audioSource;

    private const string SfxVolumeKey = "SfxVolume";

    // --- Existing sound effects (loaded from Resources) ---
    private AudioClip blockDropClip;
    private AudioClip gravityChangeClip;
    private AudioClip metalBlockClip;

    // --- Placeholder sound effects (user will add mp3 files later) ---
    private AudioClip lavaMeltClip;
    private AudioClip lineClearClip;
    private AudioClip gameOverClip;
    private AudioClip bombExplosionClip;

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
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound

        float savedVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f);
        audioSource.volume = savedVolume;

        LoadClips();
    }

    private void LoadClips()
    {
        // Existing sound effects
        blockDropClip = Resources.Load<AudioClip>("SoundEffects/block_drop");
        gravityChangeClip = Resources.Load<AudioClip>("SoundEffects/gravity_change_sound");
        metalBlockClip = Resources.Load<AudioClip>("SoundEffects/metal_block_sound");

        // Placeholder sound effects — user will add these mp3 files to Resources/SoundEffects/
        lavaMeltClip = Resources.Load<AudioClip>("SoundEffects/lava_melt_sound");
        lineClearClip = Resources.Load<AudioClip>("SoundEffects/line_clear_sound");
        gameOverClip = Resources.Load<AudioClip>("SoundEffects/game_over");
        bombExplosionClip = Resources.Load<AudioClip>("SoundEffects/explosion_sound");
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
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, audioSource.volume);
    }
}
