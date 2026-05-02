using UnityEngine;

/// <summary>
/// Camera-relative WASD movement. Works with any camera (Cinemachine third-person,
/// fixed top-down, first-person) because it derives forward/right from the camera.
///
/// SETUP:
///   1. Add a CharacterController component (auto-required).
///   2. Tag the GameObject "Player".
///   3. Drag your main camera into the cameraTransform slot, or leave empty
///      and it'll auto-find Camera.main.
///   4. Add a child mesh — the controller rotates the root toward movement.
///
/// Uses Unity's legacy Input axes ("Horizontal", "Vertical", "Jump"), which work
/// out of the box with no setup. Swap to the new Input System later if needed.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.6f;
    [SerializeField] private float rotationSpeed = 12f;
    [Tooltip("If true, the character rotates to face movement direction. Disable for top-down or strafing controls.")]
    [SerializeField] private bool rotateToMovement = true;

    [Header("Physics")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.4f;
    [SerializeField] private bool allowJump = true;

    [Header("References")]
    [Tooltip("The camera used to derive movement direction. Auto-assigned to Camera.main if empty.")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController _cc;
    private Vector3 _velocity;

    public bool CanMove { get; set; } = true;
    public Vector3 CurrentVelocity => _velocity;
    public bool IsGrounded => _cc != null && _cc.isGrounded;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (!CanMove)
        {
            // Still apply gravity so the character doesn't hover when frozen.
            ApplyGravityOnly();
            return;
        }

        // ── Read input ────────────────────────────────────────────
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool jump = allowJump && Input.GetButtonDown("Jump");

        // ── Camera-relative movement vector ───────────────────────
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right   = cameraTransform != null ? cameraTransform.right   : Vector3.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 input = forward * v + right * h;
        if (input.sqrMagnitude > 1f) input.Normalize();

        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);
        Vector3 horizontalMove = input * speed;

        // ── Rotate toward movement ────────────────────────────────
        if (rotateToMovement && input.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, target, rotationSpeed * Time.deltaTime);
        }

        // ── Gravity & jump ────────────────────────────────────────
        if (_cc.isGrounded)
        {
            _velocity.y = -1f; // small downward to keep grounded flag true
            if (jump) _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
        }

        // ── Apply ─────────────────────────────────────────────────
        Vector3 move = horizontalMove + Vector3.up * _velocity.y;
        _cc.Move(move * Time.deltaTime);
    }

    private void ApplyGravityOnly()
    {
        if (_cc == null) return;
        if (_cc.isGrounded) _velocity.y = -1f;
        else _velocity.y += gravity * Time.deltaTime;
        _cc.Move(Vector3.up * _velocity.y * Time.deltaTime);
    }
}
