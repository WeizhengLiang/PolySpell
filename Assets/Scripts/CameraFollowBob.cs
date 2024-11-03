using UnityEngine;

public class CameraFollowBob : MonoBehaviour
{
    private Transform playerTransform;
    private Camera mainCamera;

    public void SetFollowCam()
    {
        // 找到玩家（bob）
        playerTransform = FindObjectOfType<PlayerController>().transform;
        mainCamera = GetComponent<Camera>();
        
        // 设置相机的视野大小
        mainCamera.orthographicSize = 1.5f; // 可以调整这个值来改变视野范围
    }

    void LateUpdate()
    {
        if (playerTransform != null)
        {
            // 让相机跟随玩家，保持z轴不变
            Vector3 newPosition = playerTransform.position;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
    }
}