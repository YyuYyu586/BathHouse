using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillTooltipManager tooltipManager;
    public string tooltipTitle;
    [TextArea]
    public string tooltipDescription;
    public float hoverDelay = 0.5f;

    private Coroutine hoverRoutine;
    private bool pointerInside;
    private bool tooltipShown;
    private bool warnedMissingManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;

        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);

        hoverRoutine = StartCoroutine(ShowAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        CancelHoverRoutine();
        HideTooltip();
    }

    private void OnDisable()
    {
        pointerInside = false;
        CancelHoverRoutine();
        HideTooltip();
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, hoverDelay));

        hoverRoutine = null;

        if (!pointerInside)
            yield break;

        if (tooltipManager == null)
        {
            WarnMissingManager();
            yield break;
        }

        tooltipManager.ShowTooltip(tooltipTitle, tooltipDescription);
        tooltipShown = true;
    }

    private void CancelHoverRoutine()
    {
        if (hoverRoutine == null)
            return;

        StopCoroutine(hoverRoutine);
        hoverRoutine = null;
    }

    private void HideTooltip()
    {
        if (!tooltipShown)
            return;

        if (tooltipManager != null)
            tooltipManager.HideTooltip();
        else
            WarnMissingManager();

        tooltipShown = false;
    }

    private void WarnMissingManager()
    {
        if (warnedMissingManager)
            return;

        warnedMissingManager = true;
        Debug.LogWarning("SkillTooltipTrigger tooltipManager is not assigned on " + gameObject.name + ".");
    }
}
