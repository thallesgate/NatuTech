using UnityEngine;

public class AttachParticleSystemToCamera : MonoBehaviour
{
    public ParticleSystem particlePrefab; // Assign your Particle System prefab in the Inspector
    [SerializeField] private bool deleteOnDestroy = true;
    private ParticleSystem spawnedParticleSystem;
    private GameObject arCamera;
    void Start()
    {
        if (particlePrefab == null)
        {
            Debug.LogError("Homescreen Particle Spawner: Particle Prefab is not assigned in the Inspector!");
            return;
        }

        // Spawn the particle system as a child of the camera
        arCamera = FindFirstObjectByType<Camera>().gameObject;
        spawnedParticleSystem = Instantiate(particlePrefab, arCamera.transform);

        // Optional: Adjust the position of the particle system relative to the camera
        spawnedParticleSystem.transform.localPosition = Vector3.zero;
        spawnedParticleSystem.transform.localRotation = Quaternion.identity;
    }

    void OnDestroy()
    {
        // Destroy the spawned particle system
        if (spawnedParticleSystem != null && deleteOnDestroy)
        {
            Destroy(spawnedParticleSystem.gameObject);
        }
    }
}

