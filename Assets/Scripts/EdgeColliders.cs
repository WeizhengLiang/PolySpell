using UnityEngine;

public class EdgeColliders : MonoBehaviour
{
    public BoxCollider2D topCollider;
    public BoxCollider2D bottomCollider;
    public BoxCollider2D leftCollider;
    public BoxCollider2D rightCollider;

    void Start()
    {
        AdjustColliders();
    }

    void AdjustColliders()
    {
        Vector2 screenBottomLeft = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 screenTopRight = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
        
        float screenWidth = screenTopRight.x - screenBottomLeft.x;
        float screenHeight = screenTopRight.y - screenBottomLeft.y;

        // Top Collider
        topCollider.size = new Vector2(screenWidth, 1);
        topCollider.offset = new Vector2(0, screenTopRight.y + 0.5f);

        // Bottom Collider
        bottomCollider.size = new Vector2(screenWidth, 1);
        bottomCollider.offset = new Vector2(0, screenBottomLeft.y - 0.5f);

        // Left Collider
        leftCollider.size = new Vector2(1, screenHeight);
        leftCollider.offset = new Vector2(screenBottomLeft.x - 0.5f, 0);

        // Right Collider
        rightCollider.size = new Vector2(1, screenHeight);
        rightCollider.offset = new Vector2(screenTopRight.x + 0.5f, 0);
    }
}