using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // UI
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI gameOverText; // Texto para exibir a mensagem de fim de jogo

    // Contador de rodadas
    private int roundCounter = 0;

    // Propriedade pública para acessar o roundCounter
    public int RoundCounter => roundCounter;

    // Variável para controlar o turno atual
    private TurnType currentTurn;

    // Variável para sinalizar o fim do turno do jogador
    private bool playerTurnEnded = false;

    // Variável para controlar se o jogador já agiu no turno
    public bool HasPlayerActed { get; private set; } = false;

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
            currentTurn = turnQueue.Dequeue();

            UpdateTurnText(currentTurn);

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

            // Após o turno do jogador, aplica dano às árvores se a fumaça estiver ativa
            if (currentTurn == TurnType.Player)
            {
                if (thrazEngine != null && thrazEngine.activeSmoke != null)
                {
                    ApplySmokeDamageToTrees();
                }

                roundCounter++;
                Debug.Log("Iniciando a rodada " + roundCounter);
            }

            turnQueue.Enqueue(currentTurn);

            yield return new WaitForSeconds(1.0f);
        }
    }

    void UpdateTurnText(TurnType currentTurn)
    {
        if (turnText != null)
        {
            switch (currentTurn)
            {
                case TurnType.Thraz:
                    turnText.text = "Turno: Thraz";
                    break;
                case TurnType.Enemies:
                    turnText.text = "Turno: Inimigos";
                    break;
                case TurnType.Player:
                    turnText.text = "Seu Turno";
                    break;
            }
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

        // Usando um loop 'for' inverso para evitar modificar a lista durante a iteração
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            EnemyBase enemy = enemies[i];
            if (enemy != null && !enemy.IsDestroyed)
            {
                enemy.StartTurn(trees);
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Remover inimigos destruídos após a iteração
        enemies.RemoveAll(enemy => enemy == null || enemy.IsDestroyed);
    }

    IEnumerator PlayerTurn()
    {
        Debug.Log("Turno do Jogador");

        // Resetamos a sinalização do fim do turno do jogador
        playerTurnEnded = false;
        HasPlayerActed = false;

        // Exibe uma mensagem adicional no HUD
        if (turnText != null)
        {
            turnText.text += " - Faça sua jogada";
        }

        // Aguarda até que o jogador termine sua ação
        yield return StartCoroutine(WaitForPlayerAction());

        Debug.Log("Turno do Jogador terminado");
    }

    public bool CanPlayerAct()
    {
        return IsPlayerTurn() && !HasPlayerActed && !isGameOver;
    }

    IEnumerator WaitForPlayerAction()
    {
        // Exibe uma mensagem adicional no HUD
        if (turnText != null)
        {
            turnText.text += " - Faça sua jogada";
        }

        // Reseta a sinalização do fim do turno do jogador
        playerTurnEnded = false;

        // Aguarda até que o OrbManager chame EndPlayerTurn()
        while (!playerTurnEnded)
        {
            yield return null;
        }

        // Remove a mensagem adicional
        if (turnText != null)
        {
            UpdateTurnText(TurnType.Player); // Atualiza o texto para remover o aviso
        }
    }

    // Método para verificar se é o turno do jogador
    public bool IsPlayerTurn()
    {
        return currentTurn == TurnType.Player && !isGameOver;
    }

    // Método para sinalizar o fim do turno do jogador
    public void EndPlayerTurn()
    {
        playerTurnEnded = true;
        HasPlayerActed = true;
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
        // Não removemos o inimigo aqui para evitar modificar a lista durante a iteração
        // A remoção é feita após a iteração no método EnemyTurn()
    }

    public void RemoveTree(GameObject tree)
    {
        trees.Remove(tree);

        if (trees.Count == 0)
        {
            GameOver("GAME OVER! As árvores foram destruídas");
        }
    }

    public void GameOver(string message)
    {
        Debug.Log(message);
        isGameOver = true;

        // Exibe a mensagem de fim de jogo na tela
        if (gameOverText != null)
        {
            gameOverText.text = message;
            gameOverText.gameObject.SetActive(true);
        }
    }

    void ApplySmokeDamageToTrees()
    {
        int damage = 10; // Quantidade de dano causada pela fumaça

        foreach (GameObject tree in trees)
        {
            if (tree != null)
            {
                TreeEngine treeEngine = tree.GetComponent<TreeEngine>();
                if (treeEngine != null)
                {
                    treeEngine.TakeDamage(damage);
                }
            }
        }

        Debug.Log("A fumaça tóxica causou dano às árvores.");
    }
}
