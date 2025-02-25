using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchShrinkAndDestroyManager : MonoBehaviour
{
    void Update()
    {
        // Loop through all active touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i); // Get the current touch

            // Check if the touch phase is began (finger just touched the screen)
            if (touch.phase == TouchPhase.Began)
            {
                // Create a ray from the camera to the touch position
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                // Debug: Draw the ray in the scene view
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1f);

                // Check if the ray hits any 3D collider
                if (Physics.Raycast(ray, out hit))
                {
                    // Debug: Log the name of the hit object
                    Debug.Log("Raycast hit: " + hit.collider.name);

                    // Check if the hit object has the tag "chore"
                    if (hit.collider.CompareTag("chore"))
                    {
                        // Debug: Log that the object is being shrunk
                        Debug.Log("Shrinking object: " + hit.collider.name);

                        // Get the ChoreTrackerScript component on the object
                        CloudTrackerScript cloudTrackerScript = hit.collider.GetComponent<CloudTrackerScript>();

                        // Check if the component exists
                        if (cloudTrackerScript != null)
                        {
                            // Call the StartShrinkAndDestroy function
                            cloudTrackerScript.StartShrinkAndDestroy();
                        }
                        else
                        {
                            Debug.LogWarning("ChoreTrackerScript component not found on object: " + hit.collider.name);
                        }
                    }
                }
            }
        }
    }
}