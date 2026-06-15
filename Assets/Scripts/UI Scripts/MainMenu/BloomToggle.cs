using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomSettings : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;

    private Bloom bloom;

    private void Start()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume is NOT assigned!");
            return;
        }

        // Try to get Bloom from the profile
        if (!globalVolume.profile.TryGet(out bloom))
        {
            Debug.LogError("Bloom NOT found in Volume Profile!");
        }
        else
        {
            Debug.Log("Bloom successfully found!");
        }
    }

    public void SetBloom(bool enabled)
{
        if (globalVolume == null) return;

        globalVolume.weight = enabled ? 1f : 0f;
}
}