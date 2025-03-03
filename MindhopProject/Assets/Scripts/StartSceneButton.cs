using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneButton : MonoBehaviour
{
    //Switch scene when pressed Start
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Game");
    }
    //Quit game when pressed Quit
    public void QuitGame()
    {
        Application.Quit();
    }
}
