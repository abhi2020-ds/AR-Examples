using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackandClose : MonoBehaviour
{

public void backbtn()
    {
        SceneManager.LoadScene("Main");
    }

public void exitbtn()
    {
        Application.Quit();
        Debug.Log("Exit Button Is Pressed");
    }

}
