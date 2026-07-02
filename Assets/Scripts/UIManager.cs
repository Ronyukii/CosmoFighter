using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Health UI")]
    public TextMeshProUGUI healthText;

    [Header("Screens")]
    public GameObject gameOverPanel; // The UI screen container

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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

        // Stop spawning enemies when game is over
        SpawnManager spawnManager = Object.FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.StopSpawning();
        }
    }

    // This method will be linked to your UI Restart button
    public void RestartGame()
    {
        // Reloads the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}