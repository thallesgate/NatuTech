using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class PlacementSceneController : MonoBehaviour
{
    // Variables from PlacementInterfaceController
    [SerializeField] private Slider distanceSlider;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float idealDistance = 0.8f;
    [SerializeField] private float sliderMin = 0.4f;
    [SerializeField] private float sliderMax = 1.3f;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color idealColor = Color.green;
    [SerializeField] private Material midMaterial;
    [SerializeField] private Material idealMaterial;
    [SerializeField] private Image handleImageComponent;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private GameObject placeTooltipText;
    [SerializeField] private GameObject topTooltip;
    [SerializeField] private GameObject bottomTooltip;

    public string noPlaneText = "Aponte a c�mera para uma superf�cie plana e texturizada.";
    public string lowText = "Perto demais! Afaste-se.";
    public string midText = "Afaste-se um pouco mais...";
    public string idealText = "Perfeito!";
    public string mappingText = "Aponte a c�mera para uma superf�cie plana e texturizada.";

    // Variables from PlacementManager
    public GameObject indicatorPrefab;
    public GameObject indicatorBlockedPrefab;

    [HideInInspector] public GameObject indicatorInstance;
    [HideInInspector] public GameObject indicatorBlockedInstance;

    private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private Camera arCamera;
    [SerializeField] private float delayTime = 2.0f;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private float currentDistance;
    private bool isPlacementPoseValid = false;
    private bool isDistanceTooLow = false;

    public float screenPercentage = 100f;
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

    private bool mappingCompleted = false;

    private bool isAnimatorExited = false;
    [SerializeField] private Animator animator;
    [SerializeField] private string animatorTrigger = "MapPlacementEaseOut";
    [SerializeField] private GameObject flashbangPrefab;

    [Header("Audio")]
    private AudioController audioController;
    [SerializeField] private string tapSound = "Start";
    void Start()
    {
        audioController = FindFirstObjectByType<AudioController>();
        arPlaneManager = FindFirstObjectByType<ARPlaneManager>();

        if (arPlaneManager != null)
        {
            Debug.Log("Placement Scene Controller: ARPlaneManager found and Set to Active!");
            arPlaneManager.enabled = true;
        }
        else
        {
            Debug.Log("Placement Scene Controller: ARPlaneManager not found!");
        }

        // Subscribe to plane events
        SubscribeToPlanesChanged();

        // Initially, show mapping progress UI and hide distance measurement UI
        topTooltip.gameObject.SetActive(false);
        bottomTooltip.gameObject.SetActive(false);
        placeTooltipText.gameObject.SetActive(false);

        foreach (var plane in arPlaneManager.trackables)
        {
            if (!plane.gameObject.activeInHierarchy)
            {
                plane.gameObject.SetActive(true);
            }
        }

        arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
        arCamera = Camera.main;
        handleImageComponent = distanceSlider.handleRect.GetComponent<Image>();

        indicatorInstance = Instantiate(indicatorPrefab);
        indicatorInstance.SetActive(false);

        indicatorBlockedInstance = Instantiate(indicatorBlockedPrefab);
        indicatorBlockedInstance.SetActive(false);

        tap = InputSystem.actions.FindAction("Spawn Object");

        StartCoroutine(EnableInputAfterDelay());


    }

    void Update()
    {
        if (!mappingCompleted)
        {
            // Update mapping progress UI
            UpdateMappingProgressUI();
            indicatorInstance.SetActive(false);
            indicatorBlockedInstance.SetActive(false);
        }
        else
        {
            if (!topTooltip.activeInHierarchy) { topTooltip.gameObject.SetActive(true); };
            if (!topTooltip.activeInHierarchy) { bottomTooltip.gameObject.SetActive(true); };

            // Mapping is complete, update distance measurement UI
            // Update UI elements based on currentDistance
            bool isAbleToPlace = false;
            if (!isPlacementPoseValid)
            {
                handleImageComponent.color = lowColor;
                distanceSlider.value = 0f;
                tooltipText.text = noPlaneText;
            }
            else if (currentDistance < minDistance)
            {
                handleImageComponent.color = lowColor;
                distanceSlider.value = Map(currentDistance, sliderMin, sliderMax, 0f, 1f);
                tooltipText.text = lowText;
            }
            else if (currentDistance < idealDistance)
            {
                handleImageComponent.color = midColor;
                distanceSlider.value = Map(currentDistance, sliderMin, sliderMax, 0f, 1f);
                tooltipText.text = midText;
                isAbleToPlace = true;
            }
            else
            {
                handleImageComponent.color = idealColor;
                distanceSlider.value = Map(currentDistance, sliderMin, sliderMax, 0f, 1f);
                tooltipText.text = idealText;
                isAbleToPlace = true;
            }
            placeTooltipText.SetActive(isAbleToPlace);

            UpdatePlacementPose();
            UpdateIndicator();
        }
    }

    void OnDestroy()
    {
        tap.performed -= OnClick;
        UnsubscribeToPlanesChanged();
    }

    void SubscribeToPlanesChanged()
    {
        arPlaneManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    void UnsubscribeToPlanesChanged()
    {
        arPlaneManager.trackablesChanged.RemoveListener(OnPlanesChanged);
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

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon) && (indicatorInstance != null) && (indicatorBlockedInstance != null))
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

            Renderer indicatorRenderer;
            if (indicatorInstance.GetComponent<Renderer>() == null)
            {
                indicatorRenderer = indicatorInstance.GetComponentInChildren<Renderer>();
            }
            else
            {
                indicatorRenderer = indicatorInstance.GetComponent<Renderer>();
            }

            if (currentDistance < 0.5f)
            {
                isDistanceTooLow = true;
            }
            else if (currentDistance >= 0.5f && currentDistance < 0.8f)
            {
                indicatorRenderer.material = midMaterial;
            }
            else
            {
                indicatorRenderer.material = idealMaterial;

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
                mappingProgressText.text = "Mova seu dispositivo para mapear a superficie.";
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
                if (indicatorBlockedInstance != null) { indicatorBlockedInstance.SetActive(false); };
                if (indicatorInstance != null) { indicatorInstance.SetActive(false); };
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
                audioController.PlaySound(tapSound);
                InstantiateFlashbang();
                tap.Disable();
                StorePlacementData();
                InstantiateScenePrefab();
                if (animator != null)
                {
                    animator.SetTrigger(animatorTrigger);
                }
                Destroy(indicatorInstance);
                Destroy(indicatorBlockedInstance);
                foreach (var plane in arPlaneManager.trackables)
                {
                    if (plane.gameObject.activeInHierarchy)
                    {
                        plane.gameObject.SetActive(false);
                    }
                }
                Debug.Log("Click!");
            }
        }
    }

    void StorePlacementData()
    {
        Quaternion rotationAdjustment = Quaternion.Euler(0, 180, 0);
        Debug.Log("Placement Manager: Stored Placement Data!");
        GlobalPlacementData.position = indicatorInstance.transform.position;
        GlobalPlacementData.rotation = indicatorInstance.transform.rotation * rotationAdjustment;
        GlobalPlacementData.scale = indicatorInstance.transform.localScale;
    }

    void InstantiateScenePrefab()
    {
        if (sceneToLoadPrefab != null)
        {
            GameObject spawnedScene = Instantiate(sceneToLoadPrefab, GlobalPlacementData.position, GlobalPlacementData.rotation);
            spawnedScene.transform.localScale = GlobalPlacementData.scale;
        }
    }
    void InstantiateFlashbang()
    {
        if (flashbangPrefab != null)
        {
            Instantiate(flashbangPrefab);
        }
    }

    public void PlacementSceneOnAnimatorExit()
    {
        if (!isAnimatorExited)
        {
            Debug.Log("Placement Scene Controller: Animator Exit Triggered!");
            Destroy(gameObject);
            isAnimatorExited = true;
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

        if (cumulativeMappedArea >= requiredMappedArea)
        {
            mappingCompleted = true;
            // Hide mapping progress UI
            if (mappingProgressBar != null)
            {
                mappingProgressBar.gameObject.SetActive(false);
            }
            if (mappingProgressText != null)
            {
                mappingProgressText.gameObject.SetActive(false);
            }
            // Show distance measurement UI
            distanceSlider.gameObject.SetActive(true);
            tooltipText.gameObject.SetActive(true);
            placeTooltipText.SetActive(false); // Initially hide placeTooltipText
        }

        UpdateMappingProgressUI();
    }

    private void UpdateMappingProgressUI()
    {
        if (mappingCompleted)
            return;

        float progress = Mathf.Clamp01(cumulativeMappedArea / requiredMappedArea);

        if (mappingProgressBar != null)
        {
            mappingProgressBar.value = progress;
        }

        if (mappingProgressText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100);
            mappingProgressText.text = mappingText + " Mapeando:" + percentage + "%";
        }
        else
        {
            int percentage = Mathf.RoundToInt(progress * 100);
            tooltipText.text = mappingText + percentage + "%";
        }

    }

    float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}