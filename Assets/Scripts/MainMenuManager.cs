using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Cloud Animation")]
    [SerializeField] private RectTransform[] clouds;
    [SerializeField] private float[] cloudSpeeds = { 18f, 10f, 14f };
    [SerializeField] private float resetLeftX = -1250f;
    [SerializeField] private float resetRightX = 1250f;

    private void Awake()
    {
        // 如果 Inspector 没有手动拖引用，就按名字自动找到 MainMenu 里的云。
        if (clouds == null || clouds.Length == 0)
        {
            clouds = new[]
            {
                FindCloud("Cloud_1"),
                FindCloud("Cloud_2"),
                FindCloud("Cloud_3")
            };
        }
    }

    private void Update()
    {
        MoveClouds();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("StoryScene");
    }

    public void QuitGame()
    {
        Debug.Log("已经点击了退出游戏");
        Application.Quit();
    }

    private RectTransform FindCloud(string cloudName)
    {
        GameObject cloudObject = GameObject.Find(cloudName);
        return cloudObject != null ? cloudObject.GetComponent<RectTransform>() : null;
    }

    // 让云从左往右缓慢移动，移出画面后回到左侧继续循环。
    private void MoveClouds()
    {
        if (clouds == null)
        {
            return;
        }

        for (int i = 0; i < clouds.Length; i++)
        {
            RectTransform cloud = clouds[i];
            if (cloud == null)
            {
                continue;
            }

            float speed = i < cloudSpeeds.Length ? cloudSpeeds[i] : cloudSpeeds[0];
            Vector2 position = cloud.anchoredPosition;
            position.x += speed * Time.deltaTime;

            if (position.x > resetRightX)
            {
                position.x = resetLeftX;
            }

            cloud.anchoredPosition = position;
        }
    }
}
