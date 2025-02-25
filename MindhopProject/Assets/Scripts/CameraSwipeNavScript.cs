using UnityEngine;

public class CameraSwipeRotationAndPinchZoom : MonoBehaviour
{
    public float rotationSpeed = 10f; // Speed of rotation
    public float zoomSpeed = 0.1f; // Speed of zooming
    public float minZoomDistance = 2f; // Minimum distance for zoom
    public float maxZoomDistance = 25f; // Maximum distance for zoom

    public Transform target; // The GameObject to rotate (e.g., the camera or its parent)
    public Transform cameraTransform; // The camera's transform for zooming

    private Vector2 touchStartPos; // Position where the touch started
    private bool isSwiping = false; // Flag to check if swiping is in progress
    private float initialDistance; // Initial distance between two touches for pinch zoom
    private bool isPinching = false; // Flag to check if pinching is in progress

    void Update()
    {
        HandleSwipeRotation();
        HandlePinchZoom();
    }

    void HandleSwipeRotation()
    {
        // Check if there is at least one touch and not pinching
        if (Input.touchCount == 1 && !isPinching)
        {
            Touch touch = Input.GetTouch(0); // Get the first touch

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Record the start position of the touch
                    touchStartPos = touch.position;
                    isSwiping = true;
                    break;

                case TouchPhase.Moved:
                    if (isSwiping)
                    {
                        // Calculate the difference in touch position
                        Vector2 touchDelta = touch.position - touchStartPos;

                        // Rotate the target based on the swipe direction
                        float rotationAmount = -touchDelta.x * rotationSpeed * Time.deltaTime;
                        target.Rotate(0, rotationAmount, 0);

                        // Update the start position for smooth continuous rotation
                        touchStartPos = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // Stop swiping when the touch ends or is canceled
                    isSwiping = false;
                    break;
            }
        }
    }

    void HandlePinchZoom()
    {
        // Check if there are exactly two touches for pinch zoom
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Calculate the distance between the two touches
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

            if (touch2.phase == TouchPhase.Began)
            {
                // Record the initial distance when the second touch begins
                initialDistance = currentDistance;
                isPinching = true;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                // Calculate the difference in distance
                float distanceDifference = currentDistance - initialDistance;

                // Move the camera along its local Z-axis based on the pinch direction
                float newZ = cameraTransform.localPosition.z - distanceDifference * zoomSpeed * Time.deltaTime;

                // Clamp the zoom distance to min and max values
                newZ = Mathf.Clamp(newZ, -maxZoomDistance, -minZoomDistance);

                // Apply the new position
                cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, cameraTransform.localPosition.y, newZ);

                // Update the initial distance for smooth continuous zoom
                initialDistance = currentDistance;
            }
        }
        else
        {
            // Reset the pinching flag if there are not exactly two touches
            isPinching = false;
        }
    }
}