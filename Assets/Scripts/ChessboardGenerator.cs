using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine;

public class ChessboardGenerator : MonoBehaviour
{
    public GameObject squarePrefab;
    private GameObject square;
    public int boardSize = 8;
    public float squareSize = 0.2f; // Tamanho de cada quadrado

    private ARPlaneManager arPlaneManager;
    private Pose placementPose;
    private bool isPlacementValid = false;
    private bool boardOutOfView = false;
    private float timeSinceOutOfView = 0f;
    public float timeToRegenerate = 10f; // 10 segundos para mover o tabuleiro
    private bool isFirstSpawn = true; // Flag para controlar o primeiro spawn

    [SerializeField]
    private ARRaycastManager arRaycastManager;

    [SerializeField]
    private XROrigin arSessionOrigin;

    void Start()
    {
        // Verifica se o ARPlaneManager está presente
        arPlaneManager = GetComponent<ARPlaneManager>();

        if (arPlaneManager == null)
        {
            Debug.LogError("ARPlaneManager não encontrado!");
            return;
        }

        // Inscreve-se no evento de planos detectados
        arPlaneManager.planesChanged += OnPlanesChanged;
    }

    void OnDestroy()
    {
        // Desinscreve-se do evento quando o objeto for destruído
        arPlaneManager.planesChanged -= OnPlanesChanged;
    }

    void Update()
    {
        // Atualiza continuamente a pose de colocação com base na posição da câmera
        UpdatePlacementPose();

        // Verifica se o tabuleiro está fora da visão
        if (isPlacementValid)
        {
            if (!IsBoardInView(arSessionOrigin.Camera))
            {
                if (isFirstSpawn)
                {
                    // Primeira vez que o tabuleiro está fora da visão, reposiciona instantaneamente
                    if (square != null)
                    {
                        MoveBoardToNewPosition();
                    }
                    else
                    {
                        GenerateChessboard();
                    }
                    isFirstSpawn = false; // Agora a regeneração será controlada pelo timer
                }
                else if (!boardOutOfView)
                {
                    // Segunda vez e subsequentes, começa o timer
                    boardOutOfView = true;
                    timeSinceOutOfView = Time.time; // Marca o tempo atual
                }
                else if (Time.time - timeSinceOutOfView > timeToRegenerate)
                {
                    // Tabuleiro fora da visão por mais de 10 segundos, move para a nova posição
                    Debug.Log($"Tabuleiro fora da visão por {timeToRegenerate} segundos. Reposicionando.");
                    MoveBoardToNewPosition();
                    boardOutOfView = false; // Reseta o estado
                }
            }
            else
            {
                // O tabuleiro está na visão, reseta o timer
                boardOutOfView = false;
            }
        }
    }

    // Método chamado quando novos planos são detectados ou atualizados
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Opcional: Log ou qualquer ação adicional ao detectar novos planos
    }

    void UpdatePlacementPose()
    {
        var screenCenter = arSessionOrigin.Camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();

        // Faz raycast na área central da tela para encontrar um plano
        arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        isPlacementValid = hits.Count > 0;

        if (isPlacementValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = arSessionOrigin.Camera.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    void MoveBoardToNewPosition()
    {
        // Muda a posição do tabuleiro e seus objetos para a nova pose de colocação
        if (square != null)
        {
            // Atualiza a posição do tabuleiro
            square.transform.position = placementPose.position;
            square.transform.rotation = placementPose.rotation;

            Debug.Log("Tabuleiro movido para a nova posição.");
        }
    }

    void GenerateChessboard()
    {
        Quaternion additionalRotation = Quaternion.Euler(0, 90, 0);
        Quaternion finalRotation = placementPose.rotation * additionalRotation;

        // Instancia o quadrado se ainda não existir
        if (square == null)
        {
            square = Instantiate(squarePrefab, placementPose.position, finalRotation);
            Debug.Log("Tabuleiro gerado.");
        }
    }

    bool IsBoardInView(Camera camera)
    {
        // Verifica se o tabuleiro está visível na câmera fornecida
        if (square == null) return false;

        Vector3 viewportPosition = camera.WorldToViewportPoint(square.transform.position);

        // Verifica se o objeto está dentro da visão da câmera (no espaço da tela)
        return viewportPosition.z > 0 && viewportPosition.x >= 0 && viewportPosition.x <= 1 && viewportPosition.y >= 0 && viewportPosition.y <= 1;
    }
}
