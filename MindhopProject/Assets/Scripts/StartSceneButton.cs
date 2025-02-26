using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneButton : MonoBehaviour
{
   public void PlayGame(){
    SceneManager.LoadSceneAsync("Game");
   }
}
