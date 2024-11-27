using UnityEngine;
using UnityEngine.UI;

public class ImageHover : MonoBehaviour
{
    private RectTransform rectTransform;
    private bool increasingScale = true;
    private bool increasingRotation = true;

    [SerializeField] private bool enableScale = true;
    [SerializeField] private bool enableRotation = false;
    [SerializeField] private bool enableHover = true;

    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float scaleRate = 1.5f;

    [Header("Rotation Settings")]
    [SerializeField] private float minRotation = -5f; // In degrees
    [SerializeField] private float maxRotation = 5f;  // In degrees
    [SerializeField] private float rotationRate = 1.5f;

    [Header("Hover Settings")]
    [SerializeField] private float hoverAmplitude = 10f;
    [SerializeField] private float hoverFrequency = 1f;

    private float initialYPosition;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform component is required.");
            enabled = false;
            return;
        }

        initialYPosition = rectTransform.localPosition.y;
    }

    private void Update()
    {
        if (enableScale)
        {
            OscillateScale();
        }

        if (enableRotation)
        {
            OscillateRotation();
        }

        if (enableHover)
        {
            OscillatePosition();
        }
    }

    private void OscillateScale()
    {
        Vector3 currentScale = rectTransform.localScale;
        float scaleStep = scaleRate * Time.deltaTime;

        if (increasingScale)
        {
            currentScale.x += scaleStep;
            currentScale.y += scaleStep;

            if (currentScale.x >= maxScale)
            {
                increasingScale = false;
            }
        }
        else
        {
            currentScale.x -= scaleStep;
            currentScale.y -= scaleStep;

            if (currentScale.x <= minScale)
            {
                increasingScale = true;
            }
        }

        rectTransform.localScale = new Vector3(currentScale.x, currentScale.y, 1f);
    }

    private void OscillateRotation()
    {
        Vector3 currentRotation = rectTransform.localEulerAngles;
        float rotationStep = rotationRate * Time.deltaTime;

        if (increasingRotation)
        {
            currentRotation.z += rotationStep;

            if (currentRotation.z >= maxRotation)
            {
                increasingRotation = false;
            }
        }
        else
        {
            currentRotation.z -= rotationStep;

            if (currentRotation.z <= minRotation)
            {
                increasingRotation = true;
            }
        }

        rectTransform.localEulerAngles = new Vector3(0f, 0f, currentRotation.z);
    }

    private void OscillatePosition()
    {
        Vector3 currentPosition = rectTransform.localPosition;
        currentPosition.y = initialYPosition + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        rectTransform.localPosition = currentPosition;
    }
}
