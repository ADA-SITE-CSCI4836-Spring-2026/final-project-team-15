using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Reusable countdown timer. Wire UnityEvents in the inspector — no code needed
/// for tick/complete reactions. Drop on any GameObject and configure duration.
///
/// Common uses:
///   - Round timer (UI updates via OnTimerTick, scene reset via OnTimerCompleted)
///   - Respawn delay
///   - Power-up duration
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Total countdown length in seconds.")]
    [SerializeField] private float startDuration = 60f;
    [Tooltip("If true, timer auto-starts when the GameObject becomes active.")]
    [SerializeField] private bool startOnEnable = true;
    [Tooltip("If true, OnTimerCompleted fires only once even if Update runs further.")]
    [SerializeField] private bool fireCompletedOnce = true;

    [Header("Events")]
    public UnityEvent OnTimerStarted;
    public UnityEvent<float> OnTimerTick;       // emits remaining seconds every frame
    public UnityEvent OnTimerCompleted;

    // ── Public state ───────────────────────────────────────────────
    public float TimeRemaining { get; private set; }
    public float Duration => startDuration;
    public bool IsRunning { get; private set; }
    public bool HasCompleted { get; private set; }
    public float NormalizedProgress =>
        startDuration > 0f ? Mathf.Clamp01(1f - (TimeRemaining / startDuration)) : 1f;

    // ── Lifecycle ──────────────────────────────────────────────────
    private void OnEnable()
    {
        if (startOnEnable) StartTimer();
    }

    // ── Public API ─────────────────────────────────────────────────
    public void StartTimer()
    {
        TimeRemaining = startDuration;
        IsRunning = true;
        HasCompleted = false;
        OnTimerStarted?.Invoke();
        OnTimerTick?.Invoke(TimeRemaining);
    }

    public void StartTimer(float duration)
    {
        startDuration = duration;
        StartTimer();
    }

    public void Pause()  => IsRunning = false;
    public void Resume() => IsRunning = !HasCompleted;

    public void ResetTimer()
    {
        TimeRemaining = startDuration;
        HasCompleted = false;
        OnTimerTick?.Invoke(TimeRemaining);
    }

    public void StopTimer()
    {
        IsRunning = false;
        TimeRemaining = 0f;
        OnTimerTick?.Invoke(TimeRemaining);
    }

    public void AddTime(float seconds)
    {
        TimeRemaining = Mathf.Max(0f, TimeRemaining + seconds);
    }

    // ── Tick ───────────────────────────────────────────────────────
    private void Update()
    {
        if (!IsRunning) return;

        TimeRemaining -= Time.deltaTime;
        OnTimerTick?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsRunning = false;
            if (!fireCompletedOnce || !HasCompleted)
            {
                HasCompleted = true;
                OnTimerCompleted?.Invoke();
            }
        }
    }
}
