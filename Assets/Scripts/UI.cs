using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField] GameObject sphereObject;
    private void OnEnable()
    {
        private Renderer sphereRenderer;
        sphereRenderer = sphereObject.GetComponent<Renderer>();

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button buttonFire = root.Q<Button>("ElementFireButton");
        Button buttonWater = root.Q<Button>("ElementWaterButton");
        Button buttonEarth = root.Q<Button>("ElementEarthButton");
        Button buttonAir = root.Q<Button>("ElementAirButton");
        Button buttonMap = root.Q<Button>("MapButton");

        buttonFire.clicked += () => sphereRenderer.material.color = Color.red;
        buttonWater.clicked += () => sphereRenderer.material.color = Color.blue;
        buttonEarth.clicked += () => sphereRenderer.material.color = Color.brown;
        buttonAir.clicked += () => sphereRenderer.material.color = Color.white;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
