using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;
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

    public GameObject sceneToLoadPrefab;

    // Variables for mapping progress
    private float cumulativeMappedArea = 0f;
    public float requiredMappedArea = 1.0f; // Adjust based on your scale
    public Slider mappingProgressBar;
    public TextMeshProUGUI mappingProgressText;

    void Start()
    {
        arPlaneManager = FindFirstObjectByType<ARPlaneManager>();
        arPlaneManager.enabled = true;
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(true);
        }
        arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
        arCamera = FindFirstObjectByType<Camera>();

        indicatorInstance = Instantiate(indicatorPrefab);
        indicatorInstance.SetActive(false);

        indicatorBlockedInstance = Instantiate(indicatorBlockedPrefab);
        indicatorBlockedInstance.SetActive(false);

        tap = InputSystem.actions.FindAction("Spawn Object");

        StartCoroutine(EnableInputAfterDelay());

        // Subscribe to plane events
        SubscribeToPlanesChanged();
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdateIndicator();
    }

    void OnDestroy()
    {
        tap.performed -= OnClick;
        UnsubscribeToPlanesChanged();
    }

    void SubscribeToPlanesChanged()
    {
        // This is inefficient. You should re-use a saved reference instead.
        var manager = arPlaneManager;

        manager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    void UnsubscribeToPlanesChanged()
    {
        // This is inefficient. You should re-use a saved reference instead.
        var manager = arPlaneManager;

        manager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    private IEnumerator EnableInputAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        tap.Enable();
        tap.performed += OnClick;
    }

    void UpdatePlacementPose()
    {
        if (cumulativeMappedArea < requiredMappedArea)
        {
            isPlacementPoseValid = false;
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            isPlacementPoseValid = true;
            isDistanceTooLow = false;

            Pose hitPose = hits[0].pose;

            Vector3 translateObject = objectTransformDistance;
            Vector3 indicatorPosition = hitPose.position + translateObject;
            indicatorInstance.transform.position = indicatorPosition;
            indicatorBlockedInstance.transform.position = indicatorPosition;

            Vector3 planeNormal = hitPose.rotation * Vector3.up;

            Vector3 directionToCamera = arCamera.transform.position - indicatorPosition;
            directionToCamera = Vector3.ProjectOnPlane(directionToCamera, planeNormal).normalized;

            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToCamera, planeNormal);
                indicatorInstance.transform.rotation = lookRotation * objectTransformRotation;
                indicatorBlockedInstance.transform.rotation = lookRotation * objectTransformRotation;
            }
            else
            {
                indicatorInstance.transform.rotation = hitPose.rotation * objectTransformRotation;
                indicatorBlockedInstance.transform.rotation = hitPose.rotation * objectTransformRotation;
            }

            currentDistance = Vector3.Distance(arCamera.transform.position, hitPose.position);

            float desiredScreenSizeRatio = screenPercentage / 100f;
            float screenDimensionInWorldUnitsAtDistance;

            if (IsLandscape())
            {
                float verticalFOV = arCamera.fieldOfView;
                float frustumHeight = 2 * currentDistance * Mathf.Tan(verticalFOV * 0.5f * Mathf.Deg2Rad);
                screenDimensionInWorldUnitsAtDistance = frustumHeight;
            }
            else
            {
                float horizontalFOV = CalculateHorizontalFOV(arCamera.fieldOfView, arCamera.aspect);
                float frustumWidth = 2 * currentDistance * Mathf.Tan(horizontalFOV * 0.5f * Mathf.Deg2Rad);
                screenDimensionInWorldUnitsAtDistance = frustumWidth;
            }

            float desiredIndicatorSize = screenDimensionInWorldUnitsAtDistance * desiredScreenSizeRatio * objectScale;
            indicatorInstance.transform.localScale = new Vector3(desiredIndicatorSize, desiredIndicatorSize, desiredIndicatorSize);
            indicatorBlockedInstance.transform.localScale = new Vector3(desiredIndicatorSize, desiredIndicatorSize, desiredIndicatorSize);

            Renderer indicatorRenderer = indicatorInstance.GetComponent<Renderer>();
            Renderer indicatorBlockedRenderer = indicatorBlockedInstance.GetComponent<Renderer>();

            if (currentDistance < 0.5f)
            {
                indicatorBlockedRenderer.material.color = new Color(1, 0, 0, 0.5f);
                isDistanceTooLow = true;
            }
            else if (currentDistance >= 0.5f && currentDistance < 0.8f)
            {
                indicatorRenderer.material.color = new Color(1f, 0.92f, 0.016f, 0.5f);
            }
            else
            {
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
        if (cumulativeMappedArea < requiredMappedArea)
        {
            indicatorBlockedInstance.SetActive(false);
            indicatorInstance.SetActive(false);

            if (mappingProgressText != null)
            {
                mappingProgressText.text = "Move your device to map the environment.";
            }
        }
        else
        {
            if (isPlacementPoseValid)
            {
                if (isDistanceTooLow)
                {
                    indicatorBlockedInstance.SetActive(true);
                    indicatorInstance.SetActive(false);
                }
                else
                {
                    indicatorBlockedInstance.SetActive(false);
                    indicatorInstance.SetActive(true);
                }
            }
            else
            {
                indicatorBlockedInstance.SetActive(false);
                indicatorInstance.SetActive(false);
            }
        }
    }

    public bool CanPlace()
    {
        bool mappingQualityMet = cumulativeMappedArea >= requiredMappedArea;
        return isPlacementPoseValid && currentDistance >= 0.5f && mappingQualityMet;
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
        GlobalPlacementData.position = indicatorInstance.transform.position;
        GlobalPlacementData.rotation = indicatorInstance.transform.rotation;
        GlobalPlacementData.scale = indicatorInstance.transform.localScale;
    }

    void LoadNextScene()
    {
        if (sceneToLoadPrefab != null)
        {
            GameObject spawnedScene = Instantiate(sceneToLoadPrefab, GlobalPlacementData.position, GlobalPlacementData.rotation);
            spawnedScene.transform.localScale = GlobalPlacementData.scale;
            Destroy(gameObject);
        }
    }

    public void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
    {
        //foreach (var plane in changes.added)
        //{
        //    // handle added planes
        //}
        //
        //foreach (var plane in changes.updated)
        //{
        //    // handle updated planes
        //}
        //
        //foreach (var plane in changes.removed)
        //{
        //    // handle removed planes
        //}
        UpdateCumulativeMappedArea();
    }

    private void UpdateCumulativeMappedArea()
    {
        cumulativeMappedArea = 0f;
        foreach (var plane in arPlaneManager.trackables)
        {
            Vector2 planeSize = plane.size;
            float area = planeSize.x * planeSize.y;
            cumulativeMappedArea += area;
        }

        UpdateMappingProgressUI();
    }

    private void UpdateMappingProgressUI()
    {
        float progress = Mathf.Clamp01(cumulativeMappedArea / requiredMappedArea);

        mappingProgressBar.value = progress;

        if (mappingProgressText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100);
            mappingProgressText.text = "Mapping Progress: " + percentage + "%";
        }
    }
}
