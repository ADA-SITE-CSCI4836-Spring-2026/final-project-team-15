using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Reloads the current scene (or loads a named one). All public methods can
/// be wired directly to UnityEvents — e.g. CountdownTimer.OnTimerCompleted →
/// SceneResetter.ResetScene.
/// </summary>
public class SceneResetter : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Seconds to wait before actually reloading. Useful for showing a 'You died' fade.")]
    [SerializeField] private float resetDelay = 1f;
    [Tooltip("If true, GameState.AttemptNumber increments on each reset.")]
    [SerializeField] private bool incrementAttempt = true;
    [Tooltip("If true, score and round-level flags reset. Best time and attempt count are preserved.")]
    [SerializeField] private bool resetRoundState = true;

    /// <summary>Reload the active scene after the configured delay.</summary>
    public void ResetScene()
    {
        if (resetDelay > 0f) Invoke(nameof(DoReset), resetDelay);
        else DoReset();
    }

    /// <summary>Reload immediately, ignoring the delay.</summary>
    public void ResetSceneImmediate() => DoReset();

    /// <summary>Load a specific scene by name (set up in Build Settings).</summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneResetter] LoadScene called with empty name.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>Load a scene by build index.</summary>
    public void LoadSceneByIndex(int buildIndex) => SceneManager.LoadScene(buildIndex);

    private void DoReset()
    {
        if (incrementAttempt) GameState.IncrementAttempt();
        if (resetRoundState)  GameState.ResetRound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
