using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public int attackDamage = 10;
    protected float movementStep;
    protected Vector3[,] gridPositions;
    protected float cellSize;
    public int enemyHealth = 100;
    protected GameObject targetTree;

    // Inicializa o grid no inimigo (chamado pelo GridManager)
    public virtual void InitializeGrid(Vector3[,] grid, float cellSize)
    {
        gridPositions = grid;
        this.cellSize = cellSize;
        movementStep = cellSize;
    }

    // Método para iniciar o turno do inimigo
    public virtual void StartTurn(List<GameObject> trees)
    {
        // Implementação base ou abstrata
    }

    // Método para encontrar a árvore mais próxima
    protected GameObject FindClosestTree(List<GameObject> trees)
    {
        GameObject closestTree = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject tree in trees)
        {
            if (tree != null && tree.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, tree.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTree = tree;
                }
            }
        }

        return closestTree;
    }

    // Outros métodos comuns, como TakeDamage e DestroyEnemy
    public virtual void TakeDamage(int damageAmount)
    {
        enemyHealth -= damageAmount;
        Debug.Log("O inimigo tomou dano. Vida restante: " + enemyHealth);

        if (enemyHealth <= 0)
        {
            DestroyEnemy();
        }
    }

    protected virtual void DestroyEnemy()
    {
        Debug.Log("O inimigo foi destruído!");

        TurnManager turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.RemoveEnemy(this); // Agora passa 'this' corretamente
        }

        Destroy(gameObject);
    }
}
