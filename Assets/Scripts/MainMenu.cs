using UnityEngine;
using SceneManagement = UnityEngine.SceneManagement.SceneManager;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI Text Components")]
    public TextMeshProUGUI mainMenuBestScoreText;

    void Start()
    {
        // Load the persistent high score when the player boots up the game
        int savedHigh = PlayerPrefs.GetInt("CosmoFighter_HighScore", 0);

        if (mainMenuBestScoreText != null)
        {
            mainMenuBestScoreText.text = "BEST SCORE: " + savedHigh;
        }
    }

    public void PlayEasyMode()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.isHardMode = false;
        }
        SceneManagement.LoadScene("CosmoFighter"); // Replace with your gameplay scene name
    }

    public void PlayHardMode()
    {
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.isHardMode = true;
        }
        SceneManagement.LoadScene("CosmoFighter");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}