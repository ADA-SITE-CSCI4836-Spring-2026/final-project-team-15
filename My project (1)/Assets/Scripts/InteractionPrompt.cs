using UnityEngine;
using TMPro;

/// <summary>
/// Finds the nearest Interactable around the player, displays a "Press E" prompt
/// above it, and triggers Interact() on key press.
///
/// SETUP:
///   1. Create an empty GameObject named "InteractionSystem" in your scene.
///   2. Drop this script on it.
///   3. Drag the Player into the player slot.
///   4. Create a UI Canvas (Screen Space - Overlay):
///        Canvas → Image (background pill) → TMP_Text child ("Press E")
///        Drag that whole Canvas GameObject into promptCanvas, and the
///        TMP_Text into promptText.
///   5. Done. Place any Interactable in the world and it'll be picked up.
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Maximum distance from player to detect interactables.")]
    [SerializeField] private float interactionRange = 2.5f;
    [Tooltip("Key the player presses to interact.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [Tooltip("Layers to check for interactables. Default = everything.")]
    [SerializeField] private LayerMask interactableMask = ~0;
    [Tooltip("If true, prefer interactables in front of the player over those behind.")]
    [SerializeField] private bool preferForward = true;

    [Header("References")]
    [SerializeField] private Transform player;
    [Tooltip("Canvas (Screen Space - Overlay) containing the prompt UI.")]
    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private TMP_Text promptText;
    [Tooltip("Camera used to project world position to screen. Auto-assigned to Camera.main.")]
    [SerializeField] private Camera worldCamera;

    private Interactable _currentTarget;
    private static readonly Collider[] _hitBuffer = new Collider[32];

    public Interactable CurrentTarget => _currentTarget;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (worldCamera == null) worldCamera = Camera.main;
        if (promptCanvas != null) promptCanvas.SetActive(false);
    }

    private void Update()
    {
        FindClosestInteractable();
        UpdatePromptUI();
        HandleInput();
    }

    // ── Detection ──────────────────────────────────────────────────
    private void FindClosestInteractable()
    {
        if (player == null) { _currentTarget = null; return; }

        int count = Physics.OverlapSphereNonAlloc(
            player.position, interactionRange, _hitBuffer, interactableMask,
            QueryTriggerInteraction.Collide);

        Interactable best = null;
        float bestScore = float.MaxValue;
        Vector3 fwd = player.forward;

        for (int i = 0; i < count; i++)
        {
            var col = _hitBuffer[i];
            if (col == null) continue;

            var interactable = col.GetComponentInParent<Interactable>();
            if (interactable == null || !interactable.CanInteract(player.gameObject)) continue;

            Vector3 toTarget = interactable.transform.position - player.position;
            float dist = toTarget.magnitude;

            // Lower score = better. Add a penalty for being behind the player.
            float score = dist;
            if (preferForward)
            {
                float dot = Vector3.Dot(fwd.normalized, toTarget.normalized);
                if (dot < 0f) score += 1.5f; // penalize behind-the-back targets
            }

            if (score < bestScore)
            {
                bestScore = score;
                best = interactable;
            }
        }

        _currentTarget = best;
    }

    // ── UI ─────────────────────────────────────────────────────────
    private void UpdatePromptUI()
    {
        if (promptCanvas == null) return;

        if (_currentTarget == null)
        {
            if (promptCanvas.activeSelf) promptCanvas.SetActive(false);
            return;
        }

        if (!promptCanvas.activeSelf) promptCanvas.SetActive(true);

        // Project world position to screen.
        if (worldCamera != null)
        {
            Vector3 worldPos = _currentTarget.GetPromptWorldPosition();
            Vector3 screen = worldCamera.WorldToScreenPoint(worldPos);

            // Hide if behind camera.
            if (screen.z < 0f)
            {
                promptCanvas.SetActive(false);
                return;
            }
            promptCanvas.transform.position = screen;
        }

        if (promptText != null)
            promptText.text = $"[{interactionKey}] {_currentTarget.PromptText}";
    }

    // ── Input ──────────────────────────────────────────────────────
    private void HandleInput()
    {
        if (_currentTarget == null) return;
        if (Input.GetKeyDown(interactionKey))
        {
            _currentTarget.Interact(player != null ? player.gameObject : gameObject);
        }
    }

    // ── Editor visualization ───────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.4f);
        Gizmos.DrawWireSphere(player.position, interactionRange);
    }
}
