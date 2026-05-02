using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives a timer UI from a CountdownTimer's events. Drop on the timer canvas,
/// drag in references, done. No code changes per game.
///
/// PREFAB SETUP (Person D):
///   1. Create UI → Canvas (Screen Space - Overlay) named "TimerCanvas".
///   2. Add child: UI → Image (the fill bar). Set Image Type = Filled,
///      Fill Method = Horizontal. This becomes "fillBar".
///   3. Add child: UI → Text (TextMeshPro) named "TimerLabel". This becomes "label".
///   4. Add this script to TimerCanvas. Drag the timer (CountdownTimer in scene)
///      into the timer slot, plus label and fillBar.
///   5. Drag TimerCanvas into Project to make it a prefab.
/// </summary>
public class TimerUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("If empty, auto-finds any CountdownTimer in the scene.")]
    [SerializeField] private CountdownTimer timer;
    [SerializeField] private TMP_Text label;
    [Tooltip("Optional fill bar Image (Image Type: Filled).")]
    [SerializeField] private Image fillBar;

    [Header("Color thresholds (seconds)")]
    [SerializeField] private Color normalColor   = Color.white;
    [SerializeField] private Color warningColor  = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.25f, 0.25f);
    [SerializeField] private float warningThreshold  = 10f;
    [SerializeField] private float criticalThreshold = 5f;

    [Header("Format")]
    [Tooltip("If true, show MM:SS. If false, show whole seconds only.")]
    [SerializeField] private bool showMinutes = false;

    private void OnEnable()
    {
        if (timer == null) timer = FindObjectOfType<CountdownTimer>();
        if (timer != null) timer.OnTimerTick.AddListener(UpdateUI);
    }

    private void OnDisable()
    {
        if (timer != null) timer.OnTimerTick.RemoveListener(UpdateUI);
    }

    private void UpdateUI(float remaining)
    {
        if (label != null)
        {
            int totalSeconds = Mathf.CeilToInt(remaining);
            if (showMinutes)
            {
                int m = totalSeconds / 60;
                int s = totalSeconds % 60;
                label.text = $"{m}:{s:00}";
            }
            else
            {
                label.text = totalSeconds.ToString();
            }

            if (remaining <= criticalThreshold) label.color = criticalColor;
            else if (remaining <= warningThreshold) label.color = warningColor;
            else label.color = normalColor;
        }

        if (fillBar != null && timer != null && timer.Duration > 0f)
        {
            fillBar.fillAmount = Mathf.Clamp01(remaining / timer.Duration);
            if (remaining <= criticalThreshold) fillBar.color = criticalColor;
            else if (remaining <= warningThreshold) fillBar.color = warningColor;
            else fillBar.color = normalColor;
        }
    }
}
