using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject mapArea; // Referência para o objeto plano do mapa

    public int gridSizeX = 10;
    public int gridSizeY = 10;
    public GameObject treePrefab;
    public GameObject housePrefab;
    public bool showGridLines = true; // Ativa/desativa a exibição das linhas do grid no editor

    [SerializeField]
    public int qtd_casa;
    [SerializeField]
    public int qtd_arvore;

    private Vector3[,] grid;
    private float cellSize;

    void Start()
    {
        CreateGrid();
        PlaceObjectsOnGrid(qtd_arvore, qtd_casa); // Exemplo com 5 árvores e 1 casa
    }

    void CreateGrid()
    {
        // Pega o tamanho da área do mapa com base na escala do plano
        float mapWidth = mapArea.transform.localScale.x * 1; // Ajustado para o tamanho do mapa
        float mapHeight = mapArea.transform.localScale.z * 1;

        // Calcula o tamanho das células do grid com base no tamanho do plano
        cellSize = mapWidth / gridSizeX;

        grid = new Vector3[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Calcula a posição de cada célula dentro do mapa
                float posX = (x * cellSize) + mapArea.transform.position.x - mapWidth / 2 + cellSize / 2;
                float posZ = (y * cellSize) + mapArea.transform.position.z - mapHeight / 2 + cellSize / 2;
                grid[x, y] = new Vector3(posX, mapArea.transform.position.y, posZ);
            }
        }
    }

    void PlaceObjectsOnGrid(int treeCount, int houseCount)
    {
        // Coloca árvores
        for (int i = 0; i < treeCount; i++)
        {
            Vector3 position = GetRandomGridPosition();
            GameObject tree = Instantiate(treePrefab, position, Quaternion.identity);
            tree.transform.localScale *= 0.1f; // Ajusta a escala conforme necessário

            // Define a árvore como filha do tabuleiro
            tree.transform.SetParent(mapArea.transform);
        }

        // Coloca casas
        for (int i = 0; i < houseCount; i++)
        {
            Vector3 position = GetRandomGridPosition();

            GameObject house = Instantiate(housePrefab, position, Quaternion.identity);
            house.transform.localScale *= 0.1f; // Ajusta a escala conforme necessário

            // Define a casa como filha do tabuleiro
            house.transform.SetParent(mapArea.transform);
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
