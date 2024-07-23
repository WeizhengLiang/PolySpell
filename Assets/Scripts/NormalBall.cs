using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class NormalBall : MonoBehaviour
{
    public ObjectPool NormalBallPool;
    public float speed = 1f;  // Speed of the normal ball
    
    public enum PowerUpType { None, Size, Speed }
    public PowerUpType powerUp;
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI Text;

    private Vector2 direction;
    void Start()
    {
        // Assign a random direction
        direction = Random.insideUnitCircle.normalized;
        spriteRenderer = GetComponent<SpriteRenderer>();
        AssignPowerUp();
    }

    void Update()
    {
        // Move the normal ball in the assigned direction
        transform.Translate(direction * speed * Time.deltaTime);
    }
    
    void AssignPowerUp()
    {
        float chance = Random.Range(0f, 1f);
        if (chance < 0.2f)  // 20% chance to be a power-up ball
        {
            powerUp = (PowerUpType)Random.Range(1, 3);
            switch (powerUp)
            {
                case PowerUpType.Size:
                    spriteRenderer.color = Color.yellow;  // Indicate size power-up with yellow color
                    Text.text = "Size";
                    break;
                case PowerUpType.Speed:
                    spriteRenderer.color = Color.magenta;  // Indicate speed power-up with blue color
                    Text.text = "Speed";
                    break;
            }
        }
        else
        {
            powerUp = PowerUpType.None;
            spriteRenderer.color = Color.blue;  // Normal ball color
            Text.text = "Normal";
        }
    }

    private void OnBecameInvisible()
    {
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        NormalBallPool.ReturnObject(gameObject);
    }
}
