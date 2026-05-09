using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
     public void StartGame()
    {
        SceneManager.LoadScene("StoryScene");
    }

    // Update is called once per frame
    public void QuitGame()
    {
        Debug.Log("已经点击了退出游戏");
            Application.Quit();
    }
}
