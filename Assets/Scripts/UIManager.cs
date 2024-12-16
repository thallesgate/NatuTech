using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    // Referência ao OrbManager
    public OrbManager orbManager;

    // Botões para selecionar os orbes
    public Button fireOrbButton;
    public Button waterOrbButton;
    public Button earthOrbButton;
    public Button airOrbButton;
    public GameObject prefabToSpawn;

    // Sons
    private AudioController audioController;
    [SerializeField] private string SelectFogo = "SelectOrbFogo";
    [SerializeField] private string SelectTerra = "SelectOrbTerra";
    [SerializeField] private string SelectAr = "SelectOrbAr";
    [SerializeField] private string SelectAgua = "SelectOrbAgua";

    void Start()
    {
        audioController = FindFirstObjectByType<AudioController>();

        // Adiciona os event listeners aos botões
        if (fireOrbButton != null)
        {
            fireOrbButton.onClick.AddListener(OnFireButtonClicked);
        }
        else
        {
            Debug.LogWarning("Fire Orb Button não está atribuído.");
        }

        if (waterOrbButton != null)
        {
            waterOrbButton.onClick.AddListener(OnWaterButtonClicked);
        }
        else
        {
            Debug.LogWarning("Water Orb Button não está atribuído.");
        }

        if (earthOrbButton != null)
        {
            earthOrbButton.onClick.AddListener(OnEarthButtonClicked);
        }
        else
        {
            Debug.LogWarning("Earth Orb Button não está atribuído.");
        }

        if (airOrbButton != null)
        {
            airOrbButton.onClick.AddListener(OnAirButtonClicked);
        }
        else
        {
            Debug.LogWarning("Air Orb Button não está atribuído.");
        }
    }

    void OnDestroy()
    {
        // Remove os event listeners para evitar vazamentos de memória
        if (fireOrbButton != null)
        {
            fireOrbButton.onClick.RemoveListener(OnFireButtonClicked);
        }

        if (waterOrbButton != null)
        {
            waterOrbButton.onClick.RemoveListener(OnWaterButtonClicked);
        }

        if (earthOrbButton != null)
        {
            earthOrbButton.onClick.RemoveListener(OnEarthButtonClicked);
        }

        if (airOrbButton != null)
        {
            airOrbButton.onClick.RemoveListener(OnAirButtonClicked);
        }
    }

    // Métodos chamados quando os botões são clicados
    public void OnFireButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Fire);
            audioController.PlaySound(SelectFogo);
            Debug.Log("Orbe selecionado: Fire");
        }
        else
        {
            Debug.LogError("OrbManager não está atribuído no UIManager.");
        }
    }

    public void OnWaterButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Water);
            audioController.PlaySound(SelectAgua);
            Debug.Log("Orbe selecionado: Water");
        }
        else
        {
            Debug.LogError("OrbManager não está atribuído no UIManager.");
        }
    }

    public void OnEarthButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Earth);
            audioController.PlaySound(SelectTerra);
            Debug.Log("Orbe selecionado: Earth");
        }
        else
        {
            Debug.LogError("OrbManager não está atribuído no UIManager.");
        }
    }

    public void OnAirButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Air);
            audioController.PlaySound(SelectAr);
            Debug.Log("Orbe selecionado: Air");
        }
        else
        {
            Debug.LogError("OrbManager não está atribuído no UIManager.");
        }
    }
    public void OnResetButtonClicked()
    {
        //GameObject faseObject = GameObject.FindGameObjectWithTag("Fase");
        //List<GameObject> trees = new List<GameObject>(GameObject.FindGameObjectsWithTag("Tree"));
        //List<GameObject> efeitos = new List<GameObject>(GameObject.FindGameObjectsWithTag("Efeitos"));

        
        DestroyObjectsWithTag.DestroyObjects("Tree");
        DestroyObjectsWithTag.DestroyObjects("Efeitos");
        DestroyObjectsWithTag.DestroyObjects("TrajectoryPoint");
        audioController.PlaySound("Tema");
        GameObject sceneInstance = Instantiate(prefabToSpawn, GlobalPlacementData.position, GlobalPlacementData.rotation);
        sceneInstance.transform.localScale *= GlobalPlacementData.scale.x;
        DestroyObjectsWithTag.DestroyObject("Fase");

        
    }
}
