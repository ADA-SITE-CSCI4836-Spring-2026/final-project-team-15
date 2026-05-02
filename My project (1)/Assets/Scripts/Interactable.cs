using UnityEngine;

/// <summary>
/// Base class for anything the player can interact with (doors, pickups, NPCs).
/// Inherit, override Interact(), and you're done — InteractionPrompt finds it
/// automatically via OverlapSphere.
///
/// Requires a Collider on the same GameObject (any type, Trigger or solid).
///
/// EXAMPLE:
///   public class CoinPickup : Interactable {
///       public override void Interact(GameObject who) {
///           GameState.AddScore(10);
///           Destroy(gameObject);
///       }
///   }
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable")]
    [Tooltip("Text shown in the interaction prompt. Keep short — e.g. 'Open', 'Take', 'Talk'.")]
    [SerializeField] private string promptText = "Interact";
    [Tooltip("Disable to hide the prompt and ignore input on this object.")]
    [SerializeField] private bool isEnabled = true;
    [Tooltip("Optional anchor for the prompt. Defaults to top of collider bounds.")]
    [SerializeField] private Transform promptAnchor;

    public string PromptText => promptText;

    public bool IsEnabled
    {
        get => isEnabled;
        set => isEnabled = value;
    }

    /// <summary>Called when the player presses the interaction key while in range.</summary>
    public abstract void Interact(GameObject interactor);

    /// <summary>Override to gate interaction (e.g. door requires key).</summary>
    public virtual bool CanInteract(GameObject interactor) => isEnabled;

    /// <summary>World position where the prompt UI should appear.</summary>
    public virtual Vector3 GetPromptWorldPosition()
    {
        if (promptAnchor != null) return promptAnchor.position;

        var col = GetComponent<Collider>();
        if (col != null) return col.bounds.center + Vector3.up * (col.bounds.extents.y + 0.3f);
        return transform.position + Vector3.up * 1.5f;
    }
}
