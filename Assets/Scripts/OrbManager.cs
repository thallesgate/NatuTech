using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using System;

public enum OrbType
{
    Fire,
    Water,
    Earth,
    Air
}

[System.Serializable]
public class OrbSettings
{
    public OrbType orbType;
    public bool enabled = true;
    public GameObject orbPrefab;
    public int damage = 10;
    public StatusEffect statusEffect = StatusEffect.None;
    public int effectDuration = 0;
}

public class OrbManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public GridManager gridManager;

    [Header("Orb Settings")]
    public List<OrbSettings> orbSettingsList;

    [Header("AR Settings")]
    public GameObject placementIndicator;
    public GameObject trajectoryPointPrefab;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    [Header("Launch Settings")]
    public float arcHeight = 0.5f;
    public float launchDuration = 0.5f;
    public Vector3 launchOffset = new Vector3(0f, -0.05f, 0f);
    public Vector3 placeOffset = new Vector3(0f, 0.05f, 0f);
    public float trajectoryPointsSpacing = 0.1f;

    [Header("Map Settings")]
    public GameObject mapPrefab;
    private bool isMapSpawned = true; // Set to true since map is spawned elsewhere

    [Header("Turn Manager")]
    public TurnManager turnManager;

    [Header("Input")]
    [SerializeField] private InputActionReference tap;

    private Camera cameraObject;
    private OrbType currentOrbType;
    private GameObject currentOrbPrefab;
    private Pose placementPose;
    private bool isPlacementPoseValid = false;

    private bool isOrbLaunched = false;

    // Sons
    private AudioController audioController;

    [SerializeField] private string launchFogo = "LaunchOrbFogo";
    [SerializeField] private string launchTerra = "LaunchOrbTerra";
    [SerializeField] private string launchAr = "LaunchOrbAr";
    [SerializeField] private string launchAgua = "LaunchOrbAgua";




    void Start()
    {
        audioController = FindFirstObjectByType<AudioController>();

        tap.action.started += OnTap;
        tap.action.Enable();

        cameraObject = Camera.main;

        SelectFirstEnabledOrb();

        if (planeManager == null)
        {
            planeManager = FindFirstObjectByType<ARPlaneManager>();
        }

        if (raycastManager == null)
        {
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
        }

        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManager>();
        }
    }

    private void SelectFirstEnabledOrb()
    {
        foreach (OrbSettings settings in orbSettingsList)
        {
            if (settings.enabled)
            {
                SetCurrentOrbType(settings.orbType);
                return;
            }
        }
        Debug.LogError("No orb type is enabled.");
    }

    public void SetCurrentOrbType(OrbType orbType)
    {
        OrbSettings settings = orbSettingsList.Find(o => o.orbType == orbType && o.enabled);
        if (settings != null)
        {
            currentOrbType = orbType;
            currentOrbPrefab = settings.orbPrefab;
            Debug.Log("Current orb set to: " + currentOrbType);
        }
        else
        {
            currentOrbPrefab = null;
            Debug.LogWarning("Orb " + orbType + " is not enabled or does not exist.");
        }
    }

    void OnDestroy()
    {
        tap.action.started -= OnTap;
    }

    void Update()
    {
        UpdatePlacementPose();
        DrawPlacementIndicatorAndTrajectory();

        // Do not disable planes here; keep them active
        // if (isMapSpawned && planeManager != null)
        // {
        //     foreach (var plane in planeManager.trackables)
        //     {
        //         plane.gameObject.SetActive(false);
        //     }
        // }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = cameraObject.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

        // Always use ARRaycastManager to detect planes
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        raycastManager.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        if (hits.Count > 0)
        {
            placementPose = hits[0].pose;
            isPlacementPoseValid = true;
        }
        else
        {
            // Optionally use Physics.Raycast if ARRaycastManager doesn't find a plane
            Ray ray = cameraObject.ScreenPointToRay(screenCenter);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                placementPose = new Pose(hitInfo.point, Quaternion.identity);
                isPlacementPoseValid = true;
            }
            else
            {
                isPlacementPoseValid = false;
            }
        }
    }

    private void DrawPlacementIndicatorAndTrajectory()
    {
        if (isPlacementPoseValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);

            if (turnManager != null && turnManager.IsPlayerTurn() && !isOrbLaunched)
            {
                DrawTrajectory(cameraObject.transform.position + launchOffset, placementPose.position, "TrajectoryPoint", Color.white);
            }
            else
            {
                ClearTrajectoryPoints("TrajectoryPoint");
            }
        }
        else
        {
            placementIndicator.SetActive(false);
            ClearTrajectoryPoints("TrajectoryPoint");
        }
    }

    private void ClearTrajectoryPoints(string tag)
    {
        foreach (var point in GameObject.FindGameObjectsWithTag(tag))
        {
            Destroy(point);
        }
    }

    private void OnTap(InputAction.CallbackContext context)
    {
        // Check if pointer is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isPlacementPoseValid)
        {
            if (turnManager != null && turnManager.CanPlayerAct())
            {
                LaunchOrb();
                turnManager.EndPlayerTurn();
            }
            else
            {
                Debug.Log("You cannot act at the moment.");
            }
        }
    }

    private void LaunchOrb()
    {
        if (currentOrbPrefab == null)
        {
            Debug.LogWarning("No orb is selected or the prefab is not assigned.");
            return;
        }

        GameObject orb = Instantiate(currentOrbPrefab, cameraObject.transform.position + launchOffset, Quaternion.identity);

        if (orb != null)
        {
            Orb orbScript = orb.GetComponent<Orb>();
            if (orbScript != null)
            {
                OrbSettings currentSettings = orbSettingsList.Find(o => o.orbType == currentOrbType);

                orbScript.Initialize(
                    currentOrbType,
                    currentSettings.damage,
                    cameraObject.transform.position + launchOffset,
                    placementPose.position,
                    arcHeight,
                    launchDuration,
                    currentSettings.statusEffect,
                    currentSettings.effectDuration,
                    OnOrbFinished
                );

                PlaySoundLancamento(currentOrbType);
            }
            else
            {
                Debug.LogError("The orb prefab does not have the Orb.cs component.");
            }
        }
    }

    private void PlaySoundLancamento(OrbType orbType)
    {

        switch (orbType)
        {
            case OrbType.Fire:
                audioController.PlaySound(launchFogo);
                break;
            case OrbType.Water:
                audioController.PlaySound(launchAgua);
                break;
            case OrbType.Earth:
                audioController.PlaySound(launchTerra);
                break;
            case OrbType.Air:
                audioController.PlaySound(launchAr);
                break;
            default:
                Debug.Log("OrbType não identificado!");
                break;
        }

    }


    private void OnOrbFinished()
    {
        // Optionally reset orb launched state
        // isOrbLaunched = false;
    }

    private void DrawTrajectory(Vector3 start, Vector3 end, string tag, Color color)
    {
        // Maximum allowed quads to limit
        const int maxTrajectoryPoints = 20;

        // Calculate the total curve length
        var trajectoryLength = CalculateCurveLength(start, (start + end) / 2 + Vector3.up * arcHeight, end);

        // Determine the number of points based on spacing or max limit
        int trajectoryPoints = Mathf.Min(maxTrajectoryPoints, (int)(trajectoryLength / trajectoryPointsSpacing));
        if (trajectoryPoints <= 1)
            return;

        // Adjust spacing to evenly distribute the quads
        float adjustedSpacing = trajectoryLength / trajectoryPoints;

        ClearTrajectoryPoints(tag);

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t1 = i / (float)(trajectoryPoints - 1);
            float t2 = (i + 1) / (float)(trajectoryPoints - 1);

            Vector3 point1 = CalculateBezierPoint(t1, start, (start + end) / 2 + Vector3.up * arcHeight, end);
            Vector3 point2 = CalculateBezierPoint(t2, start, (start + end) / 2 + Vector3.up * arcHeight, end);

            GameObject quad = Instantiate(trajectoryPointPrefab, point1, Quaternion.identity);
            Renderer renderer = quad.GetComponentInChildren<Renderer>();

            if (renderer != null && renderer.material != null)
            {
                renderer.material.SetColor("_BaseColor", color);
            }
            quad.tag = tag;

            Vector3 direction = (point2 - point1).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);
            quad.transform.rotation = rotation;
        }
    }


    private float CalculateCurveLength(Vector3 start, Vector3 control, Vector3 end)
    {
        float length = 0f;
        const int segments = 100;

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

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }
}
