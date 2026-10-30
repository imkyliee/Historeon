using UnityEngine;

public class PersistentWorldItem : MonoBehaviour
{
    [Tooltip("Unique ID for this specific object in the world.")]
    public string worldID;

    [Tooltip("Enable this for tutorial pickups that should appear every time the tutorial is played.")]
    public bool ignoreCollectedStateInTutorial;
}
