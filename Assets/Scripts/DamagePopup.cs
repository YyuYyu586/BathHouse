using System.Collections;
using TMPro;
using UnityEngine;

// Small UI popup that floats upward and destroys itself.
public class DamagePopup : MonoBehaviour
{
    public float lifetime = 0.8f;
    public float floatDistance = 60f;

    private RectTransform rectTransform;
    private TextMeshProUGUI textComponent;

    public static DamagePopup Create(Transform parent, Vector2 anchoredPosition, string text)
    {
        GameObject popupObject = new GameObject("DamagePopup", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(DamagePopup));
        popupObject.transform.SetParent(parent, false);

        RectTransform rect = popupObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(520f, 60f);
        rect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI label = popupObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 30f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;

        return popupObject.GetComponent<DamagePopup>();
    }

    public void Play()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TextMeshProUGUI>();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        Vector2 start = rectTransform.anchoredPosition;
        Vector2 end = start + new Vector2(0f, floatDistance);
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);

            rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);

            Color color = textComponent.color;
            color.a = 1f - t;
            textComponent.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}
