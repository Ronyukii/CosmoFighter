using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    private Vector2 moveInput;
    private Rigidbody2D rb;

    [Header("Shooting")]
    public GameObject Bullet;
    public Transform firePoint;
    public float bulletSpeed = 12f;
    public float fireRate = 0.15f;
    private float nextFireTime;

    [Header("Health System")]
    public int maxHealth = 3;
    private int currentHealth;

    private Vector2 screenBounds;
    private float objectWidth;
    private float objectHeight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Calculate screen boundaries based on Main Camera to keep player on-screen
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            screenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.transform.position.z));
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            objectWidth = spriteRenderer.bounds.extents.x;
            objectHeight = spriteRenderer.bounds.extents.y;
        }

        currentHealth = maxHealth;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(currentHealth);
        }
    }

    void Update()
    {
        // Gather directional input (WASD / Arrow Keys)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Handle shooting mechanics
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FixedUpdate()
    {
        // Apply smooth kinematic movement
        Vector2 nextPosition = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;

        // Clamp position within visible screen bounds
        nextPosition.x = Mathf.Clamp(nextPosition.x, -screenBounds.x + objectWidth, screenBounds.x - objectWidth);
        nextPosition.y = Mathf.Clamp(nextPosition.y, -screenBounds.y + objectHeight, screenBounds.y - objectHeight);

        rb.MovePosition(nextPosition);
    }

    void Shoot()
    {
        if (Bullet != null && firePoint != null)
        {
            // Instantiate the projectile placeholder
            GameObject bullet = Instantiate(Bullet, firePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity = Vector2.up * bulletSpeed;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If an enemy ship hits the player directly
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
            Destroy(other.gameObject); // Destroy the crashing enemy
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverScreen();
        }
        Destroy(gameObject); // Remove the player ship from the board
    }
}