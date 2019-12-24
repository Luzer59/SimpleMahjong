using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartLevel()
    {
        SceneManager.LoadScene("MahjongLevel");
    }

    public void StartLevelBuilder()
    {
        SceneManager.LoadScene("MahjongLevelBuilder");
    }
}
