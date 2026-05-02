using UnityEngine;

/// <summary>
/// Central configuration asset. Lives in Resources so any script can grab it
/// via GameConfig.Instance — no inspector wiring needed.
///
/// CREATE IT (do this once, tonight):
///   1. Right-click in Project: Create → Jam → Game Config
///   2. Name it exactly "GameConfig"
///   3. Move it to a folder named "Resources" (must be exact spelling).
///      Final path: Assets/Resources/GameConfig.asset
///   4. Tweak values in the inspector at any time during the jam.
///
/// Anyone on the team can now read values via GameConfig.Instance.roundDuration
/// (etc.) from any script.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Jam/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Theme")]
    public string gameTitle = "Untitled Jam Game";
    [Tooltip("The Stage 2 theme word — fill in once it's announced.")]
    public string themeWord = "TBD";

    [Header("Timing")]
    [Tooltip("Round duration in seconds (used by CountdownTimer prefabs that read from config).")]
    public float roundDuration = 60f;
    public int totalLevels = 3;

    [Header("Player")]
    public float playerMoveSpeed = 5f;
    public float playerSprintMultiplier = 1.6f;
    public float playerJumpHeight = 1.4f;

    [Header("Scoring")]
    public int pointsPerPickup = 10;
    public int targetScore = 100;

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip timerTickSound;
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Visuals")]
    public Color primaryColor = Color.white;
    public Color accentColor  = new Color(0.2f, 0.8f, 1f);
    public Color dangerColor  = new Color(1f, 0.3f, 0.3f);

    [Header("Scenes")]
    [Tooltip("Scene names must match exactly and be added to Build Settings.")]
    public string titleSceneName    = "Title";
    public string gameplaySceneName = "Gameplay";
    public string winSceneName      = "Win";
    public string loseSceneName     = "Lose";

    // ── Singleton accessor ─────────────────────────────────────────
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");
                if (_instance == null)
                {
                    Debug.LogError(
                        "[GameConfig] Could not find Resources/GameConfig.asset. " +
                        "Create one via: Right-click → Create → Jam → Game Config, " +
                        "then place it in Assets/Resources/ named 'GameConfig'.");
                }
            }
            return _instance;
        }
    }
}
