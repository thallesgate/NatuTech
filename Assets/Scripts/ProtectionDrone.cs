using UnityEngine;

public class ProtectionDrone : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    private ThrazEngine thrazEngine;

    // Parâmetros do voo do drone
    public float raioOrbita = 0.3f;    // Distância do drone ao Thraz
    public float alturaVoo = 1f;       // Altura do voo do drone
    public float VelocidadeOrbita = 50f; // Velocidade de rotação em graus por segundo
    private float angle;               // Ângulo atual do drone em relação ao Thraz

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
            angle %= 360f;

            UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        float rad = Mathf.Deg2Rad * angle;

        // Calcula a posição do drone
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * raioOrbita * GlobalPlacementData.scale.y;
        Vector3 targetPosition = thrazEngine.transform.position + offset + Vector3.up * alturaVoo * GlobalPlacementData.scale.y;
        transform.position = targetPosition;

        // Calcula a direção do movimento (derivada da posição em relação ao ângulo)
        Vector3 direction = new Vector3(-Mathf.Sin(rad), 0f, Mathf.Cos(rad));

        // Define a rotação do drone para olhar na direção do movimento
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
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
