using UnityEngine;
using UnityEngine.UI;

// Applies lightweight wave shader parameters to the CombatScene background RawImage.
public class BattleBackgroundWaveController : MonoBehaviour
{
    public RawImage rawImage;

    [Header("Wave")]
    public float waveStrength = 0.006f;
    public float waveFrequency = 8f;
    public float waveSpeed = 0.5f;
    public float secondaryStrength = 0.003f;
    public float secondaryFrequency = 5f;

    private Material runtimeMaterial;

    private void Awake()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        if (rawImage == null)
        {
            Debug.LogWarning("BattleBackgroundWaveController needs a RawImage on " + gameObject.name + ".");
            return;
        }

        if (rawImage.material == null)
        {
            Debug.LogWarning("BattleBackgroundWaveController needs a wave material on " + gameObject.name + ".");
            return;
        }

        runtimeMaterial = Instantiate(rawImage.material);
        rawImage.material = runtimeMaterial;
        ApplyParameters();
    }

    private void Update()
    {
        ApplyParameters();
    }

    private void ApplyParameters()
    {
        if (runtimeMaterial == null)
            return;

        runtimeMaterial.SetFloat("_WaveStrength", waveStrength);
        runtimeMaterial.SetFloat("_WaveFrequency", waveFrequency);
        runtimeMaterial.SetFloat("_WaveSpeed", waveSpeed);
        runtimeMaterial.SetFloat("_SecondaryStrength", secondaryStrength);
        runtimeMaterial.SetFloat("_SecondaryFrequency", secondaryFrequency);
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
