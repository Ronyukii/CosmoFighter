using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayEasyMode()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(false); // Easy Mode selected
        }
        SceneManager.LoadScene("CosmoFighter");
    }

    public void PlayHardMode()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.SetDifficulty(true);  // Hard Mode selected
        }
        SceneManager.LoadScene("CosmoFighter");
    }
}