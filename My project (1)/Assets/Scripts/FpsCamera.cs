using UnityEngine;

/// <summary>
/// First-person camera that lives at the scene ROOT and follows the player.
///
/// Position offset and rotation offset are captured at Play start from the
/// camera's editor pose, so wherever you place + aim the camera in the
/// Scene view becomes the locked relative position/direction to the player.
///
/// While the player stands still, the mouse adds a small clamped peek offset.
/// When the player moves, the peek blends back to forward and the camera
/// stays locked to the player's facing.
///
/// SETUP:
///   1. Main Camera must be a SIBLING of Player (not a child).
///   2. Drag Player into the Player slot.
///   3. In the Scene view, place the camera at the character's head and rotate
///      it to look in the direction the character visually faces.
///   4. Press Play.
/// </summary>
public class FpsCamera : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform player;

    [Header("Mouse Look (peek while standing still)")]
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch =  70f;
    [Tooltip("Max yaw left/right while peeking, in degrees.")]
    [SerializeField] private float maxYaw = 80f;
    [Tooltip("How fast the peek offset blends back to forward when the player moves.")]
    [SerializeField] private float recenterSpeed = 8f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorOnStart = true;

    private Vector3 _localOffset;
    private Quaternion _localRotation;
    private float _yaw;
    private float _pitch;
    private bool _hadValidLockLastFrame;

    private void Start()
    {
        if (player != null)
        {
            _localOffset   = player.InverseTransformPoint(transform.position);
            _localRotation = Quaternion.Inverse(player.rotation) * transform.rotation;
        }

        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !PauseMenuInput.WasPauseInputHandledThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        bool moving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f
                   || Mathf.Abs(Input.GetAxisRaw("Vertical"))   > 0.01f;

        bool isLocked = Cursor.lockState == CursorLockMode.Locked;

        if (!moving && isLocked && _hadValidLockLastFrame)
        {
            // Only read mouse if the cursor was already locked LAST frame too.
            // This skips the first frame after locking, where Unity's Mouse Y/X
            // delta reflects the cursor jumping to the screen center and would
            // otherwise spike the pitch.
            _yaw   += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            _yaw    = Mathf.Clamp(_yaw,   -maxYaw,  maxYaw);
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }
        else if (moving)
        {
            float t = recenterSpeed * Time.deltaTime;
            _yaw   = Mathf.Lerp(_yaw,   0f, t);
            _pitch = Mathf.Lerp(_pitch, 0f, t);
        }

        _hadValidLockLastFrame = isLocked;

        // Position: stay at the captured local-space offset to player.
        transform.position = player.TransformPoint(_localOffset);

        // Rotation: player's rotation applied to the captured initial relative rotation,
        // plus mouse peek offset on top.
        Quaternion baseRot   = player.rotation * _localRotation;
        Quaternion offsetRot = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.rotation   = baseRot * offsetRot;
    }
}
