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

    // Refer�ncia ao ThrazEngine
    public ThrazEngine thrazEngine;
    public ThrazEngine.SummonableEnemy summonableEnemyType;

    // Vari�veis para status
    protected StatusEffect currentEffect = StatusEffect.None;
    protected int effectTurnsRemaining = 0;

    // Efeito de queimadura
    public GameObject fireEffectPrefab; // Prefab do efeito de fogo
    private GameObject activeFireEffect; // Inst�ncia do efeito de fogo

    // Efeito de atordoamento
    private Renderer[] enemyRenderers;      // Array de Renderers do inimigo
    private Color[] originalColors;         // Array de cores originais
    private bool isStunned = false;         // Flag para verificar se est� atordoado

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
            // Cria uma inst�ncia do material para evitar alterar materiais compartilhados
            enemyRenderers[i].material = new Material(enemyRenderers[i].material);
            originalColors[i] = enemyRenderers[i].material.color;
        }
    }

    protected virtual void DestroyEnemy()
    {
        Debug.Log(gameObject.name + " foi destru�do!");

        IsDestroyed = true;

        // Usando a refer�ncia ao ThrazEngine
        if (thrazEngine != null && summonableEnemyType != null)
        {
            thrazEngine.OnEnemyDestroyed(summonableEnemyType);
        }

        // Remove quaisquer efeitos ativos antes de destruir o inimigo
        ClearStatusEffect();

        // Destr�i o GameObject ap�s um pequeno delay para garantir que todas as opera��es pendentes sejam conclu�das
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

        // Se o inimigo estiver atordoado, n�o faz nada neste turno
        if (isStunned)
        {
            Debug.Log(gameObject.name + " est� atordoado e perde o turno.");
            return;
        }

        // L�gica padr�o de movimento e ataque do inimigo
        // (Implementar conforme a l�gica espec�fica do seu inimigo)
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
        Debug.Log(gameObject.name + " est� agora sob o efeito: " + effect + " por " + duration + " turnos.");

        // Se j� estiver sob o mesmo efeito, reinicia a dura��o
        if (currentEffect == effect)
        {
            effectTurnsRemaining = duration;
            return;
        }

        // Se um efeito diferente est� ativo, limpa o efeito anterior
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

                // Adicione outros casos para diferentes efeitos de status, se necess�rio
        }
    }

    protected void ApplyCurrentEffect()
    {
        switch (currentEffect)
        {
            case StatusEffect.Burning:
                // O dano � aplicado no in�cio de cada turno
                ApplyBurningDamage();
                break;

            case StatusEffect.Stunned:
                // O atordoamento � tratado pela flag isStunned
                break;

            // Adicione outros casos para diferentes efeitos de status, se necess�rio

            default:
                break;
        }
    }

    // M�todos para o efeito de queimadura
    private void StartBurningEffect()
    {
        // Instancia o efeito de fogo se ainda n�o estiver ativo
        if (fireEffectPrefab != null && activeFireEffect == null)
        {
            activeFireEffect = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity, transform);
            // Ajusta a posi��o do efeito, se necess�rio
            activeFireEffect.transform.localPosition = Vector3.zero;

            // Ajusta a rota��o no eixo X para -90 graus
            activeFireEffect.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        }
    }

    private void ApplyBurningDamage()
    {
        int burnDamage = 10; // Dano causado a cada turno
        TakeDamage(burnDamage);
        Debug.Log(gameObject.name + " sofre " + burnDamage + " de dano devido ao efeito de queimadura.");
    }

    // M�todos para o efeito de atordoamento
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

    // M�todo para limpar o efeito de status atual
    private void ClearStatusEffect()
    {
        Debug.Log(gameObject.name + " n�o est� mais afetado por nenhum efeito.");

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

                // Adicione outros casos para limpar efeitos adicionais, se necess�rio
        }

        currentEffect = StatusEffect.None;
        effectTurnsRemaining = 0;
    }

    protected void ApplyKnockbackEffect()
    {
        // Implementa��o do efeito de knockback (empurr�o)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 knockbackDirection = -transform.forward; // Ajuste conforme necess�rio
            float knockbackForce = 5f; // Ajuste conforme necess�rio
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            Debug.Log(gameObject.name + " foi empurrado para tr�s devido ao efeito de knockback.");
        }
        else
        {
            Debug.LogWarning("Rigidbody n�o encontrado em " + gameObject.name + ". O efeito de knockback n�o pode ser aplicado.");
        }

        // O efeito de knockback geralmente � instant�neo, ent�o podemos remover o efeito imediatamente
        currentEffect = StatusEffect.None;
        effectTurnsRemaining = 0;
    }
}
