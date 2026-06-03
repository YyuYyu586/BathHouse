using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string BgmVolumeKey = "BGMVolume";
    private const string SfxVolumeKey = "SFXVolume";
    private const float DefaultVolume = 0.8f;

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
    private bool warnedMissingBgmSource;
    private bool warnedMissingSfxSource;
    private float bgmVolume = DefaultVolume;
    private float sfxVolume = DefaultVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSavedVolumes();
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

    public void PlayUIClickSFX()
    {
        PlayClickSfx();
    }

    public void PlayNormalBGM()
    {
        PlayBgm(normalBgm, "normalBgm");
    }

    public void PlayBattleBGM()
    {
        PlayBgm(battleBgm, "battleBgm");
    }

    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
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

        PlayBgm(targetClip, sceneName);
    }

    private void PlayBgm(AudioClip targetClip, string sourceName)
    {
        EnsureAudioSources();

        if (targetClip == null)
        {
            Debug.LogWarning("AudioManager " + sourceName + " is not assigned. BGM will not play.");
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
        {
            if (sources.Length == 0 && !warnedMissingBgmSource)
            {
                warnedMissingBgmSource = true;
                Debug.LogWarning("AudioManager bgmSource is not assigned. A new AudioSource will be added.");
            }

            bgmSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            if (sources.Length > 1)
            {
                sfxSource = sources[1];
            }
            else
            {
                if (!warnedMissingSfxSource)
                {
                    warnedMissingSfxSource = true;
                    Debug.LogWarning("AudioManager sfxSource is not assigned. A new AudioSource will be added.");
                }

                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        sfxSource.mute = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        ApplyVolumes();
    }

    private void LoadSavedVolumes()
    {
        bgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, DefaultVolume));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume));
    }

    private void ApplyVolumes()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}
