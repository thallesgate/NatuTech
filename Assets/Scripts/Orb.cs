using UnityEngine;
using System;

public class Orb : MonoBehaviour
{
    public OrbType orbType;
    public int damage;
    public StatusEffect statusEffect = StatusEffect.None;
    public int effectDuration = 0;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float arcHeight;
    private float travelTime;
    private float timer = 0f;

    private Action onOrbFinished;

    public GameObject particleEffectPrefab;

    private bool isMoving = false;

    // Adicione um SphereCollider e marque como Trigger no pr�prio prefab do orbe.

    public void Initialize(OrbType type, int dmg, Vector3 startPos, Vector3 endPos, float arcH, float duration, StatusEffect effect, int effectTurns, Action callback)
    {
        orbType = type;
        damage = dmg;
        startPosition = startPos;
        endPosition = endPos;
        arcHeight = arcH;
        travelTime = duration;
        statusEffect = effect;
        effectDuration = effectTurns;
        onOrbFinished = callback;

        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            timer += Time.deltaTime;
            float t = timer / travelTime;

            if (t >= 1f)
            {
                OnOrbEnd();
                return;
            }

            Vector3 currentPos = CalculateBezierPoint(t, startPosition, (startPosition + endPosition) / 2 + Vector3.up * arcHeight, endPosition);
            transform.position = currentPos;
        }
    }

    private void OnOrbEnd()
    {
        isMoving = false;

        // Instanciar o efeito de part�culas no ponto de impacto
        if (particleEffectPrefab != null)
        {
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        }

        // Verifica se o orbe � do tipo Ar
        if (orbType == OrbType.Air)
        {
            // Encontra a inst�ncia do ThrazEngine
            ThrazEngine thrazEngine = FindObjectOfType<ThrazEngine>();
            if (thrazEngine != null)
            {
                thrazEngine.RemoveToxicSmoke();
                Debug.Log("Orbe de Ar lan�ado: Toxic Smoke removida.");
            }
        }

        // Chama o callback para sinalizar que o orbe terminou sua a��o
        onOrbFinished?.Invoke();

        // Destroi o orbe
        Destroy(gameObject);
    }

    // M�todo chamado quando o orbe colide com outro objeto
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se colidiu com um drone de prote��o
        ProtectionDrone drone = other.GetComponent<ProtectionDrone>();
        if (drone != null)
        {
            Debug.Log("Orbe colidiu com: " + other.gameObject.name);

            drone.TakeDamage(damage);
            Debug.Log("Drone de prote��o foi atingido pelo orbe.");

            // Efeito de part�culas e destrui��o do orbe
            if (particleEffectPrefab != null)
            {
                Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            }

            onOrbFinished?.Invoke();
            Destroy(gameObject);
            return;
        }

        // Verifica se colidiu com Thraz
        ThrazEngine thraz = other.GetComponent<ThrazEngine>();
        if (thraz != null)
        {
            thraz.TakeDamage(damage);
            Debug.Log("Thraz foi atingido pelo orbe.");

            // Efeito de part�culas e destrui��o do orbe
            if (particleEffectPrefab != null)
            {
                Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            }

            onOrbFinished?.Invoke();
            Destroy(gameObject);
            return;
        }

        // Verifica se colidiu com um inimigo
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            enemy.ApplyStatusEffect(statusEffect, effectDuration);
            Debug.Log("Inimigo atingido: " + enemy.gameObject.name);

            // Efeito de part�culas e destrui��o do orbe
            if (particleEffectPrefab != null)
            {
                Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            }

            onOrbFinished?.Invoke();
            Destroy(gameObject);
            return;
        }
    }


    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // C�lculo da curva de B�zier
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;        // (1 - t)^2 * p0
        p += 2 * u * t * p1;        // 2 * (1 - t) * t * p1
        p += tt * p2;               // t^2 * p2

        return p;
    }
}
