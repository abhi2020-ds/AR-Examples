using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneMenu : MonoBehaviour
{
    public void car()
    {
        SceneManager.LoadScene("CarTest");
    }

    public void exitgame()
    {
        Application.Quit();
        Debug.Log("Exit Button Pressed");
    }
}
