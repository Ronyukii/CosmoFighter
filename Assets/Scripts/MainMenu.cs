using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // This function will be called when the Play button is pressed
    public void PlayGame()
    {
        // Loads the gameplay scene by its name.
        SceneManager.LoadScene("CosmoFighter");
    }
}