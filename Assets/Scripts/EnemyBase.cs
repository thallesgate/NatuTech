using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum StatusEffect
{
    None,
    Burning,
    Slowed,
    Stunned,
    Knockback
}

public abstract class EnemyBase : MonoBehaviour
{
    public int attackDamage = 10;
    protected float movementStep;
    protected Vector3[,] gridPositions;
    protected float cellSize;
    public int enemyHealth = 100;
    protected GameObject targetTree;

    private DamageIndicator damageIndicator;

    // Referência ao ThrazEngine
    public ThrazEngine thrazEngine;
    public ThrazEngine.SummonableEnemy summonableEnemyType;

    // Variáveis para status
    protected StatusEffect currentEffect = StatusEffect.None;
    protected int effectTurnsRemaining = 0;

    // Efeito de queimadura
    public GameObject fireEffectPrefab; // Prefab do efeito de fogo
    private GameObject activeFireEffect; // Instância do efeito de fogo

    // Efeito de atordoamento
    private Renderer[] enemyRenderers;      // Array de Renderers do inimigo
    private Color[] originalColors;         // Array de cores originais
    private bool isStunned = false;         // Flag para verificar se está atordoado

    public bool IsDestroyed { get; private set; } = false;

    public void Start()
    {
        if (damageIndicator == null)
        {
            damageIndicator = gameObject.AddComponent<DamageIndicator>();
        }

        if (thrazEngine == null)
        {
            thrazEngine = FindObjectOfType<ThrazEngine>();
        }

        // Inicializa os Renderers e armazena as cores originais
        enemyRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[enemyRenderers.Length];

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            // Cria uma instância do material para evitar alterar materiais compartilhados
            enemyRenderers[i].material = new Material(enemyRenderers[i].material);
            originalColors[i] = enemyRenderers[i].material.color;
        }
    }

    protected virtual void DestroyEnemy()
    {
        Debug.Log(gameObject.name + " foi destruído!");

        IsDestroyed = true;

        // Usando a referência ao ThrazEngine
        if (thrazEngine != null && summonableEnemyType != null)
        {
            thrazEngine.OnEnemyDestroyed(summonableEnemyType);
        }

        // Remove quaisquer efeitos ativos antes de destruir o inimigo
        ClearStatusEffect();

        // Destrói o GameObject após um pequeno delay para garantir que todas as operações pendentes sejam concluídas
        Destroy(gameObject, 0.1f);
    }

    public virtual void InitializeGrid(Vector3[,] grid, float cellSize)
    {
        gridPositions = grid;
        this.cellSize = cellSize;
        movementStep = cellSize;
    }

    public virtual void StartTurn(List<GameObject> trees)
    {
        // Aplica o efeito atual
        ApplyCurrentEffect();

        // Diminui o contador de turnos do efeito
        if (effectTurnsRemaining > 0)
        {
            effectTurnsRemaining--;
            if (effectTurnsRemaining == 0)
            {
                ClearStatusEffect();
            }
        }

        // Se o inimigo estiver atordoado, não faz nada neste turno
        if (isStunned)
        {
            Debug.Log(gameObject.name + " está atordoado e perde o turno.");
            return;
        }

        // Lógica padrão de movimento e ataque do inimigo
        // (Implementar conforme a lógica específica do seu inimigo)
    }

    protected GameObject FindClosestTree(List<GameObject> trees)
    {
        GameObject closestTree = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject tree in trees)
        {
            if (tree != null && tree.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, tree.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTree = tree;
                }
            }
        }

        return closestTree;
    }

    public virtual void TakeDamage(int damageAmount)
    {
        enemyHealth -= damageAmount;
        Debug.Log(gameObject.name + " tomou " + damageAmount + " de dano. Vida restante: " + enemyHealth);

        // Aciona o efeito de piscar
        if (damageIndicator != null)
        {
            damageIndicator.FlashDamage();
        }

        if (enemyHealth <= 0)
        {
            DestroyEnemy();
        }
    }

    public virtual void ApplyStatusEffect(StatusEffect effect, int duration)
    {
        Debug.Log(gameObject.name + " está agora sob o efeito: " + effect + " por " + duration + " turnos.");

        // Se já estiver sob o mesmo efeito, reinicia a duração
        if (currentEffect == effect)
        {
            effectTurnsRemaining = duration;
            return;
        }

        // Se um efeito diferente está ativo, limpa o efeito anterior
        if (currentEffect != StatusEffect.None)
        {
            ClearStatusEffect();
        }

        currentEffect = effect;
        effectTurnsRemaining = duration;

        switch (effect)
        {
            case StatusEffect.Burning:
                // Inicia o efeito de queimadura
                StartBurningEffect();
                break;

            case StatusEffect.Stunned:
                // Inicia o efeito de atordoamento
                StartStunnedEffect();
                break;

                // Adicione outros casos para diferentes efeitos de status, se necessário
        }
    }

    protected void ApplyCurrentEffect()
    {
        switch (currentEffect)
        {
            case StatusEffect.Burning:
                // O dano é aplicado no início de cada turno
                ApplyBurningDamage();
                break;

            case StatusEffect.Stunned:
                // O atordoamento é tratado pela flag isStunned
                break;

            // Adicione outros casos para diferentes efeitos de status, se necessário

            default:
                break;
        }
    }

    // Métodos para o efeito de queimadura
    private void StartBurningEffect()
    {
        // Instancia o efeito de fogo se ainda não estiver ativo
        if (fireEffectPrefab != null && activeFireEffect == null)
        {
            activeFireEffect = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity, transform);
            // Ajusta a posição do efeito, se necessário
            activeFireEffect.transform.localPosition = Vector3.zero;

            // Ajusta a rotação no eixo X para -90 graus
            activeFireEffect.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        }
    }

    private void ApplyBurningDamage()
    {
        int burnDamage = 10; // Dano causado a cada turno
        TakeDamage(burnDamage);
        Debug.Log(gameObject.name + " sofre " + burnDamage + " de dano devido ao efeito de queimadura.");
    }

    // Métodos para o efeito de atordoamento
    private void StartStunnedEffect()
    {
        if (!isStunned)
        {
            isStunned = true;

            // Altera a cor de todos os Renderers do inimigo para marrom
            if (enemyRenderers != null)
            {
                foreach (var renderer in enemyRenderers)
                {
                    renderer.material.color = new Color(0.6f, 0.3f, 0f); // Cor marrom
                }
            }
        }
    }

    // Método para limpar o efeito de status atual
    private void ClearStatusEffect()
    {
        Debug.Log(gameObject.name + " não está mais afetado por nenhum efeito.");

        switch (currentEffect)
        {
            case StatusEffect.Burning:
                // Remove o efeito de fogo
                if (activeFireEffect != null)
                {
                    Destroy(activeFireEffect);
                    activeFireEffect = null;
                }
                break;

            case StatusEffect.Stunned:
                // Reverte as cores originais
                if (enemyRenderers != null && originalColors != null)
                {
                    for (int i = 0; i < enemyRenderers.Length; i++)
                    {
                        enemyRenderers[i].material.color = originalColors[i];
                    }
                }
                isStunned = false;
                break;

                // Adicione outros casos para limpar efeitos adicionais, se necessário
        }

        currentEffect = StatusEffect.None;
        effectTurnsRemaining = 0;
    }

    protected void ApplyKnockbackEffect()
    {
        // Implementação do efeito de knockback (empurrão)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 knockbackDirection = -transform.forward; // Ajuste conforme necessário
            float knockbackForce = 5f; // Ajuste conforme necessário
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            Debug.Log(gameObject.name + " foi empurrado para trás devido ao efeito de knockback.");
        }
        else
        {
            Debug.LogWarning("Rigidbody não encontrado em " + gameObject.name + ". O efeito de knockback não pode ser aplicado.");
        }

        // O efeito de knockback geralmente é instantâneo, então podemos remover o efeito imediatamente
        currentEffect = StatusEffect.None;
        effectTurnsRemaining = 0;
    }
}
