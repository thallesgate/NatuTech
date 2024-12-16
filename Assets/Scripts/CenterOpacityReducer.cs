using UnityEngine;
using System.Collections.Generic;

public class CenterTreeOpacityReducer : MonoBehaviour
{
    public string targetTag = "Tree"; // The tag to target. If null or empty, the effect applies to any object.
    public float reducedOpacity = 0.5f; // The opacity to apply to objects at the center of the screen.
    private Camera mainCamera;
    private Dictionary<GameObject, Color[]> originalColors = new Dictionary<GameObject, Color[]>();

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found. Please assign a camera tagged as MainCamera.");
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        // Get the center of the screen in screen space.
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        // Keep track of objects processed this frame.
        HashSet<GameObject> processedObjects = new HashSet<GameObject>();

        // Perform a raycast to check for objects in the center of the screen.
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Check if the object matches the tag (or no tag is specified).
            if (string.IsNullOrEmpty(targetTag) || hitObject.CompareTag(targetTag))
            {
                processedObjects.Add(hitObject);

                // Try to get the Renderer component of the object.
                Renderer renderer = hitObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (!originalColors.ContainsKey(hitObject))
                    {
                        // Save the original colors.
                        Color[] colors = new Color[renderer.materials.Length];
                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            colors[i] = renderer.materials[i].color;
                        }
                        originalColors[hitObject] = colors;
                    }

                    // Reduce the opacity of the material by modifying its alpha value.
                    foreach (Material material in renderer.materials)
                    {
                        Color color = material.color;
                        color.a = reducedOpacity;
                        material.color = color;
                    }
                }
            }
        }

        // Restore objects not in the center anymore.
        List<GameObject> objectsToRestore = new List<GameObject>(originalColors.Keys);
        foreach (GameObject obj in objectsToRestore)
        {
            if (!processedObjects.Contains(obj))
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && originalColors.ContainsKey(obj))
                {
                    // Restore the original colors.
                    Color[] colors = originalColors[obj];
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        renderer.materials[i].color = colors[i];
                    }
                }
                originalColors.Remove(obj);
            }
        }
    }
}
