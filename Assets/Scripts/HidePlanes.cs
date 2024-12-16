using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class HidePlanes : MonoBehaviour
{
    ARPlaneManager planeManager;
    private void Start()
    {
        planeManager = Object.FindFirstObjectByType<ARPlaneManager>();
        planeManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }
    public void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
    {
        foreach (var plane in changes.added)
        {
            if (plane.gameObject.activeInHierarchy)
            {
                plane.gameObject.SetActive(false);
            }
        }

        foreach (var plane in changes.updated)
        {
            if (plane.gameObject.activeInHierarchy)
            {
                plane.gameObject.SetActive(false);
            }
        }
    }
}
