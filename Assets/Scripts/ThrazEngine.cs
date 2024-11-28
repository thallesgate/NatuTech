using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ThrazEngine : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public bool isInvincible = false;

    // Drones de Proteção
    [System.Serializable]
    public class ProtectionDroneConfig
    {
        public bool enabled = false;
        public GameObject dronePrefab;
        public int numberOfDrones = 3;
        public int dronesToSummonPerTurn = 1;
        public int droneCooldown = 3;
        public int droneMinimumRound = 2;
    }
    public ProtectionDroneConfig protectionDroneConfig;
    private List<ProtectionDrone> activeDrones = new List<ProtectionDrone>();
    private int turnsSinceLastDroneUse = 0;

    // Fumaça Tóxica
    [System.Serializable]
    public class ToxicSmokeConfig
    {
        public bool enabled = false;
        public GameObject smokePrefab; // Referencie o ToxicSmokePrefab aqui
        public GameObject mapArea;
        public int toxicSmokeCooldown = 3;
        public int toxicSmokeMinimumRound = 2;
    }
    public ToxicSmokeConfig toxicSmokeConfig;
    [HideInInspector]
    public GameObject activeSmoke;
    private int turnsSinceLastSmoke = 0;

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

    private bool isFirstTurn = true;
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

        turnsSinceLastDroneUse = protectionDroneConfig.droneCooldown;
        turnsSinceLastSmoke = toxicSmokeConfig.toxicSmokeCooldown;

        foreach (var enemy in summonableEnemiesConfig.summonableEnemies)
        {
            enemy.Initialize();
        }

        UpdateCurrentCountsWithExistingEnemies();

        abilitiesConfig.Sort((a, b) => a.priority.CompareTo(b.priority));

        StartTurn();
    }

    void UpdateCurrentCountsWithExistingEnemies()
    {
        EnemyBase[] existingEnemies = FindObjectsOfType<EnemyBase>();
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

        if (!isFirstTurn)
        {
            UpdateCooldowns();
        }

        ExecuteAbilities();

        isFirstTurn = false;
        currentRound++;
    }

    void UpdateCooldowns()
    {
        if (turnsSinceLastDroneUse < protectionDroneConfig.droneCooldown)
        {
            turnsSinceLastDroneUse++;
        }

        if (turnsSinceLastSmoke < toxicSmokeConfig.toxicSmokeCooldown)
        {
            turnsSinceLastSmoke++;
        }

        UpdateEnemyCooldowns();
    }

    void UpdateEnemyCooldowns()
    {
        foreach (var enemy in summonableEnemiesConfig.summonableEnemies)
        {
            if (enemy.turnsSinceLastSummon < enemy.enemySpawnCooldown)
            {
                enemy.turnsSinceLastSummon++;
            }
        }
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

        if (turnsSinceLastSmoke < toxicSmokeConfig.toxicSmokeCooldown)
            return false;

        if (currentRound < toxicSmokeConfig.toxicSmokeMinimumRound)
            return false;

        return true;
    }

    void ReleaseToxicSmoke()
    {
        // Instancia a fumaça sem rotação adicional
        activeSmoke = Instantiate(toxicSmokeConfig.smokePrefab, toxicSmokeConfig.mapArea.transform.position, Quaternion.identity);
        activeSmoke.transform.SetParent(toxicSmokeConfig.mapArea.transform, false);

        // Ajusta a rotação local da fumaça
        activeSmoke.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        // Reseta a posição local
        activeSmoke.transform.localPosition = Vector3.zero;

        // Ajusta a escala local, se necessário
        activeSmoke.transform.localScale = Vector3.one; // Ajuste conforme necessário

        turnsSinceLastSmoke = 0;
        Debug.Log("Thraz liberou fumaça tóxica.");
    }


    public void RemoveToxicSmoke()
    {
        if (activeSmoke != null)
        {
            Destroy(activeSmoke);
            activeSmoke = null;
            Debug.Log("Toxic Smoke foi removida pelo orbe de Ar.");
        }
    }

    // Drones de Proteção
    bool CanSummonProtectionDrones()
    {
        if (turnsSinceLastDroneUse < protectionDroneConfig.droneCooldown)
            return false;

        if (currentRound < protectionDroneConfig.droneMinimumRound)
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

        turnsSinceLastDroneUse = 0;
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
        public int turnsSinceLastSummon = 0;
        public int enemySpawnMinimumRound = 1;

        public void Initialize()
        {
            currentCount = 0;
            turnsSinceLastSummon = enemySpawnCooldown;
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
                enemy.turnsSinceLastSummon = 0;
                break;
            }
        }
    }

    bool CanSummonEnemy(SummonableEnemy enemy)
    {
        if (enemy.currentCount >= enemy.enemySpawnLimit)
            return false;

        if (enemy.turnsSinceLastSummon < enemy.enemySpawnCooldown)
            return false;

        if (currentRound < enemy.enemySpawnMinimumRound)
            return false;

        return true;
    }

    void SummonEnemy(SummonableEnemy enemyToSummon)
    {
        Vector3 spawnPosition = gridManager.GetRandomGridPosition();

        GameObject newEnemy = Instantiate(enemyToSummon.enemyPrefab, spawnPosition, Quaternion.identity);
        newEnemy.transform.localScale *= 0.1f;
        newEnemy.transform.SetParent(gridManager.mapArea.transform);

        EnemyBase enemyEngine = newEnemy.GetComponent<EnemyBase>();
        if (enemyEngine != null)
        {
            enemyEngine.InitializeGrid(gridManager.grid, gridManager.cellSize);
            enemyEngine.thrazEngine = this;
            enemyEngine.summonableEnemyType = enemyToSummon;
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
            // O drone intercepta o dano
            ProtectionDrone drone = activeDrones[0];
            drone.TakeDamage(damage);
            return;
        }

        currentHealth -= damage;
        Debug.Log("Thraz recebeu " + damage + " de dano. Vida restante: " + currentHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Thraz foi derrotado!");

        if (turnManager != null)
        {
            turnManager.GameOver("Thraz foi derrotado!");
        }
        else
        {
            Debug.LogError("TurnManager não encontrado. Não é possível terminar o jogo.");
        }

        gameObject.SetActive(false);
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
