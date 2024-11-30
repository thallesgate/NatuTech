using System.Collections.Generic;
using UnityEngine;

public enum Phase
{
    Phase1,
    Phase2,
    Phase3,
    Phase4,
    Phase5
}

[System.Serializable]
public class PhaseSettings
{
    public Phase phase;

    [Header("Mundo")]
    public int treeHealth = 40;
    public int numberOfTrees = 6;

    [Header("Thraz")]
    public int thrazHealth = 100;

    [Header("Spawn de Escavadeiras")]
    public int excavatorMinSpawnTurn = 1;
    public int excavatorSpawnAmount = 1;
    public int excavatorMaxCount = 2;
    public int excavatorSpawnCooldown = 3;

    [Header("Fumaça Tóxica")]
    public int smokeCooldown = 0;
    public int smokeMinSpawnTurn = 0;

    [Header("Drones de Proteção")]
    public int droneMinSpawnTurn = 0;
    public int droneSpawnAmount = 0;
    public int droneMaxCount = 0;
    public int droneCooldown = 0;

    [Header("Jogador - Orbes")]
    public int fireOrbDamage = 20;
    public int fireOrbEffectDuration = 2;
    public int fireOrbEffectDamage = 10;

    public int earthOrbDamage = 15;
    public int earthOrbEffectDuration = 1;

    public int airOrbDamage = 10;

    public int waterOrbDamage = 15;

    // Construtor padrão
    public PhaseSettings(Phase phase)
    {
        this.phase = phase;
    }
}

public class DifficultyManager : MonoBehaviour
{
    public Phase currentPhase;

    [SerializeField]
    public List<PhaseSettings> phaseSettingsList = new List<PhaseSettings>();

    // Referências aos sistemas que precisam ser configurados
    public ThrazEngine thrazEngine;
    public OrbManager orbManager;
    public GridManager gridManager;

    // Valores constantes para todas as fases
    [Header("Valores Constantes")]
    public int excavatorHealth = 30;
    public int excavatorDamage = 10;
    public int smokeDamage = 5; // Dano fixo da fumaça tóxica

    void Awake()
    {
        InitializePhaseSettings();
        ApplySettingsForCurrentPhase();
    }

    void InitializePhaseSettings()
    {
        // Limpa a lista existente
        phaseSettingsList.Clear();

        // Fase 1
        PhaseSettings phase1 = new PhaseSettings(Phase.Phase1)
        {
            thrazHealth = 100,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 1,
            excavatorMaxCount = 2,
            excavatorSpawnCooldown = 3,
            droneMinSpawnTurn = 0,
            droneSpawnAmount = 0,
            droneMaxCount = 0,
            droneCooldown = 0,
            smokeMinSpawnTurn = 0,
            smokeCooldown = 0,
            fireOrbDamage = 20,
            fireOrbEffectDuration = 2,
            earthOrbDamage = 15,
            earthOrbEffectDuration = 1,
            airOrbDamage = 10,
            waterOrbDamage = 15
        };
        phaseSettingsList.Add(phase1);

        // Fase 2
        PhaseSettings phase2 = new PhaseSettings(Phase.Phase2)
        {
            thrazHealth = 100,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 1,
            excavatorMaxCount = 3,
            excavatorSpawnCooldown = 2,
            droneMinSpawnTurn = 3,
            droneSpawnAmount = 1,
            droneMaxCount = 2,
            droneCooldown = 2,
            smokeMinSpawnTurn = 0,
            smokeCooldown = 0,
            fireOrbDamage = 25,
            fireOrbEffectDuration = 3,
            earthOrbDamage = 15,
            earthOrbEffectDuration = 2,
            airOrbDamage = 15,
            waterOrbDamage = 20
        };
        phaseSettingsList.Add(phase2);

        // Fase 3
        PhaseSettings phase3 = new PhaseSettings(Phase.Phase3)
        {
            thrazHealth = 200,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 2,
            excavatorMaxCount = 4,
            excavatorSpawnCooldown = 1,
            droneMinSpawnTurn = 2,
            droneSpawnAmount = 2,
            droneMaxCount = 3,
            droneCooldown = 2,
            smokeMinSpawnTurn = 0,
            smokeCooldown = 0,
            fireOrbDamage = 25,
            fireOrbEffectDuration = 3,
            earthOrbDamage = 15,
            earthOrbEffectDuration = 2,
            airOrbDamage = 15,
            waterOrbDamage = 20
        };
        phaseSettingsList.Add(phase3);

        // Fase 4
        PhaseSettings phase4 = new PhaseSettings(Phase.Phase4)
        {
            thrazHealth = 250,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 3,
            excavatorMaxCount = 5,
            excavatorSpawnCooldown = 1,
            droneMinSpawnTurn = 2,
            droneSpawnAmount = 2,
            droneMaxCount = 4,
            droneCooldown = 1,
            smokeMinSpawnTurn = 3,
            smokeCooldown = 4,
            fireOrbDamage = 30,
            fireOrbEffectDuration = 4,
            earthOrbDamage = 20,
            earthOrbEffectDuration = 3,
            airOrbDamage = 20,
            waterOrbDamage = 25
        };
        phaseSettingsList.Add(phase4);

        // Fase 5
        PhaseSettings phase5 = new PhaseSettings(Phase.Phase5)
        {
            thrazHealth = 300,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 3,
            excavatorMaxCount = 6,
            excavatorSpawnCooldown = 1,
            droneMinSpawnTurn = 1,
            droneSpawnAmount = 3,
            droneMaxCount = 5,
            droneCooldown = 1,
            smokeMinSpawnTurn = 2,
            smokeCooldown = 3,
            fireOrbDamage = 30,
            fireOrbEffectDuration = 4,
            earthOrbDamage = 20,
            earthOrbEffectDuration = 3,
            airOrbDamage = 25,
            waterOrbDamage = 30
        };
        phaseSettingsList.Add(phase5);
    }

