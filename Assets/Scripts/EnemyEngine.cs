using System.Collections.Generic;
using UnityEngine;

public class EnemyEngine : MonoBehaviour
{
    public int attackDamage = 30;
    private float movementStep;
    private GameObject targetTree;
    private Vector3[,] gridPositions;
    [SerializeField] private int enemyHealth = 100;

<<<<<<< Updated upstream
    private GameObject targetTree;      // A árvore alvo mais próxima
    private bool isMoving = false;
    private Vector3[,] gridPositions;   // Grid de posições do tabuleiro
=======
    // Grid origin variables
    private float gridOriginX;
    private float gridOriginZ;
>>>>>>> Stashed changes

    // Grid size variables
    private int gridSizeX;
    private int gridSizeY;

    // Initializes the grid in the enemy (called by GridManager)
    public void InitializeGrid(Vector3[,] grid, float cellSize)
    {
        gridPositions = grid;
        movementStep = cellSize;

        gridSizeX = grid.GetLength(0);
        gridSizeY = grid.GetLength(1);

        gridOriginX = gridPositions[0, 0].x - (movementStep / 2);
        gridOriginZ = gridPositions[0, 0].z - (movementStep / 2);
    }

    // Method to start the enemy's turn
    public void StartTurn(List<GameObject> trees)
    {
        // Clean up the target tree reference if it's destroyed
        if (!targetTree || !targetTree.activeInHierarchy)
        {
            targetTree = FindClosestTree(trees);
        }

        if (!targetTree)
        {
            Debug.Log("No target tree found.");
            return;
        }

        if (IsTreeNearby())
        {
            AttackTree();
        }
        else
        {
            MoveTowardsTree();
        }
    }

    // Checks if there is a tree adjacent to the enemy
    bool IsTreeNearby()
    {
        if (!targetTree || !targetTree.activeInHierarchy)
            return false;

        int enemyX = GetGridX(transform.position.x);
        int enemyY = GetGridY(transform.position.z);

        int treeX = GetGridX(targetTree.transform.position.x);
        int treeY = GetGridY(targetTree.transform.position.z);

        int dx = Mathf.Abs(enemyX - treeX);
        int dy = Mathf.Abs(enemyY - treeY);

        bool isAdjacent = (dx == 1 && dy == 0) || (dx == 0 && dy == 1);

        Debug.Log($"IsTreeNearby: Enemy({enemyX},{enemyY}), Tree({treeX},{treeY}), Adjacent: {isAdjacent}");
        return isAdjacent;
    }

    // Method to attack the tree
    void AttackTree()
    {
        if (targetTree != null)
        {
            TreeEngine treeEngine = targetTree.GetComponent<TreeEngine>();
            if (treeEngine != null)
            {
                treeEngine.TakeDamage(attackDamage);
                Debug.Log("Enemy attacked the tree causing " + attackDamage + " damage.");
            }
        }
    }

    // Method to move the enemy towards the target tree
    void MoveTowardsTree()
    {
        if (targetTree != null)
        {
            int enemyX = GetGridX(transform.position.x);
            int enemyY = GetGridY(transform.position.z);

            int treeX = GetGridX(targetTree.transform.position.x);
            int treeY = GetGridY(targetTree.transform.position.z);

            int deltaX = treeX - enemyX;
            int deltaY = treeY - enemyY;

            int moveX = 0;
            int moveY = 0;

            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                moveX = deltaX > 0 ? 1 : -1;
            }
            else if (deltaY != 0)
            {
                moveY = deltaY > 0 ? 1 : -1;
            }
            else
            {
                // Se deltaX e deltaY forem zero, o inimigo já está na posição da árvore
                return;
            }

            int nextX = enemyX + moveX;
            int nextY = enemyY + moveY;

            // Garante que nextX e nextY estejam dentro dos limites da grade
            nextX = Mathf.Clamp(nextX, 0, gridSizeX - 1);
            nextY = Mathf.Clamp(nextY, 0, gridSizeY - 1);

            // Obtém a posição mundial da próxima célula
            float nextPosX = gridPositions[nextX, nextY].x;
            float nextPosZ = gridPositions[nextX, nextY].z;

            Vector3 nextPosition = new Vector3(nextPosX, transform.position.y, nextPosZ);

            // Atualiza a posição do inimigo
            transform.position = nextPosition;

            Debug.Log($"Inimigo moveu-se para a célula: ({nextX}, {nextY}) na posição: {nextPosition}");
        }
    }

    // Method to get grid index based on world X position
    int GetGridX(float x)
    {
        int gridX = Mathf.RoundToInt((x - gridOriginX) / movementStep);
        Debug.Log($"GetGridX: x={x}, gridOriginX={gridOriginX}, movementStep={movementStep}, gridX={gridX}");
        return gridX;
    }

    // Method to get grid index based on world Z position
    int GetGridY(float z)
    {
        int gridY = Mathf.RoundToInt((z - gridOriginZ) / movementStep);
        Debug.Log($"GetGridY: z={z}, gridOriginZ={gridOriginZ}, movementStep={movementStep}, gridY={gridY}");
        return gridY;
    }

    // Method to find the closest tree
    GameObject FindClosestTree(List<GameObject> trees)
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
<<<<<<< Updated upstream
=======

    public void TakeDamage(int damageAmount)
    {
        enemyHealth -= damageAmount;
        Debug.Log("Enemy took damage. Remaining health: " + enemyHealth);

        if (enemyHealth <= 0)
        {
            DestroyEnemy();
        }
    }

    void DestroyEnemy()
    {
        Debug.Log("The enemy was destroyed!");

        TurnManager turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.RemoveEnemy(gameObject);
        }

        Destroy(gameObject);
    }
>>>>>>> Stashed changes
}
