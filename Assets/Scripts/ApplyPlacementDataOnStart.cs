using UnityEngine;

public class ApplyPlacementDataOnStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlacementManager.placementData != null)
        {
            this.transform.position = PlacementManager.placementData.position;
            this.transform.rotation = PlacementManager.placementData.rotation;
            this.transform.localScale = PlacementManager.placementData.scale;
        }
        else
        {
            Debug.LogError("ApplyPlacementDataOnStart: Placement Data is missing!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
