using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class PlacementLaunchManager : MonoBehaviour
{
    public GameObject placementIndicator;
    public GameObject spherePrefab; // Use a sphere prefab to launch
    public GameObject trajectoryPointPrefab; // Quad with ring texture

    [SerializeField] private int rotationOffset = 180;

    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARRaycastManager raycastManager;

    private Pose placementPose;
    private bool isPlacementPoseValid = false;

    [SerializeField] TrackableType trackableType = TrackableType.Planes;
    [SerializeField] InputActionReference tap;

    [SerializeField] private float placementCooldown = 2f; // Cooldown period in seconds
    private float lastPlacementTime = -2f; // Initialize to -2 so that the first placement is allowed immediately

    [SerializeField] private float trajectoryCooldown = 0.033f;
    private float lastUpdateTrajectoryTime = -2f;

    [SerializeField] private float arcHeight = 0.1f; // The maximum height of the arc
    [SerializeField] private float launchDuration = 2f;
    [SerializeField] private Vector3 launchOffset = new Vector3(2f, -2f, 0f);
    [SerializeField] private float trajectoryPointsSpacing = 0.1f; // Number of points to visualize

    Camera cameraObject;
    
    bool isMapSpawned = false;

    void Start()
    {
        tap.action.started += OnTap;
        cameraObject = sessionOrigin.GetComponentInChildren<Camera>();
    }

    void OnDestroy()
    {
        tap.action.started -= OnTap;
    }

    void Update()
    {
        UpdatePlacementPose();
        if (isPlacementPoseValid && (Time.time - lastUpdateTrajectoryTime >= trajectoryCooldown))
        {
            DrawPlacementIndicator();
            if(isMapSpawned)
            {
                DrawTrajectory(cameraObject.transform.position + launchOffset, placementPose.position);
            }
            lastUpdateTrajectoryTime = Time.time; // Update the last placement time
        }
    }

    private void DrawPlacementIndicator()
    {
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

    private void UpdatePlacementPose()
    {;
        var screenCenter = cameraObject.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        raycastManager.Raycast(screenCenter, hits, trackableType);

        isPlacementPoseValid = hits.Count > 0;
        if (isPlacementPoseValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = cameraObject.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);

            // Debugging: Draw the raycast line
            Debug.DrawRay(cameraObject.transform.position, cameraObject.transform.forward * 30f, Color.red);
            Debug.DrawLine(cameraObject.transform.position, placementPose.position, Color.blue);
        }
    }

    private void OnTap(InputAction.CallbackContext context)
    {
        if(isMapSpawned)
        {
            if (isPlacementPoseValid && (Time.time - lastPlacementTime >= placementCooldown))
            {
                LaunchSphere();
                lastPlacementTime = Time.time; // Update the last placement time
            }
        }
        else
        {
            if (isPlacementPoseValid)
            {
                PlaceMap();
                isMapSpawned = true;
            }
        }
    }

    private void PlaceMap()
    {

    }
    private void LaunchSphere()
    {
        // Instantiate the sphere
        GameObject sphere = Instantiate(spherePrefab, sessionOrigin.transform.position, Quaternion.identity);
        SphereMovement sphereMovement = sphere.AddComponent<SphereMovement>();

        // Set the parameters for the movement
        sphereMovement.startPosition = cameraObject.transform.position + launchOffset;
        sphereMovement.endPosition = placementPose.position;
        sphereMovement.arcHeight = arcHeight;
        sphereMovement.duration = launchDuration; // Duration for the arc
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // (1-t)^2 * p0
        p += 3 * uu * t * p1; // 3 * (1-t) * t * p1
        p += 3 * u * tt * p2; // 3 * (1-t) * t^2 * p2
        p += ttt * p2; // t^3 * p2

        return p;
    }

    private void DrawTrajectory(Vector3 start, Vector3 end)
    {
        var trajectoryLength = CalculateCurveLength(start, (start + end) / 2 + Vector3.up * arcHeight, end);

        int trajectoryPoints = (int)(trajectoryLength / trajectoryPointsSpacing);
        if (trajectoryPoints <= 1)
            return;

        // Clear previous trajectory points
        foreach (var point in GameObject.FindGameObjectsWithTag("TrajectoryPoint"))
        {
            Destroy(point);
        }

        // Draw the trajectory using quads
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t1 = i / (float)(trajectoryPoints - 1);
            float t2 = (i + 1) / (float)(trajectoryPoints - 1);

            Vector3 point1 = CalculateBezierPoint(t1, start, (start + end) / 2 + Vector3.up * arcHeight, end);
            Vector3 point2 = CalculateBezierPoint(t2, start, (start + end) / 2 + Vector3.up * arcHeight, end);

            // Instantiate and position the quad
            GameObject quad = Instantiate(trajectoryPointPrefab, point1, Quaternion.identity);
            quad.tag = "TrajectoryPoint"; // Assign a tag for easy deletion

            // Align the quad with the curve
            Vector3 direction = (point2 - point1).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);
            quad.transform.rotation = rotation;
        }
    }
    private float CalculateCurveLength(Vector3 start, Vector3 control, Vector3 end)
    {
        float length = 0f;
        const int segments = 100; // Number of segments to approximate the curve

        Vector3 previousPoint = start;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 currentPoint = CalculateBezierPoint(t, start, control, end);
            length += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return length;
    }
}

public class SphereMovement : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float arcHeight;
    public float duration;

    private float elapsedTime = 0f;

    void Update()
    {
        if (elapsedTime < duration)
        {
            // Calculate the normalized time (0 to 1)
            float t = elapsedTime / duration;

            // Calculate the Bézier curve position
            transform.position = CalculateBezierPoint(t, startPosition, (startPosition + endPosition) / 2 + Vector3.up * arcHeight, endPosition);

            elapsedTime += Time.deltaTime;
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // (1-t)^2 * p0
        p += 3 * uu * t * p1; // 3 * (1-t) * t * p1
        p += 3 * u * tt * p2; // 3 * (1-t) * t^2 * p2
        p += ttt * p2; // t^3 * p2

        return p;
    }
}
