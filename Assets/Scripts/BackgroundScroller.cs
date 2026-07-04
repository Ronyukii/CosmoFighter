using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scrolling Controls")]
    public float scrollSpeed = 0.2f; // Adjust this to speed up or slow down the flight effect

    private SpriteRenderer spriteRenderer;
    private Material backgroundMaterial;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Access the instance of the material assigned to this renderer
        if (spriteRenderer != null)
        {
            backgroundMaterial = spriteRenderer.material;
        }
    }

    void Update()
    {
        // Calculate the vertical offset based on running time
        Vector2 offset = backgroundMaterial.mainTextureOffset;
        offset.y += scrollSpeed * Time.deltaTime;

        // Apply the new offset to slide the texture infinitely
        backgroundMaterial.mainTextureOffset = offset;
    }
}