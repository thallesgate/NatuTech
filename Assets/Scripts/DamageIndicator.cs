using UnityEngine;
using System.Collections;

public class DamageIndicator : MonoBehaviour
{
    private Renderer[] renderers;
    private Color[] originalColors;

    // Variáveis para o efeito de piscar
    private Coroutine damageFlashCoroutine;
    private bool isFlashing = false;

    // Variável pública para ajustar o tempo de piscar
    [Tooltip("Duração do efeito de piscar em segundos.")]
    public float flashDuration = 0.1f;

    void Start()
    {
        // Obtém todos os Renderers nos GameObjects filhos
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            // Cria uma instância do material para evitar alterar materiais compartilhados
            renderers[i].material = new Material(renderers[i].material);
            originalColors[i] = renderers[i].material.color;
        }
    }

    public void FlashDamage()
    {
        if (!isFlashing)
        {
            damageFlashCoroutine = StartCoroutine(DamageFlashEffect());
        }
    }

    private IEnumerator DamageFlashEffect()
    {
        isFlashing = true;

        // Altera a cor de todos os Renderers para vermelho
        foreach (var renderer in renderers)
        {
            renderer.material.color = Color.red;
        }

        // Aguarda o período definido
        yield return new WaitForSeconds(flashDuration);

        // Reverte para a cor original
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }

        isFlashing = false;
    }
}
