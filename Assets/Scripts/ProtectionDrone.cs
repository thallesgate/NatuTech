using UnityEngine;

public class ProtectionDrone : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    private ThrazEngine thrazEngine;

    // Par�metros do voo do drone
    public float raioOrbita = 0.3f;    // Dist�ncia do drone ao Thraz
    public float alturaVoo = 1f;       // Altura do voo do drone
    public float VelocidadeOrbita = 50f; // Velocidade de rota��o em graus por segundo
    private float angle;               // �ngulo atual do drone em rela��o ao Thraz

    public void Initialize(ThrazEngine thraz, float initialAngle)
    {
        thrazEngine = thraz;
        currentHealth = maxHealth;
        angle = initialAngle; // Define o �ngulo inicial do drone

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

        // Calcula a posi��o do drone
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * raioOrbita * GlobalPlacementData.scale.y;
        Vector3 targetPosition = thrazEngine.transform.position + offset + Vector3.up * alturaVoo * GlobalPlacementData.scale.y;
        transform.position = targetPosition;

        // Calcula a dire��o do movimento (derivada da posi��o em rela��o ao �ngulo)
        Vector3 direction = new Vector3(-Mathf.Sin(rad), 0f, Mathf.Cos(rad));

        // Define a rota��o do drone para olhar na dire��o do movimento
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
        Debug.Log("Drone foi destru�do.");

        if (thrazEngine != null)
        {
            thrazEngine.RemoveDrone(this);
        }

        Destroy(gameObject);
    }
}
