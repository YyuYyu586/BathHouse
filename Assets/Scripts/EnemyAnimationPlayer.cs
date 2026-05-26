using UnityEngine;
using UnityEngine.UI;

public class EnemyAnimationPlayer : MonoBehaviour
{
    public Image targetImage;
    public float frameRate = 8f;
    public bool loop = true;
    public Sprite[] frames;

    private int currentFrameIndex;
    private float timer;
    private bool isPlaying;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (!isPlaying || targetImage == null || frames == null || frames.Length == 0)
            return;

        float safeFrameRate = Mathf.Max(0.01f, frameRate);
        timer += Time.deltaTime;

        if (timer < 1f / safeFrameRate)
            return;

        timer = 0f;
        currentFrameIndex++;

        if (currentFrameIndex >= frames.Length)
        {
            if (loop)
                currentFrameIndex = 0;
            else
                currentFrameIndex = frames.Length - 1;
        }

        targetImage.sprite = frames[currentFrameIndex];
    }

    public void Play(Sprite[] newFrames, Sprite fallbackSprite)
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage == null)
        {
            Debug.LogWarning("EnemyAnimationPlayer targetImage is not assigned and no Image exists on " + gameObject.name + ".");
            return;
        }

        frames = newFrames;
        currentFrameIndex = 0;
        timer = 0f;

        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning("EnemyAnimationPlayer frames are empty on " + gameObject.name + ". Showing fallback sprite.");
            StopAndShow(fallbackSprite);
            return;
        }

        if (frames[0] == null)
        {
            Debug.LogWarning("EnemyAnimationPlayer first frame is null on " + gameObject.name + ". Showing fallback sprite.");
            StopAndShow(fallbackSprite);
            return;
        }

        targetImage.sprite = frames[0];
        isPlaying = true;
        Debug.Log("EnemyAnimationPlayer started on " + gameObject.name + ". frames=" + frames.Length + ", frameRate=" + frameRate + ".");
    }

    public void StopAndShow(Sprite fallbackSprite)
    {
        isPlaying = false;
        timer = 0f;
        currentFrameIndex = 0;

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage == null)
        {
            Debug.LogWarning("EnemyAnimationPlayer targetImage is not assigned and no Image exists on " + gameObject.name + ".");
            return;
        }

        if (fallbackSprite == null)
        {
            Debug.LogWarning("EnemyAnimationPlayer fallbackSprite is null on " + gameObject.name + ". Keeping current image.");
            return;
        }

        targetImage.sprite = fallbackSprite;
        Debug.Log("EnemyAnimationPlayer stopped on " + gameObject.name + ". Showing fallback sprite=" + fallbackSprite.name + ".");
    }
}
