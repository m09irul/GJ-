using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using Unity.VisualScripting;

public class BustedPostProcess : MonoBehaviour
{
    public Volume volume;

    ColorAdjustments color;
    Vignette vignette;

    private void Awake()
    {
        volume.profile.TryGet(out color);
        volume.profile.TryGet(out vignette);
    }

    public void PlayBustedEffect()
    {
        StopAllCoroutines();
        StartCoroutine(BustedRoutine());
    }

    IEnumerator BustedRoutine()
    {
        float t = 0f;
        float duration = 0.6f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / duration;

            color.saturation.value = Mathf.Lerp(0, -80, lerp);
            color.postExposure.value = Mathf.Lerp(0, -0.5f, lerp);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(0, 0.35f, lerp);

            yield return null;
        }
    }
}
