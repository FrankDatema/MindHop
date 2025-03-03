using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchToChangeScene : MonoBehaviour
{
    public string sceneToLoad; // Name of the scene to switch to

    void Update()
    {
        // Check if there is any touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // Get the first touch

            // Check if the touch phase is just began (i.e., the screen was touched)
            if (touch.phase == TouchPhase.Began)
            {
                // Load the new scene
                SceneManager.LoadScene(sceneToLoad);

            }
        }

        // Optional: Also check for mouse click for testing in the editor
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(sceneToLoad);

        }


    }
}