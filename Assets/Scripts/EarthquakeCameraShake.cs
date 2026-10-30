using System.Collections;
using UnityEngine;

public class EarthquakeCameraShake : MonoBehaviour
{
    [Header("Earthquake Settings")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float intensity = 0.2f;
    [SerializeField] private float shakeSpeed = 20f;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void StartEarthquake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        originalPosition = transform.localPosition;
        shakeCoroutine = StartCoroutine(ShakeCamera());
    }

    public void StopEarthquake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        transform.localPosition = originalPosition;
    }

    private IEnumerator ShakeCamera()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            transform.localPosition = originalPosition +
                new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(
                1f / Mathf.Max(shakeSpeed, 1f)
            );
        }

        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}