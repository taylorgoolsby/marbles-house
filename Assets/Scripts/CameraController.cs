using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float distance = 8f;
    public float height = 5f;
    public float rotationSpeed = 90f;

    private float yaw = 0f;
    private float pitch = 30f;

    void LateUpdate()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.leftArrowKey.isPressed)
            input.x += 1;

        if (Keyboard.current.rightArrowKey.isPressed)
            input.x -= 1;

        if (Keyboard.current.upArrowKey.isPressed)
            input.y -= 1;

        if (Keyboard.current.downArrowKey.isPressed)
            input.y += 1;

        yaw += input.x * rotationSpeed * Time.deltaTime;
        pitch -= input.y * rotationSpeed * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, 5f, 80f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}