using UnityEngine;

public class TreeEngine : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private DamageIndicator damageIndicator;

    void Start()
    {
        currentHealth = maxHealth;
        damageIndicator = GetComponent<DamageIndicator>();

        if (damageIndicator == null)
        {
            damageIndicator = gameObject.AddComponent<DamageIndicator>();
        }
    }

    public void Initialize(int health)
    {
        maxHealth = health;
        currentHealth = health;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " recebeu " + damageAmount + " de dano. Vida restante: " + currentHealth);

        if (damageIndicator != null)
        {
            damageIndicator.FlashDamage();
        }

        if (currentHealth <= 0)
        {
            DestroyTree();
        }
    }

    void DestroyTree()
    {
        Debug.Log(gameObject.name + " foi destruída!");
        // Remova a chamada do TurnManager aqui. Não chame turnManager.RemoveTree(gameObject).
        // Apenas destrua o objeto.
        Destroy(gameObject);
    }
}
