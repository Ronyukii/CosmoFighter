using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnBecameInvisible()
    {
        // Destroys the bullet immediately once it exits all camera perspectives
        Destroy(gameObject);
    }
}