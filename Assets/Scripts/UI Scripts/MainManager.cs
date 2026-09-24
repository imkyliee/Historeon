using UnityEngine;
using System.Collections.Generic;

public class MainManager : MonoBehaviour
{
    public List<string> questNames = new();

    public static MainManager mainManager;

    private void Awake()
    {
        if (mainManager != null && mainManager != this)
        {
            Destroy(gameObject);
            return;
        }

        mainManager = this;
        DontDestroyOnLoad(gameObject);
    }
}