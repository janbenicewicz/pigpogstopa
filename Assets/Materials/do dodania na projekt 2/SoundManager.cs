using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Dźwięki gracza (losowany przy każdym odbiciu)")]
    public AudioClip[] playerHitSounds;

    [Header("Dźwięki piłki (losowany przy każdym odbiciu)")]
    public AudioClip[] ballBounceSounds;

    [Header("Dźwięki tłumu")]
    public AudioClip crowdCheerSound;
    public AudioClip crowdGroanSound;

    [Header("Głośność")]
    [Range(0f, 1f)] public float playerHitVolume = 0.8f;
    [Range(0f, 1f)] public float ballBounceVolume = 0.6f;
    [Range(0f, 1f)] public float crowdVolume = 1.0f;

    public static SoundManager Instance { get; private set; }

    private AudioSource _source;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
    }

    // ── publiczne metody ─────────────────────────────────────────────

    public void PlayPlayerHit()
    {
        PlayRandom(playerHitSounds, playerHitVolume, "PlayerHit");
    }

    public void PlayBallBounce()
    {
        PlayRandom(ballBounceSounds, ballBounceVolume, "BallBounce");
    }

    public void PlayCrowdCheer()
    {
        Play(crowdCheerSound, crowdVolume, "CrowdCheer");
    }

    public void PlayCrowdGroan()
    {
        Play(crowdGroanSound, crowdVolume, "CrowdGroan");
    }

    // ── helpers ──────────────────────────────────────────────────────

    void PlayRandom(AudioClip[] clips, float volume, string label)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("[SoundManager] " + label + ": brak przypisanych klipow!");
            return;
        }

        // odfiltruj nulle
        var valid = System.Array.FindAll(clips, c => c != null);
        if (valid.Length == 0)
        {
            Debug.LogWarning("[SoundManager] " + label + ": wszystkie klipy sa null!");
            return;
        }

        AudioClip clip = valid[Random.Range(0, valid.Length)];
        Play(clip, volume, label);
    }

    void Play(AudioClip clip, float volume, string label)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] " + label + ": klip jest null!");
            return;
        }

        _source.PlayOneShot(clip, volume);
        Debug.Log("[SoundManager] >> " + label + " -> " + clip.name + " (vol: " + volume.ToString("F2") + ")");
    }
}