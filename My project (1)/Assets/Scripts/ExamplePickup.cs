using UnityEngine;

/// <summary>
/// Concrete example showing how to extend Interactable. Use this as a template:
/// copy, rename, change Interact() body. The InteractionPrompt picks it up
/// automatically — no extra wiring.
///
/// Place this on a cube tagged as a coin/key/item, with any Collider.
/// </summary>
public class ExamplePickup : Interactable
{
    [Header("Pickup")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupVfx;

    public override void Interact(GameObject interactor)
    {
        // Award score.
        GameState.AddScore(scoreValue);

        // Audio.
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // VFX.
        if (pickupVfx != null)
            Instantiate(pickupVfx, transform.position, Quaternion.identity);

        // Remove from world.
        Destroy(gameObject);
    }
}
