using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
public class MapPlacer : MonoBehaviour
{
    public GameObject mapPrefab;
    [HideInInspector] public XROrigin arSessionOrigin;
    [HideInInspector] public ARPlaneManager arPlaneManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arSessionOrigin = FindFirstObjectByType<XROrigin>();
        arPlaneManager = FindFirstObjectByType<ARPlaneManager>();
        // Make sure the plane prefab exists
        if (arPlaneManager != null)
        {
            
            foreach (var plane in arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
            arPlaneManager.enabled = false; //Disable PlaneManager to hide the planes.
        }
        else
        {
            Debug.LogWarning("Map Placer: ARPlaneManager not found.");
        }
        PlaceMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlaceMap()
    {
        Debug.Log("Map Placer: Placed!");
        if (PlacementManager.placementData != null)
        {
            GameObject mapInstance = Instantiate(mapPrefab);
            mapInstance.transform.position = PlacementManager.placementData.position;
            mapInstance.transform.rotation = PlacementManager.placementData.rotation;
            mapInstance.transform.localScale = PlacementManager.placementData.scale;
        }
        else
        {
            Debug.LogError("Map Placer: Placement Data is missing!");
        }
    }
}
