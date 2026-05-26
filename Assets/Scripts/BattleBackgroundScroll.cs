using UnityEngine;
using UnityEngine.UI;

// Scrolls a RawImage UV rect for a simple retro JRPG battle background effect.
public class BattleBackgroundScroll : MonoBehaviour
{
    public RawImage rawImage;
    public Vector2 scrollSpeed = new Vector2(0.02f, 0f);
    public Vector2 uvScale = new Vector2(1.5f, 1.5f);

    private Vector2 uvOffset;
    private bool warnedMissingRawImage;

    private void Awake()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        ApplyUvRect();

        if (rawImage == null && !warnedMissingRawImage)
        {
            warnedMissingRawImage = true;
            Debug.LogWarning("BattleBackgroundScroll needs a RawImage on " + gameObject.name + ".");
        }
    }

    private void Update()
    {
        if (rawImage == null)
            return;

        uvOffset += scrollSpeed * Time.deltaTime;
        uvOffset.x = Mathf.Repeat(uvOffset.x, 1f);
        uvOffset.y = Mathf.Repeat(uvOffset.y, 1f);
        ApplyUvRect();
    }

    private void ApplyUvRect()
    {
        if (rawImage == null)
            return;

        rawImage.uvRect = new Rect(uvOffset.x, uvOffset.y, uvScale.x, uvScale.y);
    }
}
