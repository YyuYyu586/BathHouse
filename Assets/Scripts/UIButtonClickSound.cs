using UnityEngine;
using UnityEngine.UI;

// Optional component for buttons that should play the shared UI click sound.
public class UIButtonClickSound : MonoBehaviour
{
    private Button button;
    private bool warnedMissingAudioManager;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("UIButtonClickSound needs a Button on " + gameObject.name + ".");
            return;
        }

        button.onClick.RemoveListener(PlayClickSound);
        button.onClick.AddListener(PlayClickSound);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance == null)
        {
            if (!warnedMissingAudioManager)
            {
                warnedMissingAudioManager = true;
                Debug.LogWarning("UIButtonClickSound could not find AudioManager.Instance.");
            }

            return;
        }

        AudioManager.Instance.PlayClickSfx();
    }
}
