using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private List<EnemyBase> enemies;
    private List<GameObject> trees;
    private bool isEnemyTurn = false;
    private bool isGameOver = false;

    void Start()
    {
        enemies = new List<EnemyBase>(FindObjectsOfType<EnemyBase>());
        trees = new List<GameObject>(GameObject.FindGameObjectsWithTag("Tree"));

        StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        while (!isGameOver)
        {
            if (isEnemyTurn)
            {
                yield return EnemyTurn();
                isEnemyTurn = false;
            }
            else
            {
                yield return TreeTurn();
                isEnemyTurn = true;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Turno dos inimigos");

        foreach (EnemyBase enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.StartTurn(trees);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator TreeTurn()
    {
        Debug.Log("Turno das árvores");
        yield return new WaitForSeconds(1.0f);
    }

    public void RegisterEnemy(EnemyBase enemy)
    {
        enemies.Add(enemy);
    }

    public void RegisterTree(GameObject tree)
    {
        trees.Add(tree);
    }

    public void RemoveEnemy(EnemyBase enemy)
    {
        enemies.Remove(enemy);
    }

    public void RemoveTree(GameObject tree)
    {
        trees.Remove(tree);

        if (trees.Count == 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over! Todas as árvores foram destruídas.");
        isGameOver = true;
    }
}
