using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AudioSettingsController : MonoBehaviour
{
    private const float DefaultVolume = 0.8f;

    [Header("Panel")]
    [SerializeField] private GameObject audioSettingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backButton;

    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private bool warnedMissingAudioManager;

    private void Start()
    {
        InitializeSliders();
        BindButtonFallbacks();

        if (audioSettingsPanel != null)
            audioSettingsPanel.SetActive(false);
        else
            Debug.LogWarning("AudioSettingsController audioSettingsPanel is not assigned.");
    }

    private void OnDestroy()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OpenAudioSettingsPanel);

        if (backButton != null)
            backButton.onClick.RemoveListener(CloseAudioSettingsPanel);
    }

    public void OpenAudioSettingsPanel()
    {
        PlayClickSound();

        if (audioSettingsPanel != null)
            audioSettingsPanel.SetActive(true);
        else
            Debug.LogWarning("AudioSettingsController audioSettingsPanel is not assigned.");
    }

    public void CloseAudioSettingsPanel()
    {
        PlayClickSound();

        if (audioSettingsPanel != null)
            audioSettingsPanel.SetActive(false);
        else
            Debug.LogWarning("AudioSettingsController audioSettingsPanel is not assigned.");
    }

    private void InitializeSliders()
    {
        AudioManager audioManager = GetAudioManager();
        float bgmVolume = audioManager != null ? audioManager.GetBGMVolume() : DefaultVolume;
        float sfxVolume = audioManager != null ? audioManager.GetSFXVolume() : DefaultVolume;

        InitializeSlider(bgmSlider, bgmVolume, "BgmSlider");
        InitializeSlider(sfxSlider, sfxVolume, "SfxSlider");

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void InitializeSlider(Slider slider, float value, string sliderName)
    {
        if (slider == null)
        {
            Debug.LogWarning("AudioSettingsController " + sliderName + " is not assigned.");
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    private void BindButtonFallbacks()
    {
        BindButtonFallback(settingsButton, OpenAudioSettingsPanel, nameof(OpenAudioSettingsPanel), "settingsButton");
        BindButtonFallback(backButton, CloseAudioSettingsPanel, nameof(CloseAudioSettingsPanel), "backButton");
    }

    private void BindButtonFallback(Button button, UnityEngine.Events.UnityAction action, string methodName, string fieldName)
    {
        if (button == null)
        {
            Debug.LogWarning("AudioSettingsController " + fieldName + " is not assigned.");
            return;
        }

        button.onClick.RemoveListener(action);

        if (!HasPersistentListener(button, methodName))
            button.onClick.AddListener(action);
    }

    private void OnBGMVolumeChanged(float value)
    {
        AudioManager audioManager = GetAudioManager();
        if (audioManager != null)
            audioManager.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager audioManager = GetAudioManager();
        if (audioManager != null)
            audioManager.SetSFXVolume(value);
    }

    private void PlayClickSound()
    {
        AudioManager audioManager = GetAudioManager();
        if (audioManager != null)
            audioManager.PlayUIClickSFX();
    }

    private AudioManager GetAudioManager()
    {
        if (AudioManager.Instance != null)
            return AudioManager.Instance;

        if (!warnedMissingAudioManager)
        {
            warnedMissingAudioManager = true;
            Debug.LogWarning("AudioSettingsController could not find AudioManager.Instance.");
        }

        return null;
    }

    private bool HasPersistentListener(Button button, string methodName)
    {
        int eventCount = button.onClick.GetPersistentEventCount();

        for (int i = 0; i < eventCount; i++)
        {
            if (button.onClick.GetPersistentMethodName(i) == methodName)
                return true;
        }

        return false;
    }
}
