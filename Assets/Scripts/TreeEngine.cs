using UnityEngine;

public class TreeEngine : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " recebeu " + damageAmount + " de dano. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            DestroyTree();
        }
    }

    void DestroyTree()
    {
        Debug.Log(gameObject.name + " foi destruída!");

        // Notifica o TurnManager que esta árvore foi removida
        TurnManager turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.RemoveTree(gameObject);
        }

        Destroy(gameObject);
    }
}
