using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum EnemyType { Small, Intermediate, Boss }

    [Header("Enemy Classification")]
    public EnemyType type = EnemyType.Small;
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Movement Profiles")]
    public float entryDuration = 1.5f; // Time in seconds it takes to reach their spot
    public float sideSpeed = 2f;
    public float sideMovementRange = 2.5f;

    [Header("Combat & Danmaku")]
    public GameObject enemyBulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 5f;

    private float nextFireTime;
    private float stopYPosition;
    private float startXPosition;

    // Smooth Entry and Invincibility trackers
    private Vector3 spawnPosition;
    private Vector3 targetBattlePosition;
    private float entryProgress = 0f;
    private bool hasReachedBattleZone = false;
    private bool isInvincible = true;

    // Components
    private SpriteRenderer spriteRenderer;

    // Pattern tracking variables
    private int attackPatternCycle = 0;
    private int spiralAngleOffset = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startXPosition = transform.position.x;
        spawnPosition = transform.position;

        // Calculate Hard Mode scaling multipliers
        float healthMultiplier = 1f;
        float attackSpeedMultiplier = 1f;

        if (DifficultyManager.Instance != null && DifficultyManager.Instance.isHardMode)
        {
            healthMultiplier = 1.5f;
            attackSpeedMultiplier = 1.4f;
            bulletSpeed *= 1.4f;
        }

        currentHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
        fireRate /= attackSpeedMultiplier;

        // Calculate the exact target y-position inside screen bounds
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector2 screenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCamera.transform.position.z)));

            if (type == EnemyType.Boss)
                stopYPosition = screenBounds.y - 1.5f;
            else if (type == EnemyType.Intermediate)
                stopYPosition = screenBounds.y - 3.0f;
            else
                stopYPosition = Random.Range(screenBounds.y - 4.5f, screenBounds.y - 2.0f);
        }

        targetBattlePosition = new Vector3(startXPosition, stopYPosition, transform.position.z);

        // Give the enemy a slight semi-transparent alpha tint at start to show invincibility visual feedback
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0.5f;
            spriteRenderer.color = c;
        }
    }

    void Update()
    {
        HandleMovement();

        if (hasReachedBattleZone && Time.time >= nextFireTime)
        {
            ExecuteAttackPattern();
            nextFireTime = Time.time + fireRate;
            attackPatternCycle++;
        }
    }

    void HandleMovement()
    {
        if (!hasReachedBattleZone)
        {
            // Smoothly interpolate from spawn position to target battle position over entryDuration
            entryProgress += Time.deltaTime / entryDuration;

            // SmoothStep creates an elegant ease-out cushion effect upon arrival
            float smoothT = Mathf.SmoothStep(0f, 1f, entryProgress);
            transform.position = Vector3.Lerp(spawnPosition, targetBattlePosition, smoothT);

            if (entryProgress >= 1f)
            {
                // Arrived safely! Turn off invincibility and begin combat behaviors
                hasReachedBattleZone = true;
                isInvincible = false;
                nextFireTime = Time.time + Random.Range(0.2f, 0.6f); // Slight reaction delay before first shot

                // Restore solid full color so the player knows they are now vulnerable
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = 1f;
                    spriteRenderer.color = c;
                }
            }
        }
        else
        {
            // Horizontal strafing pattern once locked in place on screen
            float newX = startXPosition + Mathf.Sin(Time.time * sideSpeed) * sideMovementRange;
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }
    }

    void ExecuteAttackPattern()
    {
        if (enemyBulletPrefab == null) return;

        switch (type)
        {
            case EnemyType.Small:
                FireBullet(Vector2.down);
                break;

            case EnemyType.Intermediate:
                if (attackPatternCycle % 2 == 0)
                {
                    FireBullet(new Vector2(-0.3f, -1f).normalized);
                    FireBullet(new Vector2(0.3f, -1f).normalized);
                }
                else
                {
                    FireBullet(Vector2.down);
                    FireBullet(new Vector2(-0.15f, -1f).normalized);
                    FireBullet(new Vector2(0.15f, -1f).normalized);
                }
                break;

            case EnemyType.Boss:
                int patternSelection = attackPatternCycle % 3;

                if (patternSelection == 0)
                {
                    int bulletCount = DifficultyManager.Instance != null && DifficultyManager.Instance.isHardMode ? 16 : 10;
                    for (int i = 0; i < bulletCount; i++)
                    {
                        float angle = i * (360f / bulletCount);
                        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                        FireBullet(dir);
                    }
                }
                else if (patternSelection == 1)
                {
                    spiralAngleOffset += 25;
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = (i * 90f) + spiralAngleOffset;
                        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                        FireBullet(dir);
                    }
                }
                else if (patternSelection == 2)
                {
                    float leftBound = -1.5f;
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 sweepDirection = new Vector2(leftBound + (i * 0.75f), -1f).normalized;
                        FireBullet(sweepDirection);
                    }
                }
                break;
        }
    }

    void FireBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            // Clean up player laser immediately on impact, but ignore taking damage if still entering
            Destroy(other.gameObject);

            if (!isInvincible)
            {
                TakeDamage(1);
            }
        }
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            if (GameManager.Instance != null)
            {
                int scoreReward = type == EnemyType.Boss ? 1000 : (type == EnemyType.Intermediate ? 300 : 100);
                GameManager.Instance.AddScore(scoreReward);
            }
            Destroy(gameObject);
        }
    }
}