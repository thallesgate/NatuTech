using UnityEngine;

public class OscillateUI : MonoBehaviour
{
    // Position Oscillation Settings
    [Header("Position Oscillation")]
    public bool oscillatePosition = true;
    public float positionFrequency = 1f; // Oscillations per second
    public float positionAmplitude = 10f; // Movement units (e.g., pixels in UI)

    // Scale Oscillation Settings
    [Header("Scale Oscillation")]
    public bool oscillateScale = true;
    public float scaleFrequency = 1f; // Oscillations per second
    public Vector3 scaleAmplitude = new Vector3(0.1f, 0.1f, 0f); // Scale change

    // Rotation Oscillation Settings
    [Header("Rotation Oscillation")]
    public bool oscillateRotation = true;
    public float rotationFrequency = 1f; // Oscillations per second
    public float rotationAmplitude = 10f; // Rotation degrees

    // Internal variables to store original states
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private void Start()
    {
        // Store the original transform values
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    private void Update()
    {
        float time = Time.time;

        // Oscillate Position
        if (oscillatePosition)
        {
            float posOffset = Mathf.Sin(time * positionFrequency * Mathf.PI * 2f) * positionAmplitude;
            transform.localPosition = originalPosition + new Vector3(0f, posOffset, 0f);
        }

        // Oscillate Scale
        if (oscillateScale)
        {
            float scaleOffset = Mathf.Sin(time * scaleFrequency * Mathf.PI * 2f);
            Vector3 scaleChange = Vector3.Scale(scaleAmplitude, new Vector3(scaleOffset, scaleOffset, scaleOffset));
            transform.localScale = originalScale + scaleChange;
        }

        // Oscillate Rotation
        if (oscillateRotation)
        {
            float rotOffset = Mathf.Sin(time * rotationFrequency * Mathf.PI * 2f) * rotationAmplitude;
            transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, rotOffset);
        }
    }
}
