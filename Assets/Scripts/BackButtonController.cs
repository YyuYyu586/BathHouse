using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButtonController : MonoBehaviour
{
    [SerializeField] private SavePanelController savePanelController;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private Button backButton;
    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        EnsureReferences();
        BindBackButton();
    }

    private void OnEnable()
    {
        EnsureReferences();
        BindBackButton();
    }

    private void Start()
    {
        EnsureReferences();
        BindBackButton();
    }

    private void Update()
    {
        if (backButton == null || !backButton.interactable || rectTransform == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Camera eventCamera = null;
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            eventCamera = parentCanvas.worldCamera;

        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, eventCamera))
            OpenSavePanel();
    }

    private void EnsureReferences()
    {
        if (backButton == null)
            backButton = GetComponent<Button>();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        if (savePanelController == null)
            savePanelController = FindSavePanelControllerInScene();

        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();
    }

    private void BindBackButton()
    {
        if (backButton == null)
        {
            Debug.LogWarning("BackButtonController found no Button on " + gameObject.name + ".");
            return;
        }

        backButton.onClick.RemoveListener(OpenSavePanel);
        backButton.onClick.AddListener(OpenSavePanel);
    }

    public void OpenSavePanel()
    {
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();

        if (shopManager != null && shopManager.shopPanel != null && shopManager.shopPanel.activeSelf)
        {
            shopManager.CloseShop();
            return;
        }

        if (savePanelController == null)
            savePanelController = FindSavePanelControllerInScene();

        if (savePanelController == null)
        {
            Debug.LogWarning("BackButtonController could not find SavePanelController in scene " + SceneManager.GetActiveScene().name + ".");
            return;
        }

        if (SavePanelController.IsPanelOpen)
        {
            savePanelController.ClosePanel();
            return;
        }

        if (!savePanelController.gameObject.activeSelf)
            savePanelController.gameObject.SetActive(true);

        savePanelController.OpenPanel();
    }

    private SavePanelController FindSavePanelControllerInScene()
    {
        SavePanelController[] controllers = Resources.FindObjectsOfTypeAll<SavePanelController>();
        Scene activeScene = SceneManager.GetActiveScene();

        foreach (SavePanelController controller in controllers)
        {
            if (controller != null && controller.gameObject.scene == activeScene)
                return controller;
        }

        return null;
    }
}
