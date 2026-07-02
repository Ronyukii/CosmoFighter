using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawning Configuration")]
    public GameObject Enemy;
    public float spawnInterval = 1.5f;
    private Vector2 screenBounds;
    private bool isSpawning = true;

    void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            screenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCamera.transform.position.z)));
        }

        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        // Wait a brief moment before spawning begins
        yield return new WaitForSeconds(1f);

        while (isSpawning)
        {
            // Calculate a random X position within the camera horizontal frame
            float randomX = Random.Range(-screenBounds.x + 0.5f, screenBounds.x - 0.5f);
            // Position the spawn location slightly above the top frame border
            Vector2 spawnPosition = new Vector2(randomX, screenBounds.y + 0.2f);

            if (Enemy != null)
            {
                Instantiate(Enemy, spawnPosition, Quaternion.identity);
            }

            // Pause execution until the next spawn wave timer cycle hits
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }
}