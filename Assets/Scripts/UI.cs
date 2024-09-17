using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject sphereObject;  // Marked as private

    private Renderer sphereRenderer;
    private VisualElement root;

    Button buttonFire;
    Button buttonWater;
    Button buttonEarth;
    Button buttonAir;
    Button buttonMap;

    private void OnEnable()
    {
        if (sphereObject == null)
        {
            Debug.LogError("Sphere Object is not assigned!");
            return;
        }

        sphereRenderer = sphereObject.GetComponent<Renderer>();

        if (sphereRenderer == null)
        {
            Debug.LogError("Renderer component not found on the sphere object!");
            return;
        }

        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component not found!");
            return;
        }

        root = uiDocument.rootVisualElement;
        Button buttonFire = root.Q<Button>("ElementFireButton");
        Button buttonWater = root.Q<Button>("ElementWaterButton");
        Button buttonEarth = root.Q<Button>("ElementEarthButton");
        Button buttonAir = root.Q<Button>("ElementAirButton");
        Button buttonMap = root.Q<Button>("MapButton");

        if (buttonFire != null)
        {
            Debug.Log("ButtonFire.");
            buttonFire.clicked += () => sphereRenderer.material.color = Color.red;
        }
        else
        {
            Debug.LogWarning("ButtonFire not found in the UI.");
        }

        if (buttonWater != null)
        {
            buttonWater.clicked += () => sphereRenderer.material.color = Color.blue;
        }
        else
        {
            Debug.LogWarning("ButtonWater not found in the UI.");
        }

        if (buttonEarth != null)
        {
            buttonEarth.clicked += () => sphereRenderer.material.color = new Color(0.6f, 0.3f, 0.1f); // Example brown color
        }
        else
        {
            Debug.LogWarning("ButtonEarth not found in the UI.");
        }

        if (buttonAir != null)
        {
            buttonAir.clicked += () => sphereRenderer.material.color = Color.white;
        }
        else
        {
            Debug.LogWarning("ButtonAir not found in the UI.");
        }

        if (buttonMap != null)
        {
            // Handle buttonMap click if needed
        }
        else
        {
            Debug.LogWarning("ButtonMap not found in the UI.");
        }
    }

    private void OnDisable()
    {
        // Optionally, remove event listeners to avoid potential memory leaks
        if (root != null)
        {
            Button buttonFire = root.Q<Button>("ElementFireButton");
            if (buttonFire != null)
            {
                buttonFire.clicked -= () => sphereRenderer.material.color = Color.red;
            }

            Button buttonWater = root.Q<Button>("ElementWaterButton");
            if (buttonWater != null)
            {
                buttonWater.clicked -= () => sphereRenderer.material.color = Color.blue;
            }

            Button buttonEarth = root.Q<Button>("ElementEarthButton");
            if (buttonEarth != null)
            {
                buttonEarth.clicked -= () => sphereRenderer.material.color = new Color(0.6f, 0.3f, 0.1f); // Example brown color
            }

            Button buttonAir = root.Q<Button>("ElementAirButton");
            if (buttonAir != null)
            {
                buttonAir.clicked -= () => sphereRenderer.material.color = Color.white;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Optionally initialize or set up things here
    }

    // Update is called once per frame
    void Update()
    {
        // Update logic here if needed
    }
}
