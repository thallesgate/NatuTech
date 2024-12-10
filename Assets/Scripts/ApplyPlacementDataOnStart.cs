using UnityEngine;

public class ApplyPlacementDataOnStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GlobalPlacementData.position != null)
        {
            this.transform.position = GlobalPlacementData.position;
            this.transform.rotation = GlobalPlacementData.rotation;
            this.transform.localScale = GlobalPlacementData.scale;
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
