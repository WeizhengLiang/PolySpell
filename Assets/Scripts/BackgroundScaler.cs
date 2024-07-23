using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        AdjustBackgroundSize();
    }

    void AdjustBackgroundSize()
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = Camera.main.orthographicSize * 2;
        Vector2 cameraSize = new Vector2(cameraHeight * screenAspect, cameraHeight);

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        Vector2 scale = transform.localScale;

        float widthRatio = cameraSize.x / spriteSize.x;
        float heightRatio = cameraSize.y / spriteSize.y;
        float scaleFactor = Mathf.Max(widthRatio, heightRatio);

        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    }
}