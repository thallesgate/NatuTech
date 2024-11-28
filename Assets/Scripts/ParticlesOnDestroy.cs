using UnityEngine;

public class ParticlesOnDestroy : MonoBehaviour
{
    public ParticleSystem particlePrefab; // Assign your ParticlePrefab in the inspector
    void OnDestroy()
    {
        // Check if the application is not quitting to avoid errors during shutdown
        if (Application.isPlaying)
        {
            // Instantiate the particle system at the object's position and rotation
            Instantiate(particlePrefab, transform.position, transform.rotation);
            Debug.Log("AI");
         
        }
    }
}
