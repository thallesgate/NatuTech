using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AttachCameraToCanvasOnLoad : MonoBehaviour
{
    void Awake()
    {
        // Find the Canvas component on the GameObject this script is attached to
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Attach Camera To Canvas On Load: No Canvas component found on this GameObject.");
            return;
        }

        // Find the main camera in the scene
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Attach Camera To Canvas On Load: No Main Camera found in the scene. Make sure a Camera is tagged as 'MainCamera'.");
            return;
        }

        // Set the Event Camera property of the Canvas
        canvas.worldCamera = mainCamera;
        Debug.Log($"Attach Camera To Canvas On Load: Event Camera set to {mainCamera.name} on Canvas {gameObject.name}.");
    }
}
