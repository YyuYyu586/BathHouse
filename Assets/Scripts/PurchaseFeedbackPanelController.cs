using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseFeedbackPanelController : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Image iconImage;
    public TextMeshProUGUI feedbackText;

    [Header("Timing")]
    public float hideDelay = 1.2f;

    private Coroutine hideRoutine;
    private bool warnedMissingPanel;
    private bool warnedMissingIconImage;
    private bool warnedMissingFeedbackText;

    private void Awake()
    {
        ResolveReferences();
        Hide();
    }

    public void Show(Sprite icon, string message)
    {
        ResolveReferences();

        if (panel == null)
        {
            WarnMissingPanel();
            return;
        }

        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
        else
        {
            WarnMissingFeedbackText();
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }
        else
        {
            WarnMissingIconImage();
        }

        panel.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void Hide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (panel != null)
            panel.SetActive(false);
    }

    private System.Collections.IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, hideDelay));
        hideRoutine = null;
        Hide();
    }

    private void ResolveReferences()
    {
        if (panel == null)
        {
            Transform panelTransform = transform.Find("PurchaseFeedbackPanel");
            if (panelTransform != null)
                panel = panelTransform.gameObject;
        }

        if (iconImage == null)
        {
            Transform iconTransform = panel != null ? panel.transform.Find("ItemIcon") : transform.Find("ItemIcon");
            if (iconTransform != null)
                iconImage = iconTransform.GetComponent<Image>();
        }

        if (feedbackText == null)
        {
            Transform textTransform = panel != null ? panel.transform.Find("FeedbackText") : transform.Find("FeedbackText");
            if (textTransform != null)
                feedbackText = textTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    private void WarnMissingPanel()
    {
        if (warnedMissingPanel)
            return;

        warnedMissingPanel = true;
        Debug.LogWarning("PurchaseFeedbackPanelController panel is not assigned.");
    }

    private void WarnMissingIconImage()
    {
        if (warnedMissingIconImage)
            return;

        warnedMissingIconImage = true;
        Debug.LogWarning("PurchaseFeedbackPanelController iconImage is not assigned.");
    }

    private void WarnMissingFeedbackText()
    {
        if (warnedMissingFeedbackText)
            return;

        warnedMissingFeedbackText = true;
        Debug.LogWarning("PurchaseFeedbackPanelController feedbackText is not assigned.");
    }
}
