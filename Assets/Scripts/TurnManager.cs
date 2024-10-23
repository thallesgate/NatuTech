using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnType
{
    Thraz,
    Enemies,
    Player
}

public class TurnManager : MonoBehaviour
{
    public GridManager gridManager;
    public ThrazEngine thrazEngine;

    private List<EnemyBase> enemies;
    private List<GameObject> trees;
    private Queue<TurnType> turnQueue;
    private bool isGameOver = false;

    // Adicionamos o contador de rodadas
    private int roundCounter = 0;

    // Propriedade pública para acessar o roundCounter
    public int RoundCounter => roundCounter;

    void Start()
    {
        enemies = new List<EnemyBase>(FindObjectsOfType<EnemyBase>());
        trees = new List<GameObject>(GameObject.FindGameObjectsWithTag("Tree"));

        if (thrazEngine == null)
        {
            thrazEngine = FindObjectOfType<ThrazEngine>();
            if (thrazEngine == null)
            {
                Debug.LogError("ThrazEngine não foi encontrado na cena.");
            }
        }

        turnQueue = new Queue<TurnType>();
        turnQueue.Enqueue(TurnType.Thraz);
        turnQueue.Enqueue(TurnType.Enemies);
        turnQueue.Enqueue(TurnType.Player);

        StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        while (!isGameOver)
        {
            TurnType currentTurn = turnQueue.Dequeue();

            switch (currentTurn)
            {
                case TurnType.Thraz:
                    yield return ThrazTurn();
                    break;
                case TurnType.Enemies:
                    yield return EnemyTurn();
                    break;
                case TurnType.Player:
                    yield return PlayerTurn();
                    break;
            }

            turnQueue.Enqueue(currentTurn);

            // Incrementa o contador de rodadas após o turno do jogador
            if (currentTurn == TurnType.Player)
            {
                roundCounter++;
                Debug.Log("Iniciando a rodada " + roundCounter);
            }

            yield return new WaitForSeconds(1.0f);
        }
    }


    IEnumerator ThrazTurn()
    {
        Debug.Log("Turno do Thraz");

        if (thrazEngine != null)
        {
            thrazEngine.StartTurn();
        }

        yield return new WaitForSeconds(1.0f);
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

    IEnumerator PlayerTurn()
    {
        Debug.Log("Turno do Jogador");

        // Implementação das ações do jogador

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
