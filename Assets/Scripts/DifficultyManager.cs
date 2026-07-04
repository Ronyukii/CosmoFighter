using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [HideInInspector]
    public bool isHardMode = false; // Persistent toggle state

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persists across scene reloads
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDifficulty(bool hard)
    {
        isHardMode = hard;
    }
}