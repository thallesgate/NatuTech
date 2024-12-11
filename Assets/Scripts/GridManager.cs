using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject mapArea;
    public int gridSizeX = 10;
    public int gridSizeY = 10;
    public GameObject treePrefab;
    public TurnManager turnManager;
    public bool showGridLines = true;

    public List<EnemyBase> enemies = new List<EnemyBase>();

    [SerializeField]
    public int qtd_arvore;

    public int treeHealth = 100; // Novo parâmetro para a vida das árvores

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
        PlaceTreesOnGrid(qtd_arvore);
    }

    void CreateGrid()
    {
        MeshFilter mf = mapArea.GetComponentInChildren<MeshFilter>();
        if (mf == null)
        {
            Debug.LogError("Nenhum MeshFilter encontrado no mapArea!");
            return;
        }

        // Tamanho original do mesh local (sem escala)
        Vector3 originalSize = mf.sharedMesh.bounds.size;
        Vector3 finalScale = mapArea.transform.lossyScale;

        float mapWidth = originalSize.x * finalScale.x;
        float mapHeight = originalSize.z * finalScale.z;

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

    void PlaceTreesOnGrid(int treeCount)
    {
        if (treePrefab == null)
        {
            Debug.LogError("treePrefab não está atribuído!");
            return;
        }

        for (int i = 0; i < treeCount; i++)
        {
            Vector3 position = GetRandomGridPosition();
            GameObject tree = Instantiate(treePrefab, position, Quaternion.identity);

            // tree.transform.localScale = GlobalPlacementData.scale; // Apply placement scale compensation.
            
            if (tree == null)
            {
                Debug.LogError("Instância de árvore falhou!");
            }
            else
            {
                // Define a vida da árvore
                TreeEngine treeEngine = tree.GetComponent<TreeEngine>();
                if (treeEngine != null)
                {
                    treeEngine.Initialize(treeHealth);
                }
                else
                {
                    Debug.LogError("TreeEngine não encontrado no prefab da árvore!");
                }
            }
            tree.transform.SetParent(mapArea.transform);
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
