using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Refer�ncia ao OrbManager
    public OrbManager orbManager;

    // Bot�es para selecionar os orbes
    public Button fireOrbButton;
    public Button waterOrbButton;
    public Button earthOrbButton;
    public Button airOrbButton;

    void Start()
    {
        // Adiciona os event listeners aos bot�es
        if (fireOrbButton != null)
        {
            fireOrbButton.onClick.AddListener(OnFireButtonClicked);
        }
        else
        {
            Debug.LogWarning("Fire Orb Button n�o est� atribu�do.");
        }

        if (waterOrbButton != null)
        {
            waterOrbButton.onClick.AddListener(OnWaterButtonClicked);
        }
        else
        {
            Debug.LogWarning("Water Orb Button n�o est� atribu�do.");
        }

        if (earthOrbButton != null)
        {
            earthOrbButton.onClick.AddListener(OnEarthButtonClicked);
        }
        else
        {
            Debug.LogWarning("Earth Orb Button n�o est� atribu�do.");
        }

        if (airOrbButton != null)
        {
            airOrbButton.onClick.AddListener(OnAirButtonClicked);
        }
        else
        {
            Debug.LogWarning("Air Orb Button n�o est� atribu�do.");
        }
    }

    void OnDestroy()
    {
        // Remove os event listeners para evitar vazamentos de mem�ria
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

    // M�todos chamados quando os bot�es s�o clicados
    private void OnFireButtonClicked()
    {
        if (orbManager != null)
        {
            orbManager.SetCurrentOrbType(OrbType.Fire);
            Debug.Log("Orbe selecionado: Fire");
        }
        else
        {
            Debug.LogError("OrbManager n�o est� atribu�do no UIManager.");
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
            Debug.LogError("OrbManager n�o est� atribu�do no UIManager.");
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
            Debug.LogError("OrbManager n�o est� atribu�do no UIManager.");
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
            Debug.LogError("OrbManager n�o est� atribu�do no UIManager.");
        }
    }
}
