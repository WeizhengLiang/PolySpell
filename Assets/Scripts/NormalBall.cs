using UnityEngine;

public class NormalBall : MonoBehaviour
{
    public ObjectPool NormalBallPool;
    public float speed = 1f;  // Speed of the normal ball

    private Vector2 direction;
    void Start()
    {
        // Assign a random direction
        direction = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        // Move the normal ball in the assigned direction
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void ReturnToPool()
    {
        NormalBallPool.ReturnObject(gameObject);
    }
}
