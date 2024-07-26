using UnityEngine;

public class SlowMotionManager : MonoBehaviour
{
    public float slowMotionScale = 0.05f;
    private float normalFixedDeltaTime;

    void Start()
    {
        normalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void EnterSlowMotion()
    {
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = normalFixedDeltaTime * slowMotionScale;
    }

    public void ExitSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
    }
}