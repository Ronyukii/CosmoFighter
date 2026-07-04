using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum EnemyType { Small, Intermediate, Boss }

    [Header("Enemy Classification")]
    public EnemyType type = EnemyType.Small;
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Movement Profiles")]
    public float entrySpeed = 3f;
    public float sideSpeed = 2f;
    public float sideMovementRange = 2f;

    [Header("Combat & Danmaku")]
    public GameObject enemyBulletPrefab;
    public float fireRate = 2f;
    public float bulletSpeed = 5f;

    private float nextFireTime;
    private float stopYPosition;
    private float startXPosition;
    private bool hasReachedBattleZone = false;

    // Pattern tracking variables
    private int attackPatternCycle = 0;
    private int spiralAngleOffset = 0;

    void Start()
    {
        startXPosition = transform.position.x;
        nextFireTime = Time.time + Random.Range(0.5f, 1.5f);

        // Calculate Hard Mode scaling multipliers
        float healthMultiplier = 1f;
        float speedMultiplier = 1f;
        float attackSpeedMultiplier = 1f;

        if (DifficultyManager.Instance != null && DifficultyManager.Instance.isHardMode)
        {
            healthMultiplier = 1.5f;       // 50% more HP (Endurance test)
            speedMultiplier = 1.3f;        // Faster moving enemies
            attackSpeedMultiplier = 1.4f;  // Shorter window between shots
            bulletSpeed *= 1.4f;           // Faster, harder-to-dodge bullets
        }

        currentHealth = Mathf.RoundToInt(maxHealth * healthMultiplier);
        entrySpeed *= speedMultiplier;
        sideSpeed *= speedMultiplier;
        fireRate /= attackSpeedMultiplier;

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
    }

    void Update()
    {
        HandleMovement();

        if (hasReachedBattleZone && Time.time >= nextFireTime)
        {
            ExecuteAttackPattern();
            nextFireTime = Time.time + fireRate;
            attackPatternCycle++; // Rotate to the next attack state
        }
    }

    void HandleMovement()
    {
        if (!hasReachedBattleZone)
        {
            transform.Translate(Vector2.down * entrySpeed * Time.deltaTime, Space.World);
            if (transform.position.y <= stopYPosition)
            {
                hasReachedBattleZone = true;
            }
        }
        else
        {
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
                // Alternates between 2 distinct patterns
                if (attackPatternCycle % 2 == 0)
                {
                    // Pattern A: Dual shot spread
                    FireBullet(new Vector2(-0.3f, -1f).normalized);
                    FireBullet(new Vector2(0.3f, -1f).normalized);
                }
                else
                {
                    // Pattern B: Concentrated 3-Stream Burst
                    FireBullet(Vector2.down);
                    FireBullet(new Vector2(-0.15f, -1f).normalized);
                    FireBullet(new Vector2(0.15f, -1f).normalized);
                }
                break;

            case EnemyType.Boss:
                // Cycles through 3 diverse bullet hell behaviors
                int patternSelection = attackPatternCycle % 3;

                if (patternSelection == 0)
                {
                    // Pattern 1: Dense Circular Nova Ring
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
                    // Pattern 2: Aimed Spiral Stream
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
                    // Pattern 3: Chaotic Screen Sweep Wall
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
            Destroy(other.gameObject);
            TakeDamage(1);
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