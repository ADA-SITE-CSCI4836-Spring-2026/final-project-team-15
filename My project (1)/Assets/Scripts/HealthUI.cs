using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives an HP bar UI from PlayerHealth events. Drop on the HP bar root and
/// drag in references — no per-game code changes.
///
/// PREFAB SETUP:
///   1. Create UI → Canvas (Screen Space - Overlay) named "HUD" (if not made yet).
///   2. Inside HUD, create UI → Image, name it "HPBarBG" (the dark background).
///   3. Inside HPBarBG, create UI → Image, name it "HPBarFill". Set its
///      Image Type = Filled, Fill Method = Horizontal, Fill Origin = Left.
///      Use a green/red sprite or a flat color.
///   4. Optional: add a TMP_Text child for the numeric HP label.
///   5. Add this script to HPBarBG. Drag the player's PlayerHealth into the
///      health slot, the HPBarFill image into fillBar, and the label if used.
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("If empty, auto-finds any PlayerHealth in the scene.")]
    [SerializeField] private PlayerHealth health;
    [Tooltip("Optional UI Slider — if set, the slider's value is driven by HP.")]
    [SerializeField] private Slider slider;
    [Tooltip("Optional fill Image — used for color tinting (and as fallback if Slider is empty).")]
    [SerializeField] private Image fillBar;
    [Tooltip("Optional: shows current HP as text (e.g. '75 / 100').")]
    [SerializeField] private TMP_Text label;

    [Header("Color thresholds (fraction 0–1)")]
    [SerializeField] private Color healthyColor  = new Color(0.3f, 0.9f, 0.3f);
    [SerializeField] private Color warningColor  = new Color(1f,   0.75f, 0.2f);
    [SerializeField] private Color criticalColor = new Color(1f,   0.25f, 0.25f);
    [SerializeField, Range(0f, 1f)] private float warningThreshold  = 0.5f;
    [SerializeField, Range(0f, 1f)] private float criticalThreshold = 0.25f;

    [Header("Animation")]
    [Tooltip("How fast the bar smoothly chases the actual HP. 0 = instant.")]
    [SerializeField] private float lerpSpeed = 8f;

    private float _displayedFraction = 1f;

    private void OnEnable()
    {
        if (health == null) health = FindObjectOfType<PlayerHealth>();
        if (health != null)
        {
            health.OnHealthChanged.AddListener(OnHealthChanged);
            // Sync immediately in case the event was already fired before we subscribed.
            OnHealthChanged(health.CurrentHealth);
            _displayedFraction = health.NormalizedHealth;
            ApplyVisuals(_displayedFraction);
        }
    }

    private void OnDisable()
    {
        if (health != null) health.OnHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void Update()
    {
        if (health == null || fillBar == null) return;

        float target = health.NormalizedHealth;
        _displayedFraction = lerpSpeed > 0f
            ? Mathf.Lerp(_displayedFraction, target, lerpSpeed * Time.deltaTime)
            : target;

        ApplyVisuals(_displayedFraction);
    }

    private void OnHealthChanged(float current)
    {
        if (label != null && health != null)
            label.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(health.MaxHealth)}";
    }

    private void ApplyVisuals(float fraction)
    {
        // Drive the bar — Slider takes priority, falls back to Image.fillAmount.
        if (slider != null)
            slider.value = fraction * slider.maxValue;
        else if (fillBar != null)
            fillBar.fillAmount = fraction;

        // Tint the fill image regardless of whether a Slider is sizing it.
        if (fillBar != null)
        {
            Color c =
                fraction <= criticalThreshold ? criticalColor :
                fraction <= warningThreshold  ? warningColor  :
                                                healthyColor;
            fillBar.color = c;
        }
    }
}
