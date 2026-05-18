using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Progress")]
    public int currentDay = 1;
    public int maxDay = 7;

    [Header("Player Data")]
    public int playerGold = 20;
    public int playerHP = 100;
    public int playerSP = 50;

    [Header("Shop Items")]
    public int waterLadleCount = 0;
    public int soapCount = 0;
    public int teaCount = 0;
    public int towelCount = 0;
    public bool hasWaterLadle = false;
    public bool hasGoldenTowel = false;

    public bool IsFinalDay
    {
        get { return currentDay >= maxDay; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ClampDay();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            ResetGame();
        }
    }

    // Creates a temporary GameManager when testing scenes directly from the editor.
    public static GameManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject("GameManager");
        return managerObject.AddComponent<GameManager>();
    }

    // Resets persistent data when starting a new run from MainMenu.
    public void ResetGame()
    {
        currentDay = 1;
        playerGold = 20;
        playerHP = 100;
        playerSP = 50;
        waterLadleCount = 0;
        soapCount = 0;
        teaCount = 0;
        towelCount = 0;
        hasWaterLadle = false;
        hasGoldenTowel = false;
    }

    public void GoNextDay()
    {
        AdvanceDay();
    }

    public void AdvanceDay()
    {
        if (currentDay < maxDay)
        {
            currentDay++;
        }

        ClampDay();
        playerHP = 100;
        playerSP = 50;
    }

    private void ClampDay()
    {
        currentDay = Mathf.Clamp(currentDay, 1, maxDay);
    }
}
