using UnityEngine;
using System.Collections.Generic;

public class RetroescavadeiraEnemy : EnemyBase
{
    private float originalMovementStep;

    void Start()
    {
        base.Start();
        originalMovementStep = movementStep;
    }

    public override void StartTurn(List<GameObject> trees)
    {
        base.StartTurn(trees);

        // Se o inimigo estiver atordoado, não faz nada
        if (currentEffect == StatusEffect.Stunned)
            return;

        // Ajusta a velocidade se estiver sob o efeito de lentidão
        if (currentEffect == StatusEffect.Slowed)
        {
            movementStep = originalMovementStep * 0.5f; // Reduz a velocidade pela metade
        }
        else
        {
            movementStep = originalMovementStep;
        }

        // Lógica para encontrar a árvore alvo
        if (targetTree == null || !targetTree.activeInHierarchy)
        {
            targetTree = FindClosestTree(trees);
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

    private bool IsTreeNearby()
    {
        if (targetTree == null || gridPositions == null)
            return false;

        Vector2Int enemyGridIndex = GetGridIndexFromPosition(transform.position);
        Vector2Int treeGridIndex = GetGridIndexFromPosition(targetTree.transform.position);

        int distance = Mathf.Abs(enemyGridIndex.x - treeGridIndex.x) + Mathf.Abs(enemyGridIndex.y - treeGridIndex.y);

        return distance == 1;
    }

    private void AttackTree()
    {
        if (targetTree != null)
        {
            TreeEngine treeEngine = targetTree.GetComponent<TreeEngine>();
            if (treeEngine != null)
            {
                treeEngine.TakeDamage(attackDamage);
                Debug.Log("Retroescavadeira atacou a árvore causando " + attackDamage + " de dano.");
            }
        }
    }

    private void MoveTowardsTree()
    {
        if (targetTree != null && gridPositions != null)
        {
            Vector2Int enemyGridIndex = GetGridIndexFromPosition(transform.position);
            Vector2Int treeGridIndex = GetGridIndexFromPosition(targetTree.transform.position);

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

            enemyGridIndex += movement;
            enemyGridIndex = ClampGridIndex(enemyGridIndex);

            Vector3 nextPosition = gridPositions[enemyGridIndex.x, enemyGridIndex.y];
            transform.position = nextPosition;

            // Após mover o inimigo, registre o GridIndex
            Debug.Log("Inimigo " + gameObject.name + " está na posição: " + transform.position + ", GridIndex: " + enemyGridIndex);

            Debug.Log("Retroescavadeira moveu-se para a posição: " + nextPosition);
        }
    }

    private Vector2Int GetGridIndexFromPosition(Vector3 position)
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

    private Vector2Int ClampGridIndex(Vector2Int index)
    {
        int clampedX = Mathf.Clamp(index.x, 0, gridPositions.GetLength(0) - 1);
        int clampedY = Mathf.Clamp(index.y, 0, gridPositions.GetLength(1) - 1);
        return new Vector2Int(clampedX, clampedY);
    }
}
