using UnityEngine;

/// <summary>
/// Simple AudioManager that plays SFX in response to game events.
/// Uses multiple AudioSources to allow overlapping sounds.
///
/// Attach to a persistent GameObject. Assign clips in Inspector.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Inspector Fields
    // ------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;      // one-shot SFX
    [SerializeField] private AudioSource reelLoopSource; // looping reel spin sound
    [SerializeField] private AudioSource musicSource;    // background music

    [Header("Clips")]
    [SerializeField] private AudioClip spinStartClip;
    [SerializeField] private AudioClip reelStopClip;
    [SerializeField] private AudioClip smallWinClip;
    [SerializeField] private AudioClip bigWinClip;
    [SerializeField] private AudioClip jackpotClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip freeSpinClip;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume   = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume  = 0.4f;
    [Range(0f, 1f)] [SerializeField] private float reelVolume   = 0.5f;

    // ------------------------------------------------------------------
    // Unity Lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        gameManager.onSpinStarted.AddListener(OnSpinStarted);
        gameManager.onSpinResult.AddListener(OnSpinResult);
        gameManager.onFreeSpinAwarded.AddListener(OnFreeSpinAwarded);

        // Configure audio sources
        sfxSource.volume        = sfxVolume;
        reelLoopSource.volume   = reelVolume;
        reelLoopSource.loop     = true;
        musicSource.volume      = musicVolume;
        musicSource.loop        = true;

        if (musicSource.clip != null)
            musicSource.Play();
    }

    // ------------------------------------------------------------------
    // Event Handlers
    // ------------------------------------------------------------------

    private void OnSpinStarted()
    {
        Play(sfxSource, spinStartClip);
        reelLoopSource.Play();
    }

    private void OnSpinResult(int payout, WinType winType)
    {
        reelLoopSource.Stop();
        Play(sfxSource, reelStopClip);

        if (payout <= 0) return;

        switch (winType)
        {
            case WinType.AllWilds:
                Play(sfxSource, jackpotClip);
                break;
            case WinType.Scatter:
                Play(sfxSource, bigWinClip);
                break;
            default:
                Play(sfxSource, payout > 50 ? bigWinClip : smallWinClip);
                break;
        }
    }

    private void OnFreeSpinAwarded()
    {
        Play(sfxSource, freeSpinClip);
    }

    // ------------------------------------------------------------------
    // Public API (called by UI buttons via Inspector events)
    // ------------------------------------------------------------------

    public void PlayButtonClick()
    {
        Play(sfxSource, buttonClickClip);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private void Play(AudioSource source, AudioClip clip)
    {
        if (clip == null || source == null) return;
        source.PlayOneShot(clip, sfxVolume);
    }
}
