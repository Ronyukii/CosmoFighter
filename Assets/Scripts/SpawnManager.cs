using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Prefab Variants")]
    public GameObject smallEnemyPrefab;
    public GameObject intermediateEnemyPrefab;
    public GameObject bossEnemyPrefab;

    [Header("Score Milestones for Progression")]
    public int intermediateMilestone = 500;
    public int bossMilestone = 2000;

    [Header("Spawning Controls")]
    public float baseSpawnInterval = 3f;
    private Vector2 screenBounds;
    private bool isSpawning = true;
    private bool bossSpawned = false;

    void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            screenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCamera.transform.position.z)));
        }

        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (isSpawning)
        {
            int currentScore = GameManager.Instance != null ? GameManager.Instance.GetCurrentScore() : 0;

            // Phase 3: Boss Encounter Milestone Reached
            if (currentScore >= bossMilestone)
            {
                if (!bossSpawned)
                {
                    SpawnBoss();
                }
                // Pause standard mob spawning completely during the boss fight
                yield return new WaitForSeconds(5f);
                continue;
            }

            // Determine Spawning Quantities and Mix based on Score
            float currentSpawnInterval = baseSpawnInterval;
            GameObject enemyToSpawn = smallEnemyPrefab;

            if (currentScore >= intermediateMilestone)
            {
                // Phase 2: Ramping up. 40% chance to spawn Intermediate, 60% chance to spawn Small
                enemyToSpawn = Random.value > 0.6f ? intermediateEnemyPrefab : smallEnemyPrefab;

                // Reduce the spawn interval to increase the quantity/density of enemies
                currentSpawnInterval = baseSpawnInterval * 0.6f;
            }
            else
            {
                // Phase 1: Lowest level enemy only
                enemyToSpawn = smallEnemyPrefab;
                currentSpawnInterval = baseSpawnInterval;
            }

            // Execute the Spawn
            if (enemyToSpawn != null)
            {
                float randomX = Random.Range(-screenBounds.x + 0.8f, screenBounds.x - 0.8f);
                Vector2 spawnPosition = new Vector2(randomX, screenBounds.y + 0.5f);
                Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
            }

            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    void SpawnBoss()
    {
        bossSpawned = true;
        if (bossEnemyPrefab != null)
        {
            // Spawn the Boss perfectly dead-center horizontally above the frame
            Vector2 spawnPosition = new Vector2(0f, screenBounds.y + 1.5f);
            Instantiate(bossEnemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }
}