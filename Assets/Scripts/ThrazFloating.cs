using UnityEngine;

public class ThrazFloating : MonoBehaviour
{
    // Amplitude do movimento vertical
    public float floatAmplitude = 0.1f;

    // Velocidade do movimento
    public float floatSpeed = 1f;

    // Posição inicial
    private Vector3 initialPosition;

    void Start()
    {
        // Salva a posição inicial do Thraz
        initialPosition = transform.position;
    }

    void Update()
    {
        // Calcula o deslocamento vertical usando uma função senoidal
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // Atualiza a posição do Thraz
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
