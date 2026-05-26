using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButtonController : MonoBehaviour
{
    [SerializeField] private SavePanelController savePanelController;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        if (backButton == null)
            backButton = GetComponent<Button>();

        if (savePanelController == null)
            savePanelController = FindSavePanelControllerInScene();

        if (backButton == null)
        {
            Debug.LogWarning("BackButtonController found no Button on " + gameObject.name + ".");
            return;
        }

        backButton.onClick.RemoveListener(OpenSavePanel);
        backButton.onClick.AddListener(OpenSavePanel);
        Debug.Log("BackButtonController bound BackButton on " + gameObject.name + ".");
    }

    public void OpenSavePanel()
    {
        if (savePanelController == null)
            savePanelController = FindSavePanelControllerInScene();

        if (savePanelController == null)
        {
            Debug.LogWarning("BackButtonController could not find SavePanelController in scene " + SceneManager.GetActiveScene().name + ".");
            return;
        }

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
