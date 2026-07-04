using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // A globally accessible instance so other scripts can easily award points
    public static GameManager Instance { get; private set; }

    [Header("Scoring System")]
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;

    void Awake()
    {
        // Enforce a strict Singleton pattern structure
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI();
    }

    // Call this method from other scripts to grant performance score points
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + currentScore;
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}