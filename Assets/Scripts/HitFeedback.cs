using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Small optional combat hit feedback. Safe to remove after presentation polish.
public class HitFeedback : MonoBehaviour
{
    public Color flashColor = Color.red;
    public float flashDuration = 0.3f;
    public float shakeDuration = 0.3f;
    public float shakeStrength = 12f;
    public int shakeTimes = 3;

    private Image image;
    private SpriteRenderer spriteRenderer;
    private RectTransform rectTransform;
    private Color originalImageColor;
    private Color originalSpriteColor;
    private Vector2 originalAnchoredPosition;
    private Vector3 originalLocalPosition;
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rectTransform = GetComponent<RectTransform>();

        CaptureOriginalState();
    }

    private void OnDisable()
    {
        StopAndRestore();
    }

    public void Play()
    {
        StopAndRestore();
        CaptureOriginalState();
        feedbackRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        float totalDuration = Mathf.Max(0.01f, flashDuration, shakeDuration);
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            ApplyFlash(elapsed);
            ApplyShake(elapsed);
            yield return null;
        }

        RestoreOriginalState();
        feedbackRoutine = null;
    }

    private void ApplyFlash(float elapsed)
    {
        if (flashDuration <= 0f)
            return;

        float flashProgress = Mathf.Clamp01(elapsed / flashDuration);
        if (image != null)
            image.color = Color.Lerp(flashColor, originalImageColor, flashProgress);

        if (spriteRenderer != null)
            spriteRenderer.color = Color.Lerp(flashColor, originalSpriteColor, flashProgress);
    }

    private void ApplyShake(float elapsed)
    {
        if (shakeDuration <= 0f || shakeStrength <= 0f || shakeTimes <= 0)
            return;

        float shakeProgress = Mathf.Clamp01(elapsed / shakeDuration);
        float damping = 1f - shakeProgress;
        float offset = Mathf.Sin(shakeProgress * shakeTimes * Mathf.PI * 2f) * shakeStrength * damping;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(offset, 0f);
        }
        else
        {
            transform.localPosition = originalLocalPosition + new Vector3(offset, 0f, 0f);
        }
    }

    private void CaptureOriginalState()
    {
        if (image != null)
            originalImageColor = image.color;

        if (spriteRenderer != null)
            originalSpriteColor = spriteRenderer.color;

        if (rectTransform != null)
            originalAnchoredPosition = rectTransform.anchoredPosition;

        originalLocalPosition = transform.localPosition;
    }

    private void StopAndRestore()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        RestoreOriginalState();
    }

    private void RestoreOriginalState()
    {
        if (image != null)
            image.color = originalImageColor;

        if (spriteRenderer != null)
            spriteRenderer.color = originalSpriteColor;

        if (rectTransform != null)
            rectTransform.anchoredPosition = originalAnchoredPosition;
        else
            transform.localPosition = originalLocalPosition;
    }
}
