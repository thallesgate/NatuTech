using UnityEngine;
using System.Collections.Generic;

public class ThrazEngine : MonoBehaviour
{
    public List<SummonableEnemy> summonableEnemies; // Lista dos inimigos que o ThrazEngine pode invocar
    public GridManager gridManager;
    public TurnManager turnManager;

    private bool isFirstTurn = true;

    void Start()
    {
        foreach (var enemy in summonableEnemies)
        {
            // Inicializa turnsSinceLastSummon para permitir invocação no primeiro turno
            enemy.Initialize();
        }
    }

    public void StartTurn()
    {
        Debug.Log("ThrazEngine está agindo.");

        UpdateEnemyCooldowns();
        TrySummonEnemy();

        // Após o primeiro turno, definimos isFirstTurn como false
        isFirstTurn = false;
    }

    void UpdateEnemyCooldowns()
    {
        foreach (var enemy in summonableEnemies)
        {
            if (enemy.turnsSinceLastSummon < enemy.minTurnsBetweenSummons)
            {
                enemy.turnsSinceLastSummon++;
            }
        }
    }

    void TrySummonEnemy()
    {
        // Lógica para decidir qual inimigo invocar
        foreach (var enemy in summonableEnemies)
        {
            if (CanSummonEnemy(enemy))
            {
                SummonEnemy(enemy);
                break; // Invoca apenas um inimigo por turno
            }
        }
    }

    bool CanSummonEnemy(SummonableEnemy enemy)
    {
        if (enemy.currentCount >= enemy.maxCount)
            return false;

        // Modificamos a condição para permitir invocação no primeiro turno
        if (!isFirstTurn && enemy.turnsSinceLastSummon < enemy.minTurnsBetweenSummons)
            return false;

        return true;
    }

    void SummonEnemy(SummonableEnemy enemy)
    {
        Vector3 spawnPosition = gridManager.GetRandomGridPosition();

        GameObject newEnemy = Instantiate(enemy.enemyPrefab, spawnPosition, Quaternion.identity);
        newEnemy.transform.localScale *= 0.1f;
        newEnemy.transform.SetParent(gridManager.mapArea.transform);

        EnemyBase enemyEngine = newEnemy.GetComponent<EnemyBase>();
        if (enemyEngine != null)
        {
            enemyEngine.InitializeGrid(gridManager.grid, gridManager.cellSize);
            enemyEngine.summonableEnemyType = enemy;
        }
        else
        {
            Debug.LogError("EnemyBase não foi encontrado no prefab!");
        }

        turnManager.RegisterEnemy(enemyEngine);

        enemy.currentCount++;
        enemy.turnsSinceLastSummon = 0;

        Debug.Log("ThrazEngine invocou um inimigo: " + newEnemy.name);
    }

    public void OnEnemyDestroyed(SummonableEnemy enemyType)
    {
        if (enemyType.currentCount > 0)
        {
            enemyType.currentCount--;
        }
    }
}
