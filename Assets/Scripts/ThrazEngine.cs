using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ThrazEngine : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public bool isInvincible = false;

    private DamageIndicator damageIndicator;

    // Drones de Proteção
    [System.Serializable]
    public class ProtectionDroneConfig
    {
        public bool enabled = false;
        public GameObject dronePrefab;
        public int numberOfDrones = 3;
        public int dronesToSummonPerTurn = 1;
        public int droneCooldown = 2;
        public int droneMinimumRound = 1;
    }
    public ProtectionDroneConfig protectionDroneConfig;
    private List<ProtectionDrone> activeDrones = new List<ProtectionDrone>();
    private int lastDroneSummonTurn = -1;

    // Fumaça Tóxica
    [System.Serializable]
    public class ToxicSmokeConfig
    {
        public bool enabled = false;
        public GameObject smokePrefab;
        public GameObject mapArea;
        public int toxicSmokeCooldown = 2;
        public int toxicSmokeMinimumRound = 1;
    }
    public ToxicSmokeConfig toxicSmokeConfig;
    [HideInInspector]
    public GameObject activeSmoke;
    private int lastSmokeReleaseTurn = -1;

    // Invocar Inimigos
    [System.Serializable]
    public class SummonableEnemiesConfig
    {
        public bool enabled = true;
        public List<SummonableEnemy> summonableEnemies;
    }
    public SummonableEnemiesConfig summonableEnemiesConfig;

    public GridManager gridManager;
    public TurnManager turnManager;

    private int currentRound = 0;

    public TextMeshProUGUI healthText;
    public Slider healthBar;

    public enum AbilityType
    {
        ReleaseToxicSmoke,
        SummonProtectionDrones,
        SummonEnemy
    }

    [System.Serializable]
    public class AbilityConfig
    {
        public AbilityType abilityType;
        public int priority;
    }

    public List<AbilityConfig> abilitiesConfig;

    
    void Start()
    {

        currentHealth = maxHealth;
        UpdateHealthUI();

        damageIndicator = GetComponent<DamageIndicator>() ?? gameObject.AddComponent<DamageIndicator>();

        lastDroneSummonTurn = -1;
        lastSmokeReleaseTurn = -1;

        foreach (var enemy in summonableEnemiesConfig.summonableEnemies)
        {
            enemy.Initialize();
        }

        UpdateCurrentCountsWithExistingEnemies();

        abilitiesConfig.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    void UpdateCurrentCountsWithExistingEnemies()
    {
        EnemyBase[] existingEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemyBase in existingEnemies)
        {
            if (enemyBase.thrazEngine == null)
            {
                enemyBase.thrazEngine = this;
            }

            if (enemyBase.summonableEnemyType != null)
            {
                foreach (var summonableEnemy in summonableEnemiesConfig.summonableEnemies)
                {
                    if (summonableEnemy.enemyPrefab == enemyBase.summonableEnemyType.enemyPrefab)
                    {
                        summonableEnemy.currentCount++;
                        break;
                    }
                }
            }
            else
            {
                Debug.LogWarning("EnemyBase encontrado sem summonableEnemyType definido.");
            }
        }
    }

    public void StartTurn()
    {
        Debug.Log("ThrazEngine está agindo no turno " + currentRound);

        ExecuteAbilities();

        currentRound++;
    }

    void ExecuteAbilities()
    {
        foreach (var abilityConfig in abilitiesConfig)
        {
            switch (abilityConfig.abilityType)
            {
                case AbilityType.ReleaseToxicSmoke:
                    if (toxicSmokeConfig.enabled && CanReleaseToxicSmoke())
                    {
                        ReleaseToxicSmoke();
                        return;
                    }
                    break;

                case AbilityType.SummonProtectionDrones:
                    if (protectionDroneConfig.enabled && CanSummonProtectionDrones())
                    {
                        SummonProtectionDrones();
                        return;
                    }
                    break;

                case AbilityType.SummonEnemy:
                    if (summonableEnemiesConfig.enabled && CanSummonEnemyAbility())
                    {
                        SummonEnemyAbility();
                        return;
                    }
                    break;
            }
        }
    }

    // Fumaça Tóxica
    bool CanReleaseToxicSmoke()
    {
        if (activeSmoke != null)
            return false;

        if (currentRound < toxicSmokeConfig.toxicSmokeMinimumRound)
            return false;

        if (lastSmokeReleaseTurn != -1 && currentRound < lastSmokeReleaseTurn + toxicSmokeConfig.toxicSmokeCooldown + 1)
            return false;

        return true;
    }

    void ReleaseToxicSmoke()
    {
        activeSmoke = Instantiate(toxicSmokeConfig.smokePrefab, toxicSmokeConfig.mapArea.transform.position, Quaternion.identity);
        activeSmoke.transform.SetParent(toxicSmokeConfig.mapArea.transform, false);
        activeSmoke.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        // Define a posição local em Y = 2.5
        activeSmoke.transform.localPosition = new Vector3(0f, 2.5f, 0f);

        // Aplica a escala de acordo com GlobalPlacementData, mas reduz um pouco (por exemplo para metade)
        float reductionFactor = 0.05f; // Ajuste conforme necessário
        Vector3 adjustedScale = GlobalPlacementData.scale * reductionFactor;
        activeSmoke.transform.localScale = adjustedScale;

        Debug.Log("Thraz liberou fumaça tóxica.");
    }

    public void RemoveToxicSmoke()
    {
        if (activeSmoke != null)
        {
            Destroy(activeSmoke);
            activeSmoke = null;
            lastSmokeReleaseTurn = currentRound;
            Debug.Log("Toxic Smoke foi removida pelo orbe de Ar.");
        }
    }

    // Drones de Proteção
    bool CanSummonProtectionDrones()
    {
        if (currentRound < protectionDroneConfig.droneMinimumRound)
            return false;

        if (lastDroneSummonTurn != -1 && currentRound < lastDroneSummonTurn + protectionDroneConfig.droneCooldown + 1)
            return false;

        if (activeDrones.Count >= protectionDroneConfig.numberOfDrones)
            return false;

        return true;
    }

    void SummonProtectionDrones()
    {
        int dronesToSummon = Mathf.Min(protectionDroneConfig.dronesToSummonPerTurn, protectionDroneConfig.numberOfDrones - activeDrones.Count);

        for (int i = 0; i < dronesToSummon; i++)
        {
            float angle = Random.Range(0f, 360f);

            GameObject droneObject = Instantiate(protectionDroneConfig.dronePrefab, transform.position, Quaternion.identity);
            
            // Aplica a escala de acordo com GlobalPlacementData, mas reduz um pouco (por exemplo para metade)
            float reductionFactor = 0.03f; // Ajuste conforme necessário
            Vector3 adjustedScale = GlobalPlacementData.scale * reductionFactor;
            droneObject.transform.localScale = adjustedScale;

            //droneObject.transform.localScale = droneObject.transform.localScale * GlobalPlacementData.scale;

            ProtectionDrone drone = droneObject.GetComponent<ProtectionDrone>();
            if (drone != null)
            {
                drone.Initialize(this, angle);
                activeDrones.Add(drone);
            }
            else
            {
                Debug.LogError("ProtectionDrone script não encontrado no prefab do drone.");
            }
        }
        lastDroneSummonTurn = currentRound;

        Debug.Log("Thraz invocou drones de proteção.");
    }

    public void RemoveDrone(ProtectionDrone drone)
    {
        if (activeDrones.Contains(drone))
        {
            activeDrones.Remove(drone);
        }

        if (activeDrones.Count == 0)
        {
            Debug.Log("Todos os drones foram destruídos. Thraz está vulnerável.");
        }
    }

    // Invocação de Inimigos
    [System.Serializable]
    public class SummonableEnemy
    {
        public GameObject enemyPrefab;
        [HideInInspector]
        public int currentCount = 0;
        public int enemySpawnCount = 1;
        public int enemySpawnLimit = 3;
        public int enemySpawnCooldown = 2;
        [HideInInspector]
        public int lastSummonTurn = -1;
        public int enemySpawnMinimumRound = 0;

        public void Initialize()
        {
            currentCount = 0;
            lastSummonTurn = -1;
        }
    }

    bool CanSummonEnemyAbility()
    {
        foreach (var enemy in summonableEnemiesConfig.summonableEnemies)
        {
            if (CanSummonEnemy(enemy))
                return true;
        }
        return false;
    }

    void SummonEnemyAbility()
    {
        foreach (var enemy in summonableEnemiesConfig.summonableEnemies)
        {
            if (CanSummonEnemy(enemy))
            {
                int enemiesToSummon = Mathf.Min(enemy.enemySpawnCount, enemy.enemySpawnLimit - enemy.currentCount);
                for (int i = 0; i < enemiesToSummon; i++)
                {
                    SummonEnemy(enemy);
                }
                enemy.lastSummonTurn = currentRound;
                break;
            }
        }
    }

    bool CanSummonEnemy(SummonableEnemy enemy)
    {
        if (enemy.currentCount >= enemy.enemySpawnLimit)
            return false;

        if (currentRound < enemy.enemySpawnMinimumRound)
            return false;

        if (enemy.lastSummonTurn != -1 && currentRound < enemy.lastSummonTurn + enemy.enemySpawnCooldown + 1)
            return false;

        return true;
    }

    void SummonEnemy(SummonableEnemy enemyToSummon)
    {
        Vector3 spawnPosition = gridManager.GetRandomGridPosition();

        GameObject newEnemy = Instantiate(enemyToSummon.enemyPrefab, spawnPosition, Quaternion.identity);
        newEnemy.transform.localScale *= 0.1f;
        newEnemy.transform.localScale = new Vector3(newEnemy.transform.localScale.x * GlobalPlacementData.scale.x, newEnemy.transform.localScale.y * GlobalPlacementData.scale.y, newEnemy.transform.localScale.z * GlobalPlacementData.scale.z); // Apply placement scale compensation.
        newEnemy.transform.SetParent(gridManager.mapArea.transform);

        EnemyBase enemyEngine = newEnemy.GetComponent<EnemyBase>();
        if (enemyEngine != null)
        {
            enemyEngine.InitializeGrid(gridManager.grid, gridManager.cellSize);
            enemyEngine.thrazEngine = this;
            enemyEngine.summonableEnemyType = enemyToSummon;

            gridManager.enemies.Add(enemyEngine);
        }
        else
        {
            Debug.LogError("EnemyBase não foi encontrado no prefab!");
        }

        turnManager.RegisterEnemy(enemyEngine);

        enemyToSummon.currentCount++;
    }

    public void OnEnemyDestroyed(SummonableEnemy enemyType)
    {
        if (enemyType.currentCount > 0)
        {
            enemyType.currentCount--;
        }
    }

    // Receber Dano
    public void TakeDamage(int damage)
    {
        Debug.Log("ThrazEngine.TakeDamage chamado com dano: " + damage);

        if (isInvincible)
        {
            Debug.Log("Thraz é invencível e não recebe dano.");
            return;
        }

        if (protectionDroneConfig.enabled && activeDrones.Count > 0)
        {
            Debug.Log("Drone intercepta o dano.");
            ProtectionDrone drone = activeDrones[0];
            drone.TakeDamage(damage);
            return;
        }

        currentHealth -= damage;
        Debug.Log("Thraz recebeu " + damage + " de dano. Vida restante: " + currentHealth);

        if (damageIndicator != null)
        {
            damageIndicator.FlashDamage();
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("THRAZ MORREU RAPAZ");
        Destroy(gameObject);
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Vida do Thraz: " + currentHealth + "/" + maxHealth;
        }

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }
}
