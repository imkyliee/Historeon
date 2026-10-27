
using System.Collections;
using UnityEngine;

public class PlatformDisappear : MonoBehaviour
{
    [Header("Platform Settings")]
    public float enabledDuration = 3f;
    public float disabledDuration = 2f;
    public float startDelay = 0f;

    private Renderer[] platformRenderers;
    private Collider[] platformColliders;
    private Coroutine platformCoroutine;

    private void Awake()
    {
        platformRenderers = GetComponentsInChildren<Renderer>();
        platformColliders = GetComponentsInChildren<Collider>();
    }

    private void OnEnable()
    {
        platformCoroutine = StartCoroutine(PlatformCycle());
    }

    private void OnDisable()
    {
        if (platformCoroutine != null)
        {
            StopCoroutine(platformCoroutine);
            platformCoroutine = null;
        }
    }

    private IEnumerator PlatformCycle()
    {
        if (startDelay > 0f)
        {
            SetPlatformEnabled(false);
            yield return new WaitForSeconds(startDelay);
        }

        while (true)
        {
            SetPlatformEnabled(true);
            yield return new WaitForSeconds(enabledDuration);

            SetPlatformEnabled(false);
            yield return new WaitForSeconds(disabledDuration);
        }
    }

    private void SetPlatformEnabled(bool state)
    {
        foreach (Renderer platformRenderer in platformRenderers)
        {
            if (platformRenderer != null)
                platformRenderer.enabled = state;
        }

        foreach (Collider platformCollider in platformColliders)
        {
            if (platformCollider != null)
                platformCollider.enabled = state;
        }
    }
}
