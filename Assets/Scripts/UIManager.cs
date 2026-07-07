using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Health UI")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;

    [Header("UI Screen Panels")]
    public GameObject gameOverPanel;
    public GameObject pauseMenuPanel;

    [Header("Score Breakdown Text Slots")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalBestScoreText;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateScoreUI(int currentScore)
    {
        if (scoreText != null) scoreText.text = "SCORE: " + currentScore;
    }

    void Update()
    {
        // Listen for the Escape key to toggle the pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Only allow pausing if the game isn't already over
            if (gameOverPanel != null && !gameOverPanel.activeSelf)
            {
                TogglePause();
            }
        }
    }

    // --- PAUSE MENU LOGIC ---
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        // Freeze time when paused, unfreeze when continuing
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ContinueGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    // --- CORE SCENE NAVIGATION ---
    public void RestartGame()
    {
        Time.timeScale = 1f; // Ensure time is unfrozen before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time is unfrozen before leaving
        SceneManager.LoadScene("MainMenu"); // Make sure this matches your scene name exactly
    }

    public void QuitGame()
    {
        Debug.Log("Quitting application...");
        Application.Quit(); // Works in the final built version of the game
    }

    // --- GAME OVER LOGIC ---
    public void UpdateHealthUI(int currentHealth)
    {
        if (healthText != null)
        {
            healthText.text = "LIVES: " + currentHealth;
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Freeze time scale and grab final details
        Time.timeScale = 0f;

        if (GameManager.Instance != null)
        {
            int recentScore = GameManager.Instance.GetCurrentScore();
            int allTimeBest = GameManager.Instance.GetHighScore();

            // Format our text readouts for the recent game results panel
            if (finalScoreText != null) finalScoreText.text = "CURRENT SCORE: " + recentScore;
            if (finalBestScoreText != null) finalBestScoreText.text = "ALL-TIME BEST: " + allTimeBest;
        }

        SpawnManager spawnManager = Object.FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null) spawnManager.StopSpawning();
    }
}