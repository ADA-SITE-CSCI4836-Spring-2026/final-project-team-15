using UnityEngine;

/// <summary>
/// Tank-style first-person controls.
///   W / S  → walk forward / backward along the player's facing direction.
///   A / D  → turn the player body left / right in place.
///   Shift  → sprint.
///   Space  → jump.
///
/// The mouse does NOT rotate the player body. The FpsCamera script handles
/// stationary peek-look and recenters when the player moves.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.6f;
    [Tooltip("Body turn rate from A/D in degrees per second.")]
    [SerializeField] private float turnSpeed = 120f;

    [Header("Physics")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.4f;
    [SerializeField] private bool allowJump = true;

    private CharacterController _cc;
    private Vector3 _velocity;

    public bool CanMove { get; set; } = true;
    public Vector3 CurrentVelocity => _velocity;
    public bool IsGrounded => _cc != null && _cc.isGrounded;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!CanMove)
        {
            ApplyGravityOnly();
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool jump   = allowJump && Input.GetButtonDown("Jump");

        // A/D rotates the body in world-Y so the camera (child) follows.
        if (Mathf.Abs(h) > 0.01f)
            transform.Rotate(Vector3.up, h * turnSpeed * Time.deltaTime, Space.World);

        // W/S walks along the player's current forward.
        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);
        Vector3 horizontalMove = transform.forward * v * speed;

        if (_cc.isGrounded)
        {
            _velocity.y = -1f;
            if (jump) _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
        }

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
