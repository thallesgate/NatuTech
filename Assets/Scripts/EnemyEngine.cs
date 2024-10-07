using System.Collections.Generic;
using UnityEngine;

public class EnemyEngine : MonoBehaviour
{
    public int attackDamage = 10;
    private float movementStep;  // Será inicializado com o tamanho da célula do grid

    private GameObject targetTree;      // A árvore alvo mais próxima
    private Vector3[,] gridPositions;   // Grid de posições do tabuleiro
    private float cellSize;             // Tamanho da célula do grid
    [SerializeField] private int enemyHealth = 100;

    // Inicializa o grid no inimigo (chamado pelo GridManager)
    public void InitializeGrid(Vector3[,] grid, float cellSize)
    {
        gridPositions = grid;
        this.cellSize = cellSize;
        movementStep = cellSize; // Garantir que o movimento corresponde ao tamanho da célula
    }

    // Método para iniciar o turno do inimigo
    public void StartTurn(List<GameObject> trees)
    {
        Debug.Log("Inimigo iniciando turno.");

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

    // Verifica se existe uma árvore adjacente ao inimigo
    bool IsTreeNearby()
    {
        if (targetTree == null || gridPositions == null)
            return false;

        // Obtemos os índices do inimigo e da árvore
        Vector2Int enemyGridIndex = GetGridIndexFromPosition(transform.position);
        Vector2Int treeGridIndex = GetGridIndexFromPosition(targetTree.transform.position);

        // Calcula a distância Manhattan entre os índices
        int distance = Mathf.Abs(enemyGridIndex.x - treeGridIndex.x) + Mathf.Abs(enemyGridIndex.y - treeGridIndex.y);

        // Verifica se a distância é 1 (adjacente)
        return distance == 1;
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
            // Converte as posições em índices do grid
            Vector2Int enemyGridIndex = GetGridIndexFromPosition(transform.position);
            Vector2Int treeGridIndex = GetGridIndexFromPosition(targetTree.transform.position);

            // Calcula a diferença nos índices
            int deltaX = treeGridIndex.x - enemyGridIndex.x;
            int deltaY = treeGridIndex.y - enemyGridIndex.y;

            Vector2Int movement = Vector2Int.zero;

            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                movement.x = Mathf.Clamp(deltaX, -1, 1);
            }
            else if (deltaY != 0)
            {
                movement.y = Mathf.Clamp(deltaY, -1, 1);
            }

            // Atualiza os índices do inimigo
            enemyGridIndex += movement;

            // Garante que os índices estão dentro dos limites do grid
            enemyGridIndex = ClampGridIndex(enemyGridIndex);

            // Atualiza a posição do inimigo com base nos novos índices
            Vector3 nextPosition = gridPositions[enemyGridIndex.x, enemyGridIndex.y];
            transform.position = nextPosition;

            Debug.Log("Inimigo moveu-se para a posição: " + nextPosition);
        }
    }

    // Converte uma posição no espaço mundial em um índice do grid
    Vector2Int GetGridIndexFromPosition(Vector3 position)
    {
        int closestX = 0;
        int closestY = 0;
        float minDistance = Mathf.Infinity;

        for (int x = 0; x < gridPositions.GetLength(0); x++)
        {
            for (int y = 0; y < gridPositions.GetLength(1); y++)
            {
                float distance = Vector3.Distance(position, gridPositions[x, y]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestX = x;
                    closestY = y;
                }
            }
        }

        return new Vector2Int(closestX, closestY);
    }

    // Garante que os índices estão dentro dos limites do grid
    Vector2Int ClampGridIndex(Vector2Int index)
    {
        int clampedX = Mathf.Clamp(index.x, 0, gridPositions.GetLength(0) - 1);
        int clampedY = Mathf.Clamp(index.y, 0, gridPositions.GetLength(1) - 1);
        return new Vector2Int(clampedX, clampedY);
    }

    public void TakeDamage(int damageAmount)
    {
        enemyHealth -= damageAmount;
        Debug.Log("O Inimigo tomou dano. Vida restante: " + enemyHealth);

        // Verifica se a vida do inimigo chegou a zero ou menos
        if (enemyHealth <= 0)
        {
            DestroyEnemy();
        }
    }

    // Método para destruir o inimigo
    void DestroyEnemy()
    {
        Debug.Log("O inimigo foi destruído!");

        // Notifica o TurnManager que este inimigo foi destruído
        TurnManager turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.RemoveEnemy(gameObject); // Remove o inimigo da lista de inimigos
        }

        Destroy(gameObject); // Remove o objeto da cena
    }
}
