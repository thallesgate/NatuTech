using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlacementManager : MonoBehaviour
{
    public GameObject indicatorPrefab;
    public GameObject indicatorBlockedPrefab;
    [HideInInspector]
    public GameObject indicatorInstance;
    [HideInInspector]
    public GameObject indicatorBlockedInstance;

    private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private Camera arCamera;
    [SerializeField] private float delayTime = 2.0f;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private float currentDistance;
    private bool isPlacementPoseValid = false;
    private bool isDistanceTooLow = false;

    public float screenPercentage = 80f;
    public float objectScale = 1.0f;
    public Vector3 objectTransformDistance; // Set to 0 to place directly on the plane
    [SerializeField] private Quaternion objectTransformRotation = Quaternion.Euler(0, 0, 0);

    [SerializeField] private InputAction tap;

    public static PlacementData placementData;
    public string sceneToLoad = "TestGameScene";
    void Start()
    {
        arPlaneManager = FindFirstObjectByType<ARPlaneManager>();
        arPlaneManager.enabled = true; // Enable the Plane Manager to start tracking and visualize planes.
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(true);
        }
        arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
        arCamera = FindFirstObjectByType<Camera>();

        // Instantiate the indicators in the scene
        indicatorInstance = Instantiate(indicatorPrefab);
        indicatorInstance.SetActive(false); // Hide initially

        indicatorBlockedInstance = Instantiate(indicatorBlockedPrefab);
        indicatorBlockedInstance.SetActive(false); // Hide initially

        tap = InputSystem.actions.FindAction("Spawn Object");
        StartCoroutine(EnableInputAfterDelay());
    }
    
    void Update()
    {
        UpdatePlacementPose();
        UpdateIndicator();
    }
    void OnDestroy()
    {
        tap.performed -= OnClick;
    }

    private IEnumerator EnableInputAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        tap.Enable();
        tap.performed += OnClick;
    }
    void UpdatePlacementPose()
    {
        // Use the center of the screen as the touch point
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            isPlacementPoseValid = true;
            isDistanceTooLow = false;

            // Get the pose of the hit point
            Pose hitPose = hits[0].pose;

            // Update the position of the indicators
            Vector3 translateObject = objectTransformDistance;
            Vector3 indicatorPosition = hitPose.position + translateObject;
            indicatorInstance.transform.position = indicatorPosition;
            indicatorBlockedInstance.transform.position = indicatorPosition;

            // Get the normal of the plane from the hitPose rotation
            Vector3 planeNormal = hitPose.rotation * Vector3.up;

            // Calculate the direction to the camera in the horizontal plane
            Vector3 directionToCamera = arCamera.transform.position - indicatorPosition;
            directionToCamera = Vector3.ProjectOnPlane(directionToCamera, planeNormal).normalized;

            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                // Create a rotation that faces the camera around the plane's normal
                Quaternion lookRotation = Quaternion.LookRotation(directionToCamera, planeNormal);
                indicatorInstance.transform.rotation = lookRotation * objectTransformRotation;
                indicatorBlockedInstance.transform.rotation = lookRotation * objectTransformRotation;
            }
            else
            {
                // If the direction is too small, use the plane's rotation
                indicatorInstance.transform.rotation = hitPose.rotation * objectTransformRotation;
                indicatorBlockedInstance.transform.rotation = hitPose.rotation * objectTransformRotation;
            }

            // Calculate the distance from the camera to the hit point
            currentDistance = Vector3.Distance(arCamera.transform.position, hitPose.position);

            // Adjust the scale of the indicators to remain fixed relative to the screen size
            float desiredScreenSizeRatio = screenPercentage / 100f; // e.g., 80% of screen dimension
            float screenDimensionInWorldUnitsAtDistance;

            if (IsLandscape())
            {
                // Use screen height in landscape mode
                float verticalFOV = arCamera.fieldOfView;
                float frustumHeight = 2 * currentDistance * Mathf.Tan(verticalFOV * 0.5f * Mathf.Deg2Rad);
                screenDimensionInWorldUnitsAtDistance = frustumHeight;
            }
            else
            {
                // Use screen width in portrait mode
                float horizontalFOV = CalculateHorizontalFOV(arCamera.fieldOfView, arCamera.aspect);
                float frustumWidth = 2 * currentDistance * Mathf.Tan(horizontalFOV * 0.5f * Mathf.Deg2Rad);
                screenDimensionInWorldUnitsAtDistance = frustumWidth;
            }

            float desiredIndicatorSize = screenDimensionInWorldUnitsAtDistance * desiredScreenSizeRatio * objectScale;
            indicatorInstance.transform.localScale = new Vector3(desiredIndicatorSize, desiredIndicatorSize, desiredIndicatorSize);
            indicatorBlockedInstance.transform.localScale = new Vector3(desiredIndicatorSize, desiredIndicatorSize, desiredIndicatorSize);

            // Adjust color based on distance
            Renderer indicatorRenderer = indicatorInstance.GetComponent<Renderer>();
            Renderer indicatorBlockedRenderer = indicatorBlockedInstance.GetComponent<Renderer>();

            if (currentDistance < 0.5f)
            {
                // Too close
                indicatorBlockedRenderer.material.color = new Color(1, 0, 0, 0.5f);
                isDistanceTooLow = true;
            }
            else if (currentDistance >= 0.5f && currentDistance < 0.8f)
            {
                // Not ideal
                indicatorRenderer.material.color = new Color(1f, 0.92f, 0.016f, 0.5f);
            }
            else
            {
                // Ideal
                indicatorRenderer.material.color = new Color(0, 1, 0, 0.5f);
            }
        }
        else
        {
            isPlacementPoseValid = false;
        }
    }

    void UpdateIndicator()
    {
        if (isPlacementPoseValid)
        {
            if (isDistanceTooLow)
            {
                if (!indicatorBlockedInstance.activeInHierarchy)
                    indicatorBlockedInstance.SetActive(true);
                if (indicatorInstance.activeInHierarchy)
                    indicatorInstance.SetActive(false);
            }
            else
            {
                if (indicatorBlockedInstance.activeInHierarchy)
                    indicatorBlockedInstance.SetActive(false);

                if (!indicatorInstance.activeInHierarchy)
                    indicatorInstance.SetActive(true);
            }
        }
        else
        {
            if (indicatorBlockedInstance.activeInHierarchy)
                indicatorBlockedInstance.SetActive(false);
            if (indicatorInstance.activeInHierarchy)
                indicatorInstance.SetActive(false);
        }
    }

    public bool CanPlace()
    {
        // Determine if placement is allowed based on distance and plane detection
        return isPlacementPoseValid && currentDistance >= 0.5f;
    }

    public Pose GetPlacementPose()
    {
        return hits[0].pose;
    }

    public float GetCurrentDistance()
    {
        return currentDistance;
    }

    private bool IsLandscape()
    {
        return Screen.width > Screen.height;
    }

    private float CalculateHorizontalFOV(float verticalFOV, float aspectRatio)
    {
        // Convert vertical FOV to horizontal FOV
        float verticalFOVRad = verticalFOV * Mathf.Deg2Rad;
        float horizontalFOVRad = 2 * Mathf.Atan(Mathf.Tan(verticalFOVRad / 2) * aspectRatio);
        return horizontalFOVRad * Mathf.Rad2Deg;
    }
    private void OnClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (CanPlace())
            {
                tap.Disable();
                StorePlacementData();
                LoadNextScene();
            }
        }
    }

    void StorePlacementData()
    {
        Debug.Log("Placement Manager: Stored Placement Data!");
        placementData = new PlacementData
        {
            position = indicatorInstance.transform.position,
            rotation = indicatorInstance.transform.rotation,
            scale = indicatorInstance.transform.localScale
        };
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
