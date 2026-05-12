using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Placeholder controller for the post-combat story scene.
public class AfterCombatStoryController : MonoBehaviour
{
    [Header("Optional UI References")]
    public TextMeshProUGUI storyText;
    public Button continueButton;

    [Header("Story Text")]
    [TextArea(2, 4)]
    public string placeholderText = "The bathhouse grows quiet after the battle.\n\nPress F or click Continue.";

    private bool storyEnded;

    // Keeps AfterCombatScene runnable even before this script is manually added.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForAfterCombatScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreateForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreateForScene(scene);
    }

    private static void TryCreateForScene(Scene scene)
    {
        if (scene.name != "AfterCombatScene" || FindObjectOfType<AfterCombatStoryController>() != null)
            return;

        new GameObject("AfterCombatStoryController").AddComponent<AfterCombatStoryController>();
    }

    private void Start()
    {
        EnsureGameManagerExists();
        BindExistingOrCreateUI();

        if (storyText != null)
            storyText.text = placeholderText;

        if (continueButton != null)
            continueButton.onClick.AddListener(EndStory);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            EndStory();
    }

    // Called by the Continue button or F key when the placeholder story is done.
    public void EndStory()
    {
        if (storyEnded)
            return;

        storyEnded = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AdvanceDay();

        SceneManager.LoadScene("BathhouseMain");
    }

    private void EnsureGameManagerExists()
    {
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();
    }

    private void BindExistingOrCreateUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        if (storyText == null)
            storyText = FindText("StoryText") ?? CreateText(canvas.transform, "StoryText", new Vector2(0f, 80f), placeholderText);

        if (continueButton == null)
            continueButton = FindButton("ContinueButton") ?? CreateButton(canvas.transform, "ContinueButton", new Vector2(0f, -120f), "Continue");
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        return canvas;
    }

    private TextMeshProUGUI FindText(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<TextMeshProUGUI>() : null;
    }

    private Button FindButton(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<Button>() : null;
    }

    private TextMeshProUGUI CreateText(Transform parent, string objectName, Vector2 position, string text)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(900f, 220f);
        rect.anchoredPosition = position;

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 36f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return label;
    }

    private Button CreateButton(Transform parent, string objectName, Vector2 position, string label)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(260f, 72f);
        rect.anchoredPosition = position;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.92f, 0.92f, 0.92f, 1f);

        TextMeshProUGUI buttonText = CreateText(buttonObject.transform, "Text (TMP)", Vector2.zero, label);
        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.sizeDelta = rect.sizeDelta;
        buttonText.fontSize = 30f;
        buttonText.color = Color.black;

        Button button = buttonObject.GetComponent<Button>();
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        return button;
    }
}
