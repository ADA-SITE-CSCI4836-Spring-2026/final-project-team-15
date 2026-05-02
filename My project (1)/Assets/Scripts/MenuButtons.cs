using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// All UI button handlers in one place. Drop this on a GameObject in your
/// Title/Win/Lose scenes and wire button OnClick() events to its public methods.
///
/// Pulls scene names from GameConfig.Instance — if config is missing, falls back
/// to literal "Title"/"Gameplay"/"Win" so you can still iterate before creating
/// the config asset.
/// </summary>
public class MenuButtons : MonoBehaviour
{
    [Header("Optional per-button overrides")]
    [Tooltip("If set, overrides GameConfig.gameplaySceneName for Start/PlayAgain.")]
    [SerializeField] private string overrideGameplayScene = "";
    [Tooltip("If set, overrides GameConfig.titleSceneName for GoToTitle.")]
    [SerializeField] private string overrideTitleScene = "";

    /// <summary>Wire to "Start" button on Title screen.</summary>
    public void StartGame()
    {
        GameState.ResetAll();
        SceneManager.LoadScene(GameplayScene());
    }

    /// <summary>Wire to "Play Again" button on Win/Lose screens.</summary>
    public void PlayAgain()
    {
        GameState.ResetAll();
        SceneManager.LoadScene(GameplayScene());
    }

    /// <summary>Wire to "Main Menu" or "Back to Title" button.</summary>
    public void GoToTitle()
    {
        SceneManager.LoadScene(TitleScene());
    }

    /// <summary>Wire to "Quit" button. Does nothing in WebGL builds.</summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>Generic — wire from any button with a scene name argument.</summary>
    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    // ── Helpers ────────────────────────────────────────────────────
    private string GameplayScene()
    {
        if (!string.IsNullOrEmpty(overrideGameplayScene)) return overrideGameplayScene;
        var cfg = GameConfig.Instance;
        return cfg != null ? cfg.gameplaySceneName : "Gameplay";
    }

    private string TitleScene()
    {
        if (!string.IsNullOrEmpty(overrideTitleScene)) return overrideTitleScene;
        var cfg = GameConfig.Instance;
        return cfg != null ? cfg.titleSceneName : "Title";
    }
}
