using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject mapArea; // Referência para o objeto plano do mapa
    public int gridSizeX = 10;
    public int gridSizeY = 10;
    public GameObject treePrefab;
    public GameObject housePrefab;
    public GameObject enemyPrefab; // Prefab do inimigo
    public TurnManager turnManager;
    public bool showGridLines = true; // Ativa/desativa a exibição das linhas do grid no editor

    [SerializeField]
    public int qtd_casa;
    [SerializeField]
    public int qtd_arvore;
    [SerializeField]
    public int qtd_inimigos; // Quantidade de inimigos a ser gerada

    private Vector3[,] grid; // Mantemos a matriz de posições para armazenar o grid
    private float cellSize;

    private void OnEnable()
    {
        // Verificar se mapArea e turnManager estão atribuídos corretamente
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
        PlaceObjectsOnGrid(qtd_arvore, qtd_inimigos); // Exemplo com 5 árvores, 1 casa e 1 inimigo  
    }

    void CreateGrid()
    {
        // Pega o tamanho da área do mapa com base na escala do plano
        float mapWidth = mapArea.transform.localScale.x;
        float mapHeight = mapArea.transform.localScale.z;

        // Calcula o tamanho das células do grid com base no tamanho do plano
        cellSize = mapWidth / gridSizeX;

        // Cria a matriz para armazenar as posições das células do grid
        grid = new Vector3[gridSizeX, gridSizeY];

        // Cria a matriz de transformação baseada na posição, rotação e escala do mapArea
        Matrix4x4 mapMatrix = Matrix4x4.TRS(mapArea.transform.position, mapArea.transform.rotation, Vector3.one);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Calcula a posição local de cada célula dentro do mapa
                float posX = (x * cellSize) - mapWidth / 2 + cellSize / 2;
                float posZ = (y * cellSize) - mapHeight / 2 + cellSize / 2;
                Vector3 localPosition = new Vector3(posX, 0, posZ); // Posicionamos no plano Y=0

                // Aplica a matriz de transformação para obter a posição global rotacionada
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
            tree.transform.localScale *= 0.1f;
            tree.transform.SetParent(mapArea.transform);
            // Registra a árvore no TurnManager
            //turnManager.RegisterTree(tree);

            
        }

        // Coloca inimigos
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 position = GetRandomGridPosition();
            GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            if (enemy == null)
            {
                Debug.LogError("Instância de inimigo falhou!");
            }
            enemy.transform.localScale *= 0.1f;

            // Inicializa o grid no inimigo
            EnemyEngine enemyEngine = enemy.GetComponent<EnemyEngine>();
            if (enemyEngine != null)
            {
                enemyEngine.InitializeGrid(grid, cellSize);
            }
            else
            {
                Debug.LogError("enemyEngine não foi encontrado no prefab!");
            }

            // Registra o inimigo no TurnManager
            //turnManager.RegisterEnemy(enemy);

            enemy.transform.SetParent(mapArea.transform);
        }
    }

    Vector3 GetRandomGridPosition()
    {
        int x = Random.Range(0, gridSizeX);
        int y = Random.Range(0, gridSizeY);
        return grid[x, y];
    }

    // Desenha o grid no editor para propósitos de debug
    void OnDrawGizmos()
    {
        if (showGridLines && grid != null)
        {
            Gizmos.color = Color.red;

            // Desenha linhas de grid
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    // Desenha um cubo wireframe representando cada célula do grid
                    Gizmos.DrawWireCube(grid[x, y], new Vector3(cellSize, 0.01f, cellSize));
                }
            }
        }
    }
}
