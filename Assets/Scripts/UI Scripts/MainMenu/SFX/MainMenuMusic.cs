using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    [SerializeField] private GameObject soundManagerPrefab;

    void Start()
    {
        if (SoundManager.Instance == null)
        {
            Instantiate(soundManagerPrefab);
        }
    }
}