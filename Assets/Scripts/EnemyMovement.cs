using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;

    void Update()
    {
        // Move the enemy downwards over time
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    // This detects 2D trigger collisions
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object hitting the enemy is tagged as a Bullet
        if (other.CompareTag("Bullet"))
        {
            // 1. Destroy the bullet object
            Destroy(other.gameObject);

            // 2. Destroy this enemy object
            Destroy(gameObject);

            // TODO: Add points to the scoring system here later!
        }
    }

    void OnBecameInvisible()
    {
        // Clean up the enemy object once it completely exits the bottom of the screen
        Destroy(gameObject);
    }
}