using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
public class PlacementManager : MonoBehaviour
{
    public GameObject placementIndicator;
    public GameObject objectToPlace;
    [SerializeField] private int rotationOffset = 180;

    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARRaycastManager raycastManager;

    private Pose placementPose;
    private bool isPlacementPoseValid = false;

    [SerializeField] TrackableType trackableType = TrackableType.Planes;
    [SerializeField] InputActionReference tap;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
        if (isPlacementPoseValid && (Pointer.current.press.value > 0))
        {
            PlaceObject();
        }
    }

    private void UpdatePlacementIndicator()
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
    {
        var screenCenter = sessionOrigin.GetComponentInChildren<Camera>().ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        raycastManager.Raycast(screenCenter, hits, trackableType);

        isPlacementPoseValid = hits.Count > 0;
        if (isPlacementPoseValid)
        {
            placementPose = hits[0].pose;

            
            var cameraForward = sessionOrigin.GetComponentInChildren<Camera>().transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void PlaceObject()
    {
        Quaternion rotationOffsetPose = Quaternion.Euler(0, rotationOffset, 0);
        Quaternion newRotation = placementPose.rotation * rotationOffsetPose;

        Instantiate(objectToPlace, placementPose.position, newRotation);
    }
}
