using UnityEngine;
using TMPro;

public class TextPulsing : MonoBehaviour
{
    private TextMeshProUGUI text;
    private bool increasing = true;
    private float alpha = 0.5f;
    [SerializeField] private float minimumAlpha = 0.3f;
    [SerializeField] private float rate = 1.5f;
    private float fontSize;
    [SerializeField] private float sizeVariance = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        fontSize = text.fontSize;
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Time.deltaTime;
        if (increasing)
        {
            alpha += delta*rate;
        }
        else
        {
            alpha -= delta*rate;
        }

        if(alpha >= 1f)
        {
            increasing = false;
        }else if(alpha <= minimumAlpha)
        {
            increasing = true;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
        text.fontSize = fontSize - (sizeVariance * alpha * -1);
    }
}
