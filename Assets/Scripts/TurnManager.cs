using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Para reiniciar a cena (opcional)

public class TurnManager : MonoBehaviour
{
    public GridManager gridManager;  // Referência ao script de grid
    private List<GameObject> enemies; // Lista de inimigos no mapa
    private List<GameObject> trees;   // Lista de árvores no mapa
    private bool isEnemyTurn = false;
    private bool isGameOver = false;  // Variável para controlar o estado do jogo

    void Start()
    {
        // Inicializa as listas de inimigos e árvores com base nos objetos criados no GridManager
        enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        trees = new List<GameObject>(GameObject.FindGameObjectsWithTag("Tree"));

        // Começa o primeiro turno
        StartCoroutine(NextTurn());
    }

    IEnumerator NextTurn()
    {
        while (!isGameOver)
        {
            // Alterna entre turnos de inimigos e outros (por exemplo, jogador)
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

            // Pausa entre turnos para permitir que o jogador veja as ações
            yield return new WaitForSeconds(1.0f);
        }
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Turno dos inimigos");

        // Loop por todos os inimigos e faça-os agir (movimentar ou atacar)
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                EnemyEngine enemyEngine = enemy.GetComponent<EnemyEngine>();
                if (enemyEngine != null)
                {
                    // Passa a lista de árvores para o inimigo
                    enemyEngine.StartTurn(trees);
                }
            }
            yield return new WaitForSeconds(0.5f); // Intervalo entre ações dos inimigos
        }
    }

    IEnumerator TreeTurn()
    {
        Debug.Log("Turno das árvores");

        // Implementação do turno das árvores (se necessário)
        yield return new WaitForSeconds(1.0f);
    }

    public void RegisterEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }

    public void RegisterTree(GameObject tree)
    {
        trees.Add(tree);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }

    public void RemoveTree(GameObject tree)
    {
        trees.Remove(tree);

        // Verifica se todas as árvores foram destruídas
        if (trees.Count == 0)
        {
            GameOver(); // Chama o Game Over quando não há mais árvores
        }
    }

    // Método para lidar com Game Over
    void GameOver()
    {
        Debug.Log("Game Over! Todas as árvores foram destruídas.");

        isGameOver = true;

        // Aqui você pode implementar o que acontecerá no Game Over:
        // - Mostrar uma tela de game over
        // - Reiniciar a cena
        // - Parar o jogo

        // Exemplo: reiniciar a cena atual (opcional)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Alternativamente, você pode exibir uma UI ou realizar outra ação
        // Aqui você pode implementar a lógica para mostrar uma tela de Game Over
        // ou realizar outras ações necessárias.
    }
}
