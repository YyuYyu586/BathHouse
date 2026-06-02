using TMPro;
using UnityEngine;

public class SkillTooltipManager : MonoBehaviour
{
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTitleText;
    public TextMeshProUGUI tooltipDescriptionText;

    private bool warnedMissingPanel;
    private bool warnedMissingTitleText;
    private bool warnedMissingDescriptionText;

    private void Start()
    {
        HideTooltip();
    }

    public void ShowTooltip(string title, string description)
    {
        if (tooltipTitleText != null)
            tooltipTitleText.text = title;
        else
            WarnMissingTitleText();

        if (tooltipDescriptionText != null)
            tooltipDescriptionText.text = description;
        else
            WarnMissingDescriptionText();

        if (tooltipPanel != null)
            tooltipPanel.SetActive(true);
        else
            WarnMissingPanel();
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
        else
            WarnMissingPanel();
    }

    private void WarnMissingPanel()
    {
        if (warnedMissingPanel)
            return;

        warnedMissingPanel = true;
        Debug.LogWarning("SkillTooltipManager tooltipPanel is not assigned.");
    }

    private void WarnMissingTitleText()
    {
        if (warnedMissingTitleText)
            return;

        warnedMissingTitleText = true;
        Debug.LogWarning("SkillTooltipManager tooltipTitleText is not assigned.");
    }

    private void WarnMissingDescriptionText()
    {
        if (warnedMissingDescriptionText)
            return;

        warnedMissingDescriptionText = true;
        Debug.LogWarning("SkillTooltipManager tooltipDescriptionText is not assigned.");
    }
}
