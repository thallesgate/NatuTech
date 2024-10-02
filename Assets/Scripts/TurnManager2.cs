using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager2 : MonoBehaviour
{
    public GridManager gridManager;  // Reference to the GridManager script
    private List<GameObject> enemies; // List of enemies on the map
    private List<GameObject> trees;   // List of trees on the map

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the lists of enemies and trees based on the objects created in GridManager
        enemies = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        trees = new List<GameObject>(GameObject.FindGameObjectsWithTag("Tree"));

        // Start the turn routine
        StartCoroutine(TurnRoutine());
    }

    IEnumerator TurnRoutine()
    {
        while (true)
        {
            // Enemy Turn
            foreach (GameObject enemy in enemies)
            {
                EnemyEngine enemyEngine = enemy.GetComponent<EnemyEngine>();
                if (enemyEngine != null)
                {
                    enemyEngine.StartTurn(trees);
                    // Wait for a short duration to simulate turn-based actions
                    yield return new WaitForSeconds(0.5f);
                }
            }

            // Placeholder for player's turn
            // You can implement player's actions here

            // For now, wait some time to simulate player turn
            yield return new WaitForSeconds(1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Quantidade de Inimigos: {enemies.Count}");
        Debug.Log($"Quantidade de Árvores: {trees.Count}");
    }

    // Method to register enemies
    public void RegisterEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }

    // Method to register trees
    public void RegisterTree(GameObject tree)
    {
        trees.Add(tree);
    }

    // Method to remove an enemy when destroyed
    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }

    // Method to remove a tree when destroyed (if needed)
    public void RemoveTree(GameObject tree)
    {
        trees.Remove(tree);
    }
}
