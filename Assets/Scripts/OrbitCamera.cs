using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 12f;
    public float minDistance = 5f;
    public float maxDistance = 20f;
    public float orbitSpeed = 180f;
    public float zoomSpeed = 3f;
    public float minVerticalAngle = 5f;
    public float maxVerticalAngle = 80f;

    private float currentYaw = 0f;
    private float currentPitch = 30f;
    private bool isDragging = false;
    private Vector2 lastMousePos;

    void Start()
    {
        if (target == null)
        {
            var tower = GameObject.Find("EiffelTower_Puzzle");
            if (tower != null) target = tower.transform;
        }
        UpdatePosition();
    }

    void Update()
    {
        HandleMouseInput();
        HandleZoom();
        UpdatePosition();
    }

    void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.rightButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePos = mouse.position.ReadValue();
        }
        if (mouse.rightButton.wasReleasedThisFrame)
            isDragging = false;

        if (isDragging)
        {
            Vector2 currentPos = mouse.position.ReadValue();
            Vector2 delta = currentPos - lastMousePos;
            currentYaw += delta.x * orbitSpeed * Time.deltaTime;
            currentPitch -= delta.y * orbitSpeed * Time.deltaTime;
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
            lastMousePos = currentPos;
        }
    }

    void HandleZoom()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        distance -= scroll * zoomSpeed * 0.1f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void UpdatePosition()
    {
        if (target == null) return;
        Quaternion rot = Quaternion.Euler(currentPitch, currentYaw, 0f);
        transform.position = target.position + rot * new Vector3(0, 0, -distance);
        transform.LookAt(target.position + Vector3.up * 3f);
    }
}
