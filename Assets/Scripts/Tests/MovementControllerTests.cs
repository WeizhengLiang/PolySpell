using UnityEngine;
using NUnit.Framework;

public class MovementControllerTests
{
    private GameObject testObject;
    private MovementController movementController;
    private Rigidbody2D mockRigidbody;

    [SetUp]
    public void Setup()
    {
        // 创建一个模拟的Rigidbody2D
        GameObject gameObject = new GameObject();
        mockRigidbody = gameObject.AddComponent<Rigidbody2D>();
        movementController = new MovementController(mockRigidbody);
    }

    [Test]
    public void SetDirection_ShouldNormalizeDirection()
    {
        Vector2 inputDirection = new Vector2(3, 4);
        movementController.SetDirection(inputDirection);

        Vector2 expectedDirection = inputDirection.normalized;
        Assert.That(movementController.CurrentDirection, Is.EqualTo(expectedDirection).Within(0.0001f));
    }

    [Test]
    public void Shoot_ShouldSetVelocityInCurrentDirection()
    {
        Vector2 direction = new Vector2(1, 0);
        movementController.SetDirection(direction);
        movementController.Shoot();

        Vector2 expectedVelocity = direction * 8f; // 8f是shootForce的值
        Assert.That(mockRigidbody.velocity, Is.EqualTo(expectedVelocity).Within(0.0001f));
    }

    [Test]
    public void Bounce_ShouldReflectDirection()
    {
        Vector2 initialDirection = new Vector2(1, 0);
        movementController.SetDirection(initialDirection);

        Vector2 normal = new Vector2(0, 1);
        movementController.Bounce(normal);

        Vector2 expectedDirection = new Vector2(1, 0); // 反弹后的方向
        Assert.That(movementController.CurrentDirection, Is.EqualTo(expectedDirection).Within(0.0001f));

        Vector2 expectedVelocity = expectedDirection * 5f; // 5f是bounceForce的值
        Assert.That(mockRigidbody.velocity, Is.EqualTo(expectedVelocity).Within(0.0001f));
    }

    [TearDown]
    public void Teardown()
    {
        if (testObject != null)
        {
            #if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(testObject);
            #else
                UnityEngine.Object.Destroy(testObject);
            #endif
        }
    }
}