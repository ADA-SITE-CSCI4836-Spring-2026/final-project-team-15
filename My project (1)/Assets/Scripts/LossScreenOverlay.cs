using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime-created loss screen. This avoids needing a separate Lose scene or
/// manual Canvas setup: call Show() when the player dies.
/// </summary>
public class LossScreenOverlay : MonoBehaviour
{
    private const string OverlayName = "LossScreenOverlay";
    private const string MainMenuSceneFallback = "MainMenu (1)";

    private bool _isPauseMenu;

    public static bool IsShowing => FindObjectOfType<LossScreenOverlay>() != null;
    public static bool IsPauseShowing
    {
        get
        {
            LossScreenOverlay overlay = FindObjectOfType<LossScreenOverlay>();
            return overlay != null && overlay._isPauseMenu;
        }
    }

    public static void Show()
    {
        ShowLost();
    }

    public static void ShowLost()
    {
        if (FindObjectOfType<LossScreenOverlay>() != null) return;

        EnsureEventSystem();

        GameObject root = new GameObject(OverlayName);
        DontDestroyOnLoad(root);
        LossScreenOverlay overlay = root.AddComponent<LossScreenOverlay>();
        overlay.Build(
            root,
            "YOU LOST",
            "HP or time reached 0",
            new Color(0.86f, 0.12f, 0.16f),
            new Color(1f, 0.22f, 0.26f),
            new Color(0.58f, 0.06f, 0.09f),
            false);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void ShowPause()
    {
        if (FindObjectOfType<LossScreenOverlay>() != null) return;

        EnsureEventSystem();

        GameObject root = new GameObject(OverlayName);
        DontDestroyOnLoad(root);
        LossScreenOverlay overlay = root.AddComponent<LossScreenOverlay>();
        overlay._isPauseMenu = true;
        overlay.Build(
            root,
            "PAUSED",
            "Press ESC or P to resume",
            new Color(0.18f, 0.32f, 0.62f),
            new Color(0.26f, 0.44f, 0.82f),
            new Color(0.10f, 0.20f, 0.42f),
            true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void ClosePause()
    {
        if (!IsPauseShowing) return;

        Time.timeScale = 1f;
        CloseExisting();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public static void ShowFinish()
    {
        if (FindObjectOfType<LossScreenOverlay>() != null) return;

        EnsureEventSystem();
        GameState.HasWon = true;

        GameObject root = new GameObject(OverlayName);
        DontDestroyOnLoad(root);
        LossScreenOverlay overlay = root.AddComponent<LossScreenOverlay>();
        overlay.Build(
            root,
            "FINISH POINT",
            "You reached the door",
            new Color(0.12f, 0.54f, 0.32f),
            new Color(0.18f, 0.72f, 0.42f),
            new Color(0.06f, 0.34f, 0.18f),
            false);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private void Build(
        GameObject root,
        string titleText,
        string messageText,
        Color buttonColor,
        Color buttonHighlightColor,
        Color buttonPressedColor,
        bool includeResumeButton)
    {
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        Image backdrop = CreateImage("Backdrop", root.transform, new Color(0.03f, 0.04f, 0.06f, 0.88f));
        RectTransform backdropRect = backdrop.rectTransform;
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;

        GameObject panel = new GameObject("Content");
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(720f, 420f);
        panelRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 28f;
        layout.padding = new RectOffset(40, 40, 40, 40);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateText("Title", panel.transform, titleText, 76, FontStyles.Bold, Color.white);
        title.GetComponent<LayoutElement>().preferredHeight = 105f;

        TextMeshProUGUI message = CreateText("Message", panel.transform, messageText, 32, FontStyles.Normal, new Color(0.88f, 0.92f, 0.96f));
        message.GetComponent<LayoutElement>().preferredHeight = 56f;

        GameObject row = new GameObject("Buttons");
        row.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.spacing = 22f;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        LayoutElement rowElement = row.AddComponent<LayoutElement>();
        rowElement.preferredHeight = 72f;

        if (includeResumeButton)
            CreateButton("ResumeButton", row.transform, "Resume", Resume, buttonColor, buttonHighlightColor, buttonPressedColor);

        CreateButton("RestartButton", row.transform, "Restart", Restart, buttonColor, buttonHighlightColor, buttonPressedColor);
        CreateButton("MainMenuButton", row.transform, "Main Menu", GoToMainMenu, buttonColor, buttonHighlightColor, buttonPressedColor);
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image image = obj.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string content, int size, FontStyles style, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 18;
        text.fontSizeMax = size;

        RectTransform rect = text.rectTransform;
        rect.sizeDelta = new Vector2(620f, size + 24f);

        obj.AddComponent<LayoutElement>();
        return text;
    }

    private static void CreateButton(
        string name,
        Transform parent,
        string label,
        UnityEngine.Events.UnityAction action,
        Color buttonColor,
        Color highlightedColor,
        Color pressedColor)
    {
        Image image = CreateImage(name, parent, buttonColor);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = highlightedColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        RectTransform rect = image.rectTransform;
        rect.sizeDelta = new Vector2(230f, 68f);

        LayoutElement element = image.gameObject.AddComponent<LayoutElement>();
        element.preferredWidth = 230f;
        element.preferredHeight = 68f;

        TextMeshProUGUI text = CreateText("Text", image.transform, label, 30, FontStyles.Bold, Color.white);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private static void Restart()
    {
        Time.timeScale = 1f;
        GameState.ResetRound();
        CloseExisting();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void Resume()
    {
        ClosePause();
    }

    private static void GoToMainMenu()
    {
        Time.timeScale = 1f;
        GameState.ResetAll();
        CloseExisting();

        string sceneName = MainMenuSceneName();
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            SceneManager.LoadScene(0);
    }

    private static void CloseExisting()
    {
        LossScreenOverlay overlay = FindObjectOfType<LossScreenOverlay>();
        if (overlay != null)
            Destroy(overlay.gameObject);
    }

    private static string MainMenuSceneName()
    {
        GameConfig config = Resources.Load<GameConfig>("GameConfig");
        if (config != null && Application.CanStreamedLevelBeLoaded(config.titleSceneName))
            return config.titleSceneName;

        return MainMenuSceneFallback;
    }
}
