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
    public GameObject enemyBulletPrefab; // Create an enemy bullet prefab!
    public float fireRate = 2f;
    public float bulletSpeed = 5f;

    private float nextFireTime;
    private float stopYPosition; // The vertical limit where they stop descending
    private float startXPosition;
    private bool hasReachedBattleZone = false;

    void Start()
    {
        currentHealth = maxHealth;
        startXPosition = transform.position.x;
        nextFireTime = Time.time + Random.Range(0.5f, 1.5f); // Stagger initial shots

        // Assign stopping heights based on their classification
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector2 screenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(mainCamera.transform.position.z)));

            if (type == EnemyType.Boss)
                stopYPosition = screenBounds.y - 1.5f; // Boss sits near the very top
            else if (type == EnemyType.Intermediate)
                stopYPosition = screenBounds.y - 3.0f; // Mid-boss sits slightly lower
            else
                stopYPosition = Random.Range(screenBounds.y - 4.5f, screenBounds.y - 2.0f); // Small mobs scatter
        }
    }

    void Update()
    {
        HandleMovement();

        if (hasReachedBattleZone && Time.time >= nextFireTime)
        {
            ExecuteAttackPattern();
            nextFireTime = Time.time + fireRate;
        }
    }

    void HandleMovement()
    {
        if (!hasReachedBattleZone)
        {
            // Descend into the screen sight
            transform.Translate(Vector2.down * entrySpeed * Time.deltaTime, Space.World);
            if (transform.position.y <= stopYPosition)
            {
                hasReachedBattleZone = true;
            }
        }
        else
        {
            // Horizontal strafing pattern once locked on screen
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
                // Small Mobs: Single aimed shot downwards
                FireBullet(Vector2.down);
                break;

            case EnemyType.Intermediate:
                // Intermediate: Variable set (Dual shot spread)
                FireBullet(new Vector2(-0.3f, -1f).normalized);
                FireBullet(new Vector2(0.3f, -1f).normalized);
                break;

            case EnemyType.Boss:
                // Boss: Radical bullet hell burst ring
                int bulletCount = 8;
                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = i * (360f / bulletCount);
                    Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                    FireBullet(dir);
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
        if (other.CompareTag("Bullet")) // Player's bullet hitting enemy
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
                // Give higher points depending on target difficulty
                int scoreReward = type == EnemyType.Boss ? 1000 : (type == EnemyType.Intermediate ? 300 : 100);
                GameManager.Instance.AddScore(scoreReward);
            }
            Destroy(gameObject);
        }
    }
}