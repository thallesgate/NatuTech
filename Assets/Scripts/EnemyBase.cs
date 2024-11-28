using UnityEngine;
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

    public ThrazEngine thrazEngine;
    public ThrazEngine.SummonableEnemy summonableEnemyType;

    // Vari�veis para status
    protected StatusEffect currentEffect = StatusEffect.None;
    protected int effectTurnsRemaining = 0;

    public void Start()
    {
        if (thrazEngine == null)
        {
            thrazEngine = FindObjectOfType<ThrazEngine>();
        }
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
                currentEffect = StatusEffect.None;
                Debug.Log(gameObject.name + " n�o est� mais afetado por nenhum efeito.");
            }
        }

        // Se o inimigo estiver atordoado, n�o faz nada neste turno
        if (currentEffect == StatusEffect.Stunned)
        {
            Debug.Log(gameObject.name + " est� atordoado e perde o turno.");
            return;
        }

        // Implementa��o do comportamento padr�o do inimigo
        // (Mover-se em dire��o � �rvore, atacar, etc.)
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

        if (enemyHealth <= 0)
        {
            DestroyEnemy();
        }
    }

    public virtual void ApplyStatusEffect(StatusEffect effect, int duration)
    {
        currentEffect = effect;
        effectTurnsRemaining = duration;
        Debug.Log(gameObject.name + " est� agora sob o efeito: " + effect + " por " + duration + " turnos.");
    }

    protected void ApplyCurrentEffect()
    {
        switch (currentEffect)
        {
            case StatusEffect.Burning:
                ApplyBurningEffect();
                break;
            case StatusEffect.Slowed:
                // A redu��o de velocidade pode ser aplicada aqui
                break;
            case StatusEffect.Stunned:
                // O atordoamento � tratado no StartTurn
                break;
            case StatusEffect.Knockback:
                ApplyKnockbackEffect();
                break;
            default:
                break;
        }
    }

    protected void ApplyBurningEffect()
    {
        int burnDamage = 3; // Dano causado a cada turno (ajuste conforme necess�rio)
        TakeDamage(burnDamage);
        Debug.Log(gameObject.name + " sofre " + burnDamage + " de dano devido ao efeito de queimadura.");
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

    protected virtual void DestroyEnemy()
    {
        Debug.Log(gameObject.name + " foi destru�do!");

        TurnManager turnManager = FindObjectOfType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.RemoveEnemy(this);
        }

        // Usando a refer�ncia ao ThrazEngine
        if (thrazEngine != null && summonableEnemyType != null)
        {
            thrazEngine.OnEnemyDestroyed(summonableEnemyType);
        }

        Destroy(gameObject);
    }
}
