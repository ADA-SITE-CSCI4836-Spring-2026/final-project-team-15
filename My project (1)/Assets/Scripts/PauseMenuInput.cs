using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global pause-menu input. Auto-installs at runtime, so no Unity scene setup is
/// needed. Press Escape or P during gameplay to pause/resume.
/// </summary>
public class PauseMenuInput : MonoBehaviour
{
    private static PauseMenuInput _instance;
    private static int _lastHandledInputFrame = -1;

    public static bool WasPauseInputHandledThisFrame => _lastHandledInputFrame == Time.frameCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (_instance != null) return;

        GameObject obj = new GameObject("PauseMenuInput");
        DontDestroyOnLoad(obj);
        _instance = obj.AddComponent<PauseMenuInput>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.P)) return;
        if (!IsGameplayScene()) return;

        if (LossScreenOverlay.IsPauseShowing)
        {
            _lastHandledInputFrame = Time.frameCount;
            LossScreenOverlay.ClosePause();
        }
        else if (!LossScreenOverlay.IsShowing)
        {
            _lastHandledInputFrame = Time.frameCount;
            LossScreenOverlay.ShowPause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
    }

    private static bool IsGameplayScene()
    {
        return FindObjectOfType<PlayerHealth>() != null || GameObject.Find("Player") != null;
    }
}
