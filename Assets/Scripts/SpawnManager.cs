using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Prefab Variants")]
    public GameObject smallEnemyPrefab;
    public GameObject intermediateEnemyPrefab;
    public GameObject bossEnemyPrefab;

    [Header("Dynamic Loop Calibration")]
    public int scoreNeededPerBossCycle = 2000; // How many points the player needs to score to summon the next boss
    public float baseSpawnInterval = 3f;

    private int nextIntermediateMilestone;
    private int nextBossMilestone;
    private int bossCycleCount = 0; // Tracks how many bosses have been beaten to ramp up difficulty

    private Vector2 screenBounds;
    private bool isSpawning = true;
    private bool bossIsActive = false;

    void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            screenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCamera.transform.position.z)));
        }

        // Apply Hard Mode modifiers to the initial launch settings
        if (DifficultyManager.Instance != null && DifficultyManager.Instance.isHardMode)
        {
            scoreNeededPerBossCycle = 4000; // Hard requires double the endurance gap per loop
            baseSpawnInterval = 1.8f;      // Much more aggressive baseline density
        }

        CalculateNextMilestones();
        StartCoroutine(SpawnWaveRoutine());
    }

    // Calculates milestones relative to the player's current progression loop
    void CalculateNextMilestones()
    {
        int currentScore = GameManager.Instance != null ? GameManager.Instance.GetCurrentScore() : 0;

        // Intermediate ships mix in halfway through the current boss cycle
        nextIntermediateMilestone = currentScore + (scoreNeededPerBossCycle / 4);
        nextBossMilestone = currentScore + scoreNeededPerBossCycle;

        bossIsActive = false;
    }

    IEnumerator SpawnWaveRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (isSpawning)
        {
            int currentScore = GameManager.Instance != null ? GameManager.Instance.GetCurrentScore() : 0;

            // Phase 3: Boss Time!
            if (currentScore >= nextBossMilestone)
            {
                if (!bossIsActive)
                {
                    SpawnBoss();
                }
                // Yield the coroutine frame and wait. DO NOT spawn regular enemies while the boss rules the screen.
                yield return new WaitForSeconds(1f);
                continue;
            }

            // If a boss is alive but the score hasn't updated past the milestone yet, hold fire
            if (bossIsActive)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Phase 1 & 2: Dynamic Standard Waves
            float currentSpawnInterval = baseSpawnInterval;
            GameObject enemyToSpawn = smallEnemyPrefab;

            // Scale difficulty down dynamically based on how many bosses you've already destroyed
            // Each successful boss cycle slashes the interval timer down by an extra 15%
            float speedRampReduction = Mathf.Clamp(1f - (bossCycleCount * 0.15f), 0.4f, 1f);

            if (currentScore >= nextIntermediateMilestone)
            {
                // Mix in Intermediate units (50/50 odds) at faster rates
                enemyToSpawn = Random.value > 0.5f ? intermediateEnemyPrefab : smallEnemyPrefab;
                currentSpawnInterval = baseSpawnInterval * 0.5f * speedRampReduction;
            }
            else
            {
                // Scouts only
                enemyToSpawn = smallEnemyPrefab;
                currentSpawnInterval = baseSpawnInterval * speedRampReduction;
            }

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
        bossIsActive = true;
        if (bossEnemyPrefab != null)
        {
            Vector2 spawnPosition = new Vector2(0f, screenBounds.y + 1.5f);
            GameObject bossInstance = Instantiate(bossEnemyPrefab, spawnPosition, Quaternion.identity);

            // Start monitoring when this specific boss is destroyed by the player
            StartCoroutine(TrackBossDeath(bossInstance));
        }
    }

    IEnumerator TrackBossDeath(GameObject bossObject)
    {
        // Keep waiting as long as the boss GameObject exists in the scene
        while (bossObject != null)
        {
            yield return null;
        }

        // --- THE BOSS HAS FALLEN ---
        bossCycleCount++;

        // Recalculate new milestones relative to our new higher score baseline!
        CalculateNextMilestones();
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }
}