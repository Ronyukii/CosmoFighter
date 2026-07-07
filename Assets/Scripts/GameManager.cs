using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int currentScore = 0;
    private const string HighScoreKey = "CosmoFighter_HighScore"; // Unique save key for PlayerPrefs

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int points)
    {
        currentScore += points;

        // Update the HUD display in real-time
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreUI(currentScore);
        }

        // Check and save high score instantly if broken
        CheckAndSaveHighScore();
    }

    private void CheckAndSaveHighScore()
    {
        int savedHighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        if (currentScore > savedHighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, currentScore);
            PlayerPrefs.Save(); // Writes the data to the local disk disk
        }
    }

    public int GetCurrentScore() => currentScore;

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt(HighScoreKey, 0);
    }
}