using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.ARSubsystems;

public class PlacementLaunchManager : MonoBehaviour
{
    public GameObject placementIndicator;
    public GameObject spherePrefab;
    public GameObject mapPrefab;
    public GameObject trajectoryPointPrefab;

    [SerializeField] private int rotationOffset = 180;
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    private Pose placementPose;
    private bool isPlacementPoseValid = false;
    [SerializeField] TrackableType trackableType = TrackableType.Planes;

    [SerializeField] private float placementCooldown = 0.5f;
    private float lastPlacementTime = -2f;

    [SerializeField] private float trajectoryPointsSpacing = 0.1f;
    private Camera cameraObject;
    [SerializeField] private GameObject destroyParticle;

    [Header("Input Actions")]
    public InputActionReference dragCurrentPosition;
    public InputActionReference dragDelta;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isDragging = false;

    void OnEnable()
    {
        dragCurrentPosition.action.performed += OnDragCurrentPosition;
        dragDelta.action.performed += OnDragDelta;
    }

    void OnDisable()
    {
        dragCurrentPosition.action.performed -= OnDragCurrentPosition;
        dragDelta.action.performed -= OnDragDelta;
    }

    void Start()
    {
        cameraObject = sessionOrigin.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        UpdatePlacementPose();
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = cameraObject.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        bool isHit = raycastManager.Raycast(screenCenter, hits, trackableType);

        if (isHit)
        {
            placementPose = hits[0].pose;
            isPlacementPoseValid = true;
        }
        else
        {
            isPlacementPoseValid = false;
        }

        if (isPlacementPoseValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void OnDragCurrentPosition(InputAction.CallbackContext context)
    {
        if (!isDragging)
        {
            isDragging = true;
            startTouchPosition = context.ReadValue<Vector2>();
        }
    }

    private void OnDragDelta(InputAction.CallbackContext context)
    {
        if (isDragging)
        {
            Vector2 swipeDelta = context.ReadValue<Vector2>();
            endTouchPosition = startTouchPosition + swipeDelta;

            if (swipeDelta.magnitude > 50) // Minimum swipe threshold
            {
                LaunchSphere(swipeDelta);
            }
            isDragging = false;
        }
    }

    private void LaunchSphere(Vector2 swipeDelta)
    {
        GameObject sphere = Instantiate(spherePrefab, cameraObject.transform.position, Quaternion.identity);
        Rigidbody rb = sphere.AddComponent<Rigidbody>();

        // Calculate direction and speed
        Vector3 direction = cameraObject.transform.forward + new Vector3(swipeDelta.x, 0, swipeDelta.y).normalized;
        float speed = swipeDelta.magnitude * 0.1f; // Scale speed by swipe magnitude

        rb.useGravity = true;
        rb.linearVelocity = direction * speed;

        // Add rotation for curve effect
        rb.AddTorque(Vector3.Cross(Vector3.up, direction) * speed);

        // Destroy the sphere after some time
        Destroy(sphere, 5f);

        // Optional: Spawn destroy particles
        if (destroyParticle != null)
        {
            Instantiate(destroyParticle, sphere.transform.position, Quaternion.identity);
        }
    }
}
