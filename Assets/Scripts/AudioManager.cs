using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip normalBgm;
    [SerializeField] private AudioClip battleBgm;
    [SerializeField] private AudioClip uiClickSfx;

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private bool warnedMissingNormalBgm;
    private bool warnedMissingBattleBgm;
    private bool warnedMissingClickSfx;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureAudioSources();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        PlayBgmForScene(SceneManager.GetActiveScene().name);
    }

    public void PlayClickSfx()
    {
        if (uiClickSfx == null)
        {
            if (!warnedMissingClickSfx)
            {
                warnedMissingClickSfx = true;
                Debug.LogWarning("AudioManager uiClickSfx is not assigned. UI click sound will not play.");
            }

            return;
        }

        EnsureAudioSources();
        sfxSource.PlayOneShot(uiClickSfx);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBgmForScene(scene.name);
    }

    private void PlayBgmForScene(string sceneName)
    {
        EnsureAudioSources();

        AudioClip targetClip = GetBgmForScene(sceneName);
        if (targetClip == null)
        {
            WarnMissingBgmForScene(sceneName);
            if (bgmSource.isPlaying)
                bgmSource.Stop();

            bgmSource.clip = null;
            return;
        }

        if (bgmSource.clip == targetClip && bgmSource.isPlaying)
            return;

        bgmSource.clip = targetClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private AudioClip GetBgmForScene(string sceneName)
    {
        if (sceneName == "CombatScene")
            return battleBgm;

        if (sceneName == "MainMenu" ||
            sceneName == "StoryScene" ||
            sceneName == "BathhouseMain" ||
            sceneName == "AfterCombatScene")
        {
            return normalBgm;
        }

        return null;
    }

    private void WarnMissingBgmForScene(string sceneName)
    {
        if (sceneName == "CombatScene")
        {
            if (warnedMissingBattleBgm)
                return;

            warnedMissingBattleBgm = true;
            Debug.LogWarning("AudioManager battleBgm is not assigned. CombatScene BGM will not play.");
            return;
        }

        if (sceneName == "MainMenu" ||
            sceneName == "StoryScene" ||
            sceneName == "BathhouseMain" ||
            sceneName == "AfterCombatScene")
        {
            if (warnedMissingNormalBgm)
                return;

            warnedMissingNormalBgm = true;
            Debug.LogWarning("AudioManager normalBgm is not assigned. Normal scene BGM will not play.");
        }
    }

    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (bgmSource == null)
            bgmSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();

        if (sfxSource == null)
        {
            if (sources.Length > 1)
                sfxSource = sources[1];
            else
                sfxSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }
}
