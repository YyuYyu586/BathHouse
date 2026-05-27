using System.Collections;
using UnityEngine;

public class SpriteRendererAnimationPlayer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer;
    public Sprite[] frames;
    public float frameRate = 8f;
    public bool loop = true;
    public bool playOnEnable = true;

    private Coroutine playRoutine;

    private void Awake()
    {
        EnsureTargetRenderer();
    }

    private void OnEnable()
    {
        EnsureTargetRenderer();

        if (playOnEnable)
            PlayFromStart();
    }

    private void OnDisable()
    {
        StopPlayback();
    }

    public void PlayFromStart()
    {
        StopPlayback();

        if (targetRenderer == null || frames == null || frames.Length == 0)
            return;

        targetRenderer.sprite = frames[0];

        if (frames.Length == 1)
            return;

        playRoutine = StartCoroutine(PlayRoutine());
    }

    public void StopPlayback()
    {
        if (playRoutine == null)
            return;

        StopCoroutine(playRoutine);
        playRoutine = null;
    }

    private IEnumerator PlayRoutine()
    {
        int frameIndex = 0;

        while (true)
        {
            float safeFrameRate = Mathf.Max(0.01f, frameRate);
            yield return new WaitForSeconds(1f / safeFrameRate);

            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                if (!loop)
                {
                    playRoutine = null;
                    yield break;
                }

                frameIndex = 0;
            }

            if (targetRenderer != null && frames[frameIndex] != null)
                targetRenderer.sprite = frames[frameIndex];
        }
    }

    private void EnsureTargetRenderer()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();
    }
}
