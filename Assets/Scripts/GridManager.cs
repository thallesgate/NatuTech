using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject mapArea;
    public int gridSizeX = 10;
    public int gridSizeY = 10;
    public GameObject treePrefab;
    public GameObject housePrefab;
    public GameObject enemyPrefab;
    public TurnManager turnManager;
    public bool showGridLines = true;

    public List<EnemyBase> enemies = new List<EnemyBase>();

    [SerializeField]
    public int qtd_casa;
    [SerializeField]
    public int qtd_arvore;
    [SerializeField]
    public int qtd_inimigos;

    public Vector3[,] grid { get; private set; }
    public float cellSize { get; private set; }

    private void OnEnable()
    {
        if (mapArea == null)
        {
            Debug.LogError("mapArea não está atribuído!");
            return;
        }

        if (turnManager == null)
        {
            Debug.LogError("turnManager não está atribuído!");
            return;
        }

        CreateGrid();
        PlaceObjectsOnGrid(qtd_arvore, qtd_inimigos);
    }

    void CreateGrid()
    {
        float mapWidth = mapArea.transform.localScale.x;
        float mapHeight = mapArea.transform.localScale.z;

        cellSize = mapWidth / gridSizeX;

        grid = new Vector3[gridSizeX, gridSizeY];

        Matrix4x4 mapMatrix = Matrix4x4.TRS(mapArea.transform.position, mapArea.transform.rotation, Vector3.one);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                float posX = (x * cellSize) - mapWidth / 2 + cellSize / 2;
                float posZ = (y * cellSize) - mapHeight / 2 + cellSize / 2;
                Vector3 localPosition = new Vector3(posX, 0, posZ);

                grid[x, y] = mapMatrix.MultiplyPoint3x4(localPosition);
            }
        }
    }

    void PlaceObjectsOnGrid(int treeCount, int enemyCount)
    {
        if (treePrefab == null)
        {
            Debug.LogError("treePrefab não está atribuído!");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("enemyPrefab não está atribuído!");
            return;
        }

        // Coloca árvores
        for (int i = 0; i < treeCount; i++)
        {
            Vector3 position = GetRandomGridPosition();
            GameObject tree = Instantiate(treePrefab, position, Quaternion.identity);
            if (tree == null)
            {
                Debug.LogError("Instância de árvore falhou!");
            }
            //tree.transform.localScale = Vector3.one * 1.5f; // Define a escala para 5
            tree.transform.localScale = Vector3.one * 0.6f; // Define a escala para 5
            tree.transform.SetParent(mapArea.transform);
        }

        // Coloca inimigos
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 position = GetRandomGridPosition();

            GameObject enemyPrefabToInstantiate = DecideEnemyPrefab();
            GameObject enemy = Instantiate(enemyPrefabToInstantiate, position, Quaternion.identity);
            enemy.transform.SetParent(mapArea.transform);
            enemy.transform.localScale = Vector3.one * 1f; // Escala padrão

            EnemyBase enemyEngine = enemy.GetComponent<EnemyBase>();
            if (enemyEngine != null)
            {
                enemyEngine.InitializeGrid(grid, cellSize);
                enemies.Add(enemyEngine);

                // Log para depuração
                Vector2Int enemyGridIndex = GetGridIndexFromPosition(enemy.transform.position);
                Debug.Log("Inimigo criado: " + enemy.name + ", Posição: " + enemy.transform.position + ", GridIndex: " + enemyGridIndex);
            }
            else
            {
                Debug.LogError("EnemyBase não foi encontrado no prefab!");
            }
        }
    }


    public List<EnemyBase> GetEnemiesAtGridIndex(Vector2Int gridIndex)
    {
        List<EnemyBase> enemiesAtPosition = new List<EnemyBase>();

        foreach (EnemyBase enemy in enemies)
        {
            Vector2Int enemyGridIndex = GetGridIndexFromPosition(enemy.transform.position);

            if (enemyGridIndex == gridIndex)
            {
                enemiesAtPosition.Add(enemy);
            }
        }

        return enemiesAtPosition;
    }

    public Vector2Int GetGridIndexFromPosition(Vector3 position)
    {
        Vector3 localPosition = mapArea.transform.InverseTransformPoint(position);

        float adjustedX = (localPosition.x + (cellSize * gridSizeX) / 2);
        float adjustedZ = (localPosition.z + (cellSize * gridSizeY) / 2);

        int x = Mathf.FloorToInt(adjustedX / cellSize);
        int y = Mathf.FloorToInt(adjustedZ / cellSize);

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return new Vector2Int(x, y);
    }

    GameObject DecideEnemyPrefab()
    {
        GameObject retroescavadeiraPrefab = enemyPrefab;
        return retroescavadeiraPrefab;
    }

    public Vector3 GetRandomGridPosition()
    {
        int x = Random.Range(0, gridSizeX);
        int y = Random.Range(0, gridSizeY);
        return grid[x, y];
    }

    void OnDrawGizmos()
    {
        if (showGridLines && grid != null)
        {
            Gizmos.color = Color.red;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Gizmos.DrawWireCube(grid[x, y], new Vector3(cellSize, 0.01f, cellSize));
                }
            }
        }
    }
}