    public void SetPhase(Phase phase)
    {
        currentPhase = phase;
        ApplySettingsForCurrentPhase();
    }

    void ApplySettingsForCurrentPhase()
    {
        PhaseSettings settings = phaseSettingsList.Find(s => s.phase == currentPhase);
        if (settings != null)
        {
            // Configurar o Mundo
            if (gridManager != null)
            {
                gridManager.treeHealth = settings.treeHealth;
                gridManager.qtd_arvore = settings.numberOfTrees;
            }

            // Configurar o Thraz
            if (thrazEngine != null)
            {
                thrazEngine.maxHealth = settings.thrazHealth;

                // Configurar Spawn de Escavadeiras
                // Encontrar o SummonableEnemy correspondente às escavadeiras
                ThrazEngine.SummonableEnemy excavatorEnemy = thrazEngine.summonableEnemiesConfig.summonableEnemies.Find(e => e.enemyPrefab.name.Contains("Escavadeira"));
                if (excavatorEnemy != null)
                {
                    excavatorEnemy.enemySpawnCount = settings.excavatorSpawnAmount;
                    excavatorEnemy.enemySpawnLimit = settings.excavatorMaxCount;
                    excavatorEnemy.enemySpawnCooldown = settings.excavatorSpawnCooldown;
                    excavatorEnemy.enemySpawnMinimumRound = settings.excavatorMinSpawnTurn;

                }
                else
                {
                    Debug.LogWarning("Escavadeira não encontrada na lista de inimigos invocáveis.");
                }

                // Configurar Fumaça Tóxica
                thrazEngine.toxicSmokeConfig.enabled = settings.smokeMinSpawnTurn > 0;
                thrazEngine.toxicSmokeConfig.toxicSmokeCooldown = settings.smokeCooldown;
                thrazEngine.toxicSmokeConfig.toxicSmokeMinimumRound = settings.smokeMinSpawnTurn;

                // Configurar Drones de Proteção
                thrazEngine.protectionDroneConfig.enabled = settings.droneMaxCount > 0;
                thrazEngine.protectionDroneConfig.droneMinimumRound = settings.droneMinSpawnTurn;
                thrazEngine.protectionDroneConfig.dronesToSummonPerTurn = settings.droneSpawnAmount;
                thrazEngine.protectionDroneConfig.numberOfDrones = settings.droneMaxCount;
                thrazEngine.protectionDroneConfig.droneCooldown = settings.droneCooldown;
            }

            // Configurar Orbes do Jogador
            if (orbManager != null)
            {
                // Orbe de Fogo
                OrbSettings fireOrbSettings = orbManager.orbSettingsList.Find(o => o.orbType == OrbType.Fire);
                if (fireOrbSettings != null)
                {
                    fireOrbSettings.damage = settings.fireOrbDamage;
                    fireOrbSettings.effectDuration = settings.fireOrbEffectDuration;
                    fireOrbSettings.damage = settings.fireOrbEffectDamage;
                }

                // Orbe de Terra
                OrbSettings earthOrbSettings = orbManager.orbSettingsList.Find(o => o.orbType == OrbType.Earth);
                if (earthOrbSettings != null)
                {
                    earthOrbSettings.damage = settings.earthOrbDamage;
                    earthOrbSettings.effectDuration = settings.earthOrbEffectDuration;
                }

                // Orbe de Ar
                OrbSettings airOrbSettings = orbManager.orbSettingsList.Find(o => o.orbType == OrbType.Air);
                if (airOrbSettings != null)
                {
                    airOrbSettings.damage = settings.airOrbDamage;
                }

                // Orbe de Água
                OrbSettings waterOrbSettings = orbManager.orbSettingsList.Find(o => o.orbType == OrbType.Water);
                if (waterOrbSettings != null)
                {
                    waterOrbSettings.damage = settings.waterOrbDamage;
                }
            }

            Debug.Log("Configurações aplicadas para a " + currentPhase.ToString());
        }
        else
        {
            Debug.LogError("Configurações não encontradas para a fase: " + currentPhase.ToString());
        }
    }
}
