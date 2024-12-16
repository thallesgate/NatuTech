using SOG.CVDFilter;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class DaltonismoDropDown : MonoBehaviour
{
    private CVDFilter cvdFilter;
    [SerializeField] private TMP_Dropdown dropdown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cvdFilter = FindFirstObjectByType<CVDFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private string GetDropdownValue()
    {
        int selectedEntryIndex = dropdown.value;
        string selectedOption = dropdown.options[selectedEntryIndex].text;
        return selectedOption;
    }
    public void OnChangeFilter()
    {
        int selectedEntryIndex = dropdown.value;
        string selectedOption = dropdown.options[selectedEntryIndex].text;

        switch (selectedOption) {
            case "Protanopia":
                Debug.Log("DaltonismoDropDown: Protanopia!");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Protanopia;
                break;
            case "Protanopia Leve":
                Debug.Log("DaltonismoDropDown: Protanopia leve!");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Protanomaly;
                break;
            case "Deuteranopia":
                Debug.Log("DaltonismoDropDown: Deuteranopia!");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Deuteranopia;
                break;
            case "Deuteranopia Leve":
                Debug.Log("DaltonismoDropDown: Deuteranopia leve!");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Deuteranomaly;
                break;
            case "Tritanopia":
                Debug.Log("DaltonismoDropDown: Tritanopia!");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Tritanopia;
                break;
            case "Tritanopia Leve":
                Debug.Log("DaltonismoDropDown: Tritanopia leve!");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Tritanomaly;
                break;
            case "Acromatopsia":
                Debug.Log("DaltonismoDropDown: Acromatopsia!");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Achromatopsia;
                break;
            case "Acromatopsia Leve":
                Debug.Log("DaltonismoDropDown: Acromatopsia leve!");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Achromatomaly;
                break;
            default:
                Debug.Log("DaltonismoDropDown: default! (Desligado)");
                cvdFilter.currentType = SOG.CVDFilter.VisionTypeNames.Normal;
                break;
        }
        
        cvdFilter.ChangeProfile();
    }
}
