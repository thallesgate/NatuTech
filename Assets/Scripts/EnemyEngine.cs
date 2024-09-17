using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEngine : MonoBehaviour
{
    public int attackDamage = 10;
    public float movementStep = 1.0f;  // Tamanho do movimento em uma célula do grid

    private GameObject targetTree;      // A árvore alvo mais próxima
    private bool isMoving = false;
    private Vector3[,] gridPositions;   // Grid de posições do tabuleiro

    // Inicializa o grid no inimigo (chamado pelo GridManager)
    public void InitializeGrid(Vector3[,] grid)
    {
        gridPositions = grid;
    }

    // Método para iniciar o turno do inimigo
    public void StartTurn(List<GameObject> trees)
    {
        // Se o alvo anterior foi destruído ou não existe, procure uma nova árvore
        if (targetTree == null || !targetTree.activeInHierarchy)
        {
            targetTree = FindClosestTree(trees);
        }

        // Verifica se há uma árvore adjacente
        if (IsTreeNearby())
        {
            // Se houver uma árvore adjacente, ataque
            AttackTree();
        }
        else
        {
            // Se não houver, mova-se em direção à árvore alvo
            MoveTowardsTree();
        }
    }

    // Verifica se existe uma árvore adjacente ao inimigo
    bool IsTreeNearby()
    {
        if (targetTree == null)
            return false;

        // Verifica se a distância entre o inimigo e a árvore é de 1 célula (adjacente)
        float distance = Vector3.Distance(transform.position, targetTree.transform.position);
        return distance <= movementStep;  // Considera adjacente se estiver a uma célula de distância ou menos
    }

    // Método para atacar a árvore
    void AttackTree()
    {
        if (targetTree != null)
        {
            TreeEngine treeEngine = targetTree.GetComponent<TreeEngine>();
            if (treeEngine != null)
            {
                treeEngine.TakeDamage(attackDamage);
                Debug.Log("Inimigo atacou a árvore causando " + attackDamage + " de dano.");
            }
        }
    }

    // Método para mover o inimigo em direção à árvore alvo
    void MoveTowardsTree()
    {
        if (targetTree != null && gridPositions != null)
        {
            // Calcula a direção para a árvore
            Vector3 direction = (targetTree.transform.position - transform.position).normalized;

            // Arredonda a direção para garantir que o inimigo se mova em uma célula exata
            Vector3 movement = new Vector3(Mathf.Round(direction.x), 0, Mathf.Round(direction.z));

            // Calcula a nova posição no grid
            Vector3 nextPosition = transform.position + movement * movementStep;

            // Limita a posição para estar dentro dos limites do grid
            nextPosition = ClampPositionToGrid(nextPosition);

            // Atualiza a posição do inimigo para a nova célula
            transform.position = nextPosition;

            Debug.Log("Inimigo moveu-se para a célula: " + nextPosition);
        }
    }

    // Método para garantir que o inimigo não saia dos limites do tabuleiro
    Vector3 ClampPositionToGrid(Vector3 position)
    {
        // Obtemos os limites do grid
        float minX = gridPositions[0, 0].x;
        float maxX = gridPositions[gridPositions.GetLength(0) - 1, 0].x;
        float minZ = gridPositions[0, 0].z;
        float maxZ = gridPositions[0, gridPositions.GetLength(1) - 1].z;

        // Limitamos a posição do inimigo para que esteja sempre dentro do tabuleiro
        float clampedX = Mathf.Clamp(position.x, minX, maxX);
        float clampedZ = Mathf.Clamp(position.z, minZ, maxZ);
        return new Vector3(clampedX, position.y, clampedZ);
    }

    // Método para encontrar a árvore mais próxima
    GameObject FindClosestTree(List<GameObject> trees)
    {
        GameObject closestTree = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject tree in trees)
        {
            if (tree != null && tree.activeInHierarchy) // Verifica se a árvore não foi destruída
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
}
