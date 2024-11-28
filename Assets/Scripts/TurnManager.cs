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

    // Adicionamos o contador de rodadas
    private int roundCounter = 0;

    // Propriedade p�blica para acessar o roundCounter
    public int RoundCounter => roundCounter;

    // Vari�vel para controlar o turno atual
    private TurnType currentTurn;

    // Vari�vel para sinalizar o fim do turno do jogador
    private bool playerTurnEnded = false;

    void Start()
    {
        enemies = new List<EnemyBase>(FindObjectsOfType<EnemyBase>());
        trees = new List<GameObject>(GameObject.FindGameObjectsWithTag("Tree"));

        if (thrazEngine == null)
        {
            thrazEngine = FindObjectOfType<ThrazEngine>();
            if (thrazEngine == null)
            {
                Debug.LogError("ThrazEngine n�o foi encontrado na cena.");
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

            // Ap�s o turno do jogador, aplica dano �s �rvores se a fuma�a estiver ativa
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

        // Aguarda at� que o jogador termine sua a��o
        yield return StartCoroutine(WaitForPlayerAction());

        Debug.Log("Turno do Jogador terminado");
    }

    IEnumerator WaitForPlayerAction()
    {
        // Exibe uma mensagem adicional no HUD
        if (turnText != null)
        {
            turnText.text += " - Fa�a sua jogada";
        }

        // Reseta a sinaliza��o do fim do turno do jogador
        playerTurnEnded = false;

        // Aguarda at� que o OrbManager chame EndPlayerTurn()
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

    // M�todo para verificar se � o turno do jogador
    public bool IsPlayerTurn()
    {
        return currentTurn == TurnType.Player && !isGameOver;
    }

    // M�todo para sinalizar o fim do turno do jogador
    public void EndPlayerTurn()
    {
        playerTurnEnded = true;
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
            GameOver("GAME OVER! As �rvores foram destru�das");
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
        int damage = 10; // Quantidade de dano causada pela fuma�a

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

        Debug.Log("A fuma�a t�xica causou dano �s �rvores.");
    }
}
