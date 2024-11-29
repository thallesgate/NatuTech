using UnityEngine;

public class ParticlesOnDestroy : MonoBehaviour
{
    public ParticleSystem createParticlePrefab; // Assign your ParticlePrefab in the inspector

    public ParticleSystem destroyParticlePrefab; // Assign your ParticlePrefab in the inspector

    private void Start()
    {
        // Check if the application is not quitting to avoid errors during shutdown
        if (Application.isPlaying)
        {
            // Instantiate the particle system at the object's position and rotation
            Instantiate(createParticlePrefab, transform.position, transform.rotation);
            Debug.Log("Surgi!");
        }
    }

    void OnDestroy()
    {
        // Check if the application is not quitting to avoid errors during shutdown
        if (Application.isPlaying)
        {
            // Instantiate the particle system at the object's position and rotation
            Instantiate(destroyParticlePrefab, transform.position, transform.rotation);
            Debug.Log("AI");
         
        }
    }
}
