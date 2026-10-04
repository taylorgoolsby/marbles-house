using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarbleController : MonoBehaviour
{
    public float torque = 10f;
    public float maxTorque = 20f;
    public float jumpForce = 5f;
    public Transform cameraTransform;

    private Rigidbody rb;

    private bool jumpPressed;
    private bool grounded;

    // We save the average of all contact points per collider, 
    // and the transform.position at the time of OnCollisionStay
    private Dictionary<Collider, (Vector3 point, Vector3 position)> contactPoints =
    new Dictionary<Collider, (Vector3, Vector3)>();

    private Vector3 jumpDirection = Vector3.up;

    public LineRenderer line;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
        
        float length = grounded ? 2f : 0f;
        line.SetPosition(0, transform.position);
        line.SetPosition(
            1,
            transform.position + jumpDirection * length
        );
    }

    void FixedUpdate()
    {
        grounded = contactPoints.Count > 0;

        if (grounded)
        {
            Vector3 averageContactPoint = Vector3.zero;
            Vector3 averagePosition = Vector3.zero;
            foreach (var contact in contactPoints.Values)
            {
                averageContactPoint += contact.point;
                averagePosition += contact.position;
            }
            averageContactPoint /= contactPoints.Count;
            averagePosition /= contactPoints.Count;
            jumpDirection =
                (averagePosition - averageContactPoint).normalized;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y -= 1;

        if (Keyboard.current.sKey.isPressed)
            input.y += 1;

        if (Keyboard.current.aKey.isPressed)
            input.x += 1;

        if (Keyboard.current.dKey.isPressed)
            input.x -= 1;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // Dampen spinning around contact point
        Vector3 spin = Vector3.Project(rb.angularVelocity, jumpDirection);
        rb.AddTorque(-spin * 1f); 

        Vector3 direction =
            forward * input.y +
            right * input.x;

        if (direction.sqrMagnitude > 0)
        {
            Vector3 torqueAxis =
                Vector3.Cross(direction, jumpDirection);

            float appliedTorque =
                Mathf.Min(torque, maxTorque);

            rb.AddTorque(
                torqueAxis.normalized * appliedTorque
            );
        }

        // Debug.Log($"grounded: {grounded}");
        if (jumpPressed && grounded)
        {
            jumpPressed = false;
            grounded = false;

            rb.AddForce(
                jumpDirection * jumpForce,
                ForceMode.Impulse
            );
        }

        jumpPressed = false;
    }

    void OnCollisionStay(Collision collision)
    {
        // Store the average of all contact points for this collider.
        // Note that OnCollisionStay does not always call every frame, so
        // instead we save points OnCollisionStay and remove them in OnCollisionExit
        // because even if OnCollisionStay is not called every frame, OnCollisionExit
        // will not be called until collision ends.
        // It seems OnCollisionStay is not called if the rigidbody goes to sleep, 
        // i.e. the ball stops moving.
        Vector3 average = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            average += collision.GetContact(i).point;
        }
        average /= collision.contactCount;
        contactPoints[collision.collider] = (
            average,
            transform.position
        );
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log($"OnCollisionExit");
        contactPoints.Remove(collision.collider);
    }
}