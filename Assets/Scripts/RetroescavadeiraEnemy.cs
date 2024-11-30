using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

        // Se o inimigo estiver atordoado, n�o faz nada
        if (currentEffect == StatusEffect.Stunned)
            return;

        // Ajusta a velocidade se estiver sob o efeito de lentid�o
        if (currentEffect == StatusEffect.Slowed)
        {
            movementStep = originalMovementStep * 0.5f; // Reduz a velocidade pela metade
        }
        else
        {
            movementStep = originalMovementStep;
        }

        // L�gica para encontrar a �rvore alvo
        if (targetTree == null || !targetTree.activeInHierarchy)
        {
            targetTree = FindClosestTree(trees);
        }

        if (IsTreeNearby())
        {
            FaceTarget(targetTree.transform.position); // Vira para a �rvore antes de atacar
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
                Debug.Log("Retroescavadeira atacou a �rvore causando " + attackDamage + " de dano.");
            }
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        float duration = 1f; // Dura��o da movimenta��o (ajuste conforme necess�rio)

        // Calcula a dire��o para a qual o inimigo deve se virar
        Vector3 direction = (targetPosition - startPosition).normalized;
        direction.y = 0f; // Mant�m apenas a rota��o no plano XZ

        // Rota��o inicial e final
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(direction); // N�o inverte o direction

        while (elapsedTime < duration)
        {
            // Interpola a posi��o
            transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / duration));

            // Interpola a rota��o
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, (elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Garante que a posi��o e rota��o finais sejam exatamente as desejadas
        transform.position = targetPosition;
        transform.rotation = targetRotation;
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

            // Inicia a coroutine para movimenta��o suave
            StartCoroutine(MoveToPosition(nextPosition));

            // Ap�s mover o inimigo, registre o GridIndex
            Debug.Log("Inimigo " + gameObject.name + " est� na posi��o: " + transform.position + ", GridIndex: " + enemyGridIndex);

            Debug.Log("Retroescavadeira moveu-se para a posi��o: " + nextPosition);
        }
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f; // Mant�m apenas a rota��o no plano XZ

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction); // Corrigido para usar direction
            transform.rotation = targetRotation;
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
