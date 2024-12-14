using UnityEngine;
using System;
using UnityEngine.WSA;

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

    // Sons
    private AudioController audioController;

    [SerializeField] private string HitFogo = "HitOrbFogo";
    [SerializeField] private string HitTerra = "HitOrbTerra";
    [SerializeField] private string HitAr = "HitOrbAr";
    [SerializeField] private string HitAgua = "HitOrbAgua";

    // Adicione um SphereCollider e marque como Trigger no próprio prefab do orbe.

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

    void Start()
    {
        audioController = FindFirstObjectByType<AudioController>();
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

        // Instanciar o efeito de partículas no ponto de impacto
        if (particleEffectPrefab != null)
        {
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        }

        // Verifica se o orbe é do tipo Ar
        if (orbType == OrbType.Air)
        {
            // Encontra a instância do ThrazEngine
            ThrazEngine thrazEngine = FindFirstObjectByType<ThrazEngine>();
            if (thrazEngine != null)
            {
                thrazEngine.RemoveToxicSmoke();
                Debug.Log("Orbe de Ar lançado: Toxic Smoke removida.");
            }
        }

        // Chama o callback para sinalizar que o orbe terminou sua ação
        onOrbFinished?.Invoke();

        // Destroi o orbe
        Destroy(gameObject,0.1f);
    }

    // Método chamado quando o orbe colide com outro objeto
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se colidiu com um drone de proteção
        ProtectionDrone drone = other.GetComponent<ProtectionDrone>();
        if (drone != null)
        {
            Debug.Log("Orbe colidiu com: " + other.gameObject.name);

            // Som
            PlaySoundHit(orbType);

            drone.TakeDamage(damage);
            Debug.Log("Drone de proteção foi atingido pelo orbe.");

            // Efeito de partículas e destruição do orbe
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

            // Som
            PlaySoundHit(orbType);

            // Efeito de partículas e destruição do orbe
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

            // Som
            PlaySoundHit(orbType);

            // Efeito de partículas e destruição do orbe
            if (particleEffectPrefab != null)
            {
                Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            }

            onOrbFinished?.Invoke();
            Destroy(gameObject);
            return;
        }
    }

    private void PlaySoundHit(OrbType orbType)
    {
        Debug.Log("Esse é o ORBTYPE: " + orbType);
        switch (orbType)
        {
            case OrbType.Fire:
                audioController.PlaySound(HitFogo);
                Debug.Log("SOM DE HIT Fogo");
                break;
            case OrbType.Water:
                audioController.PlaySound(HitAgua);
                Debug.Log("SOM DE HIT Water");
                break;
            case OrbType.Earth:
                audioController.PlaySound(HitTerra);
                Debug.Log("SOM DE HIT Earth");
                break;
            case OrbType.Air:
                audioController.PlaySound(HitAr);
                Debug.Log("SOM DE HIT Air");
                break;
            default:
                Debug.Log("OrbType não identificado!");
                break;
        }

    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // Cálculo da curva de Bézier
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;        // (1 - t)^2 * p0
        p += 2 * u * t * p1;        // 2 * (1 - t) * t * p1
        p += tt * p2;               // t^2 * p2

        return p;
    }
}
