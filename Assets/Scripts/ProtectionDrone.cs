using UnityEngine;

public class ProtectionDrone : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    private ThrazEngine thrazEngine;

    // VOO do DRONE
    public float raioOrbita = 0.3f; // Distância do drone ao Thraz
    public float alturaVoo = 1;
    public float VelocidadeOrbita = 50f;   // Velocidade de rotação em graus por segundo
    private float angle;              // Ângulo atual do drone em relação ao Thraz

    public void Initialize(ThrazEngine thraz, float initialAngle)
    {
        thrazEngine = thraz;
        currentHealth = maxHealth;
        angle = initialAngle; // Define o ângulo inicial do drone

        UpdatePosition();
    }

    void Update()
    {
        if (thrazEngine != null)
        {
            angle += VelocidadeOrbita * Time.deltaTime;
            angle %= 360;

            UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        float rad = Mathf.Deg2Rad * angle;
        Vector3 offset = new Vector3(Mathf.Cos(rad), alturaVoo, Mathf.Sin(rad)) * raioOrbita;
        transform.position = thrazEngine.transform.position + offset;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Drone recebeu " + damage + " de dano. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            DestroyDrone();
        }
    }

    private void DestroyDrone()
    {
        Debug.Log("Drone foi destruído.");

        if (thrazEngine != null)
        {
            thrazEngine.RemoveDrone(this);
        }

        Destroy(gameObject);
    }
}
