using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement2D : MonoBehaviour
{
    private const float MinDirectionSqrMagnitude = 0.001f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float thrustForce = 1.0f;
    [SerializeField, Min(0f)] private float maxSpeed = 5.0f;

    [Header("Input Actions")]
    [SerializeField] private InputAction moveForward;
    [SerializeField] private InputAction lookPosition;

    private Rigidbody2D rb;
    private Camera mainCamera;

    private bool canMove;
    private bool isThrusting;

    private Vector2 thrustDirection = Vector2.up;

    public Rigidbody2D Body => rb;
    public bool IsMovementEnabled => canMove;
    public bool IsThrusting => isThrusting;
    public Vector2 ThrustDirection => thrustDirection;
    public Vector2 FacingDirection => transform.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();

        canMove = false;
        isThrusting = false;
    }

    public void SetMovementEnabled(bool isEnabled)
    {
        canMove = isEnabled;

        if (!canMove)
        {
            isThrusting = false;
        }
    }

    public void TickInput()
    {
        if (!canMove)
        {
            isThrusting = false;
            return;
        }

        isThrusting = moveForward != null && moveForward.IsPressed();

        if (!isThrusting)
        {
            return;
        }

        UpdateThrustDirectionFromPointer();
    }

    public void FixedTickMovement()
    {
        if (!canMove || !isThrusting)
        {
            return;
        }

        rb.AddForce(thrustDirection * thrustForce);
        ClampSpeed();
    }

    public bool TryGetPointerWorldPosition(out Vector2 worldPosition)
    {
        worldPosition = Vector2.zero;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null || lookPosition == null)
        {
            return false;
        }

        Vector2 screenPosition = lookPosition.ReadValue<Vector2>();
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPoint.z = 0f;

        worldPosition = worldPoint;
        return true;
    }

    public void StopMovement()
    {
        isThrusting = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void UpdateThrustDirectionFromPointer()
    {
        if (!TryGetPointerWorldPosition(out Vector2 worldPosition))
        {
            return;
        }

        Vector2 direction = worldPosition - (Vector2)transform.position;

        // Avoid unstable rotation when the pointer is too close to the player.
        if (direction.sqrMagnitude <= MinDirectionSqrMagnitude)
        {
            return;
        }

        thrustDirection = direction.normalized;
        transform.up = thrustDirection;
    }

    private void ClampSpeed()
    {
        if (rb == null)
        {
            return;
        }

        if (maxSpeed <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentSpeed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void EnableInputActions()
    {
        if (moveForward != null)
        {
            moveForward.Enable();
        }

        if (lookPosition != null)
        {
            lookPosition.Enable();
        }
    }

    private void DisableInputActions()
    {
        if (moveForward != null)
        {
            moveForward.Disable();
        }

        if (lookPosition != null)
        {
            lookPosition.Disable();
        }
    }

    private void OnValidate()
    {
        thrustForce = Mathf.Max(0f, thrustForce);
        maxSpeed = Mathf.Max(0f, maxSpeed);
    }
}