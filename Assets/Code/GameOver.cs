using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Level 1");
        }
    }
    public void RestartButton()
    {
        SceneManager.LoadScene("Level 1");
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Level 1");
        }
    }

    public void ExitButton()
    {
        Application.Quit();
    }
}
