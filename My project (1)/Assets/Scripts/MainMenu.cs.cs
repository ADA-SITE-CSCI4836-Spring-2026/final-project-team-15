using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Optional UI")]
    [SerializeField] private GameObject instructionsPanel;

    // Wire this to the Start button OnClick().
    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogWarning("[MainMenu] Game scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError(
                $"[MainMenu] Scene '{gameSceneName}' is not in Build Settings. " +
                "Add it via File > Build Settings > Scenes In Build.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // Wire this to Rules/Instructions button OnClick().
    public void OpenInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    // Wire this to close button on instructions panel.
    public void CloseInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }
}
