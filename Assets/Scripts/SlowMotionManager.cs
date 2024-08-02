using UnityEngine;

public class SlowMotionManager : MonoBehaviour
{
    public bool inSlowMotion;
    public float slowMotionScale = 0.05f;
    private float normalFixedDeltaTime;

    void Start()
    {
        normalFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void EnterSlowMotion()
    {
        inSlowMotion = true;
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = normalFixedDeltaTime * slowMotionScale;
    }

    public void ExitSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = normalFixedDeltaTime;
        inSlowMotion = false;
    }
}