using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Referência ao OrbManager
    public OrbManager orbManager;

    // Botões para selecionar os orbes
    public Button fireOrbButton;
    public Button waterOrbButton;
    public Button earthOrbButton;
    public Button airOrbButton;

    void Start()
    {
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
    private void OnFireButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Fire);
            Debug.Log("Orbe selecionado: Fire");
        }
        else
        {
            Debug.LogError("OrbManager não está atribuído no UIManager.");
        }
    }

    private void OnWaterButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Water);
            Debug.Log("Orbe selecionado: Water");
        }
        else
        {
            Debug.LogError("OrbManager não está atribuído no UIManager.");
        }
    }

    private void OnEarthButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Earth);
            Debug.Log("Orbe selecionado: Earth");
        }
        else
        {
            Debug.LogError("OrbManager não está atribuído no UIManager.");
        }
    }

    private void OnAirButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Air);
            Debug.Log("Orbe selecionado: Air");
        }
        else
        {
            Debug.LogError("OrbManager não está atribuído no UIManager.");
        }
    }
}
