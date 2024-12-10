using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using TMPro;
public class PlacementInterfaceController : MonoBehaviour
{
    public Slider distanceSlider;
    private ARRaycastManager arRaycastManager;
    private Camera arCamera;
    public float minDistance = 0.5f;
    public float idealDistance = 0.8f;
    public float sliderMin = 0.4f;
    public float sliderMax = 1.3f;
    public Color lowColor = Color.red;
    public Color midColor = Color.yellow;
    public Color idealColor = Color.green;
    private Image handleImageComponent;
    public TextMeshProUGUI tooltipText;
    public GameObject placeTooltipText;

    public string noPlaneText = "Aponte a câmera para uma superfície plana.";
    public string lowText = "Perto demais!";
    public string midText = "Afaste um pouquinho mais.";
    public string idealText = "Perfeito!";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
        arCamera = FindFirstObjectByType<Camera>();
        handleImageComponent = distanceSlider.handleRect.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = GetDistanceToPlane();
        bool isAbleToPlace = false;
        if (distance < 0f)
        {
            handleImageComponent.color = lowColor;
            distanceSlider.value = 0f;
            tooltipText.text = noPlaneText;
        }
        else if (distance < minDistance)
        {
            handleImageComponent.color = lowColor;
            distanceSlider.value = Map(distance, sliderMin, sliderMax, 0f, 1f);
            tooltipText.text = lowText;
        }
        else if(distance < idealDistance)
        {
            handleImageComponent.color = midColor;
            distanceSlider.value = Map(distance, sliderMin, sliderMax, 0f, 1f);
            tooltipText.text = midText;
            isAbleToPlace = true;
        }
        else
        {
            handleImageComponent.color = idealColor;
            distanceSlider.value = Map(distance, sliderMin, sliderMax, 0f, 1f);
            tooltipText.text = idealText;
            isAbleToPlace = true;
        }
        if (isAbleToPlace)
        {
            placeTooltipText.SetActive(true);
        }
        else
        {
            placeTooltipText.SetActive(false);
        }
    }

    float GetDistanceToPlane()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            float distance = Vector3.Distance(arCamera.transform.position, hitPose.position);
            return distance;
        }
        else
        {
            return -1f;
        }
    }

    float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
