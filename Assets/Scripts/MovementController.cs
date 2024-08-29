using UnityEngine;

public class MovementController
{
    private Rigidbody2D rb;
    public Vector2 CurrentDirection { get; private set; }
    private float bounceForce = 5f;
    private float shootForce = 8f;

    public MovementController(Rigidbody2D rb)
    {
        this.rb = rb;
    }

    public void SetDirection(Vector2 direction)
    {
        CurrentDirection = direction.normalized;
    }

    public void Shoot()
    {
        rb.velocity = CurrentDirection * shootForce;
    }

    public void Bounce(Vector2 normal)
    {
        var reflection = CurrentDirection - 2 * Vector2.Dot(CurrentDirection, normal) * normal;
        CurrentDirection = reflection;
        rb.velocity = CurrentDirection * bounceForce;
    }
}