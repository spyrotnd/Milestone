using UnityEngine;
using System.Collections;

public class FlashOnHit : MonoBehaviour
{
    [Header("Shader Link")]
    [Tooltip("The Renderer whose material has the exposed _FlashAmount property.")]
    public Renderer targetRenderer;

    [Tooltip("The exact name of the exposed Float property in the Shader Graph.")]
    public string propertyName = "_FlashAmount";

    [Header("Timing")]
    [Tooltip("How long the flash takes to fade back to 0. This is a response curve, the same idea as Week 3's Attack/Decay.")]
    public float flashDuration = 0.2f;

    private Coroutine activeFlash;

    public void OnHit()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning($"{name}: No Target Renderer assigned on FlashOnHit.");
            return;
        }

        if (activeFlash != null)
        {
            StopCoroutine(activeFlash);
        }
        activeFlash = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float amount = 1f - (t / flashDuration);
            targetRenderer.material.SetFloat(propertyName, amount);
            yield return null;
        }

        targetRenderer.material.SetFloat(propertyName, 0f);
        activeFlash = null;
    }
}
