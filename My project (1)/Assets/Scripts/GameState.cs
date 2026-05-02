using System.Collections.Generic;

/// <summary>
/// Static persistent state that survives scene reloads. Use for anything that
/// must outlive a scene: score, attempt counter, current level, win flag, etc.
///
/// Static state is wiped only when the application closes. Call ResetAll()
/// when the player starts a new run from the title screen.
///
/// For ad-hoc data during the jam (e.g. "did the player pick up the key?"),
/// use the flag system: GameState.SetFlag("hasKey", true).
/// </summary>
public static class GameState
{
    // ── Core slots ─────────────────────────────────────────────────
    public static int Score;
    public static int AttemptNumber = 1;
    public static int CurrentLevelIndex;
    public static bool HasWon;
    public static float TimeElapsed;
    public static float BestTime = float.MaxValue;

    // ── Generic flags for jam flexibility ──────────────────────────
    private static readonly Dictionary<string, object> _flags = new();

    public static void SetFlag(string key, object value) => _flags[key] = value;

    public static T GetFlag<T>(string key, T defaultValue = default)
    {
        if (_flags.TryGetValue(key, out var v) && v is T t) return t;
        return defaultValue;
    }

    public static bool HasFlag(string key) => _flags.ContainsKey(key);
    public static void ClearFlag(string key) => _flags.Remove(key);

    // ── Helpers ────────────────────────────────────────────────────
    public static void IncrementAttempt() => AttemptNumber++;
    public static void AddScore(int amount) => Score += amount;

    /// <summary>
    /// Wipe everything. Call this from the Title screen "Start" button.
    /// </summary>
    public static void ResetAll()
    {
        Score = 0;
        AttemptNumber = 1;
        CurrentLevelIndex = 0;
        HasWon = false;
        TimeElapsed = 0f;
        _flags.Clear();
    }

    /// <summary>
    /// Reset only round-level state (keeps best time, attempt counter).
    /// Call this on scene reload during a single play session.
    /// </summary>
    public static void ResetRound()
    {
        Score = 0;
        HasWon = false;
        TimeElapsed = 0f;
        _flags.Clear();
    }
}
