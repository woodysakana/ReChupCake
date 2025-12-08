using UnityEngine;
using UnityEngine.InputSystem; // 新輸入系統


public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform boardCenter;

    private GridManager gridManager;

    [Header("Rotation")]
    public float rotateSpeed = 15f;
    private float angle = 0f;

    [Header("Zoom")]
    public float minDistance = 5f;
    public float maxDistance = 20f;
    public float zoomSpeed = 5f;
    private float distance = 20f;

    [Header("Vertical Limit")]
    public float minAngle = 30f;
    public float maxAngle = 80f;
    private float currentPitch = 35f;



    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();

    }
    public void FindCenter()
    {
        if (gridManager != null)
        {
            boardCenter = gridManager.boardCenter;
        }
        else
        {
            Debug.LogWarning("CameraController: 沒找到 GridManager，自動使用 (0,0,0)");
            GameObject go = new GameObject("BoardCenter_Default");
            boardCenter = go.transform;
        }
    }

    void Update()
    {
        if (boardCenter == null) return;

        HandleRotation();
        HandleZoom();

        Quaternion rotation = Quaternion.Euler(currentPitch, angle, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = boardCenter.position + offset;

        transform.LookAt(boardCenter.position);
    }

    void HandleRotation()
    {
        // 新輸入系統：右鍵檢測
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue(); // 滑鼠移動
            angle += delta.x * rotateSpeed * Time.deltaTime;
            currentPitch = Mathf.Clamp(currentPitch - delta.y * Time.deltaTime * rotateSpeed, minAngle, maxAngle);
        }
    }

    void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y; // 滾輪輸入
        distance = Mathf.Clamp(distance - scroll * 0.1f * zoomSpeed, minDistance, maxDistance);
    }
}
