using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereManager : MonoBehaviour
{
    public GameObject particleEffectPrefab; // Assign the particle effect prefab in the Unity Inspector
    Collider cl;
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("I HIT SOMETHING!!!");
        // Check if the particleEffectPrefab is assigned
        if (particleEffectPrefab != null)
        {
            // Get the contact point of the collision
            ContactPoint contact = collision.contacts[0];
            
            // Instantiate the particle effect at the contact point
            Instantiate(particleEffectPrefab, contact.point, Quaternion.identity);
        }

        // Destroy the sphere
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        cl.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}