using UnityEngine;

public class TreeEngine : MonoBehaviour
{
    public int treeHealth = 100;

    // Método para aplicar dano à árvore
    public void TakeDamage(int damageAmount)
    {
        treeHealth -= damageAmount;
        Debug.Log("A árvore tomou dano. Vida restante: " + treeHealth);

        // Verifica se a vida da árvore chegou a zero ou menos
        if (treeHealth <= 0)
        {
            DestroyTree();
        }
    }

    // Método para destruir a árvore
    void DestroyTree()
    {
        Debug.Log("A árvore foi destruída!");

        // Notifica o TurnManager que esta árvore foi destruída
        TurnManager turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.RemoveTree(gameObject); // Remove a árvore da lista de árvores
        }

        Destroy(gameObject); // Remove o objeto da cena
    }
}
