using System.Collections.Generic;
using UnityEngine;

public enum Phase
{
    Phase1,
    Phase2,
    Phase3,
    Phase4,
    Phase5,
    Phase6,
    Phase7,
    Phase8,
    Phase9,
    Phase10
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
    public bool enableToxicSmoke = false;
    public int smokeCooldown = 0;
    public int smokeMinSpawnTurn = 0;

    [Header("Drones de Proteção")]
    public bool enableDrones = false;
    public int droneMinSpawnTurn = 0;
    public int droneSpawnAmount = 0;
    public int droneMaxCount = 0;
    public int droneCooldown = 0;

    [Header("Jogador - Orbes")]
    public int fireOrbDamage = 20;
    public int fireOrbEffectDuration = 2;

    public int earthOrbDamage = 15;
    public int earthOrbEffectDuration = 1;

    public int airOrbDamage = 10;

    public int waterOrbDamage = 15;

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

    public ThrazEngine thrazEngine;
    public OrbManager orbManager;
    public GridManager gridManager;

    [Header("Valores Constantes")]
    public int excavatorHealth = 30;
    public int excavatorDamage = 10;
    public int smokeDamage = 5;

    void Awake()
    {
        InitializePhaseSettings();
        ApplySettingsForCurrentPhase();
    }

    void InitializePhaseSettings()
    {
        phaseSettingsList.Clear();

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase1)
        {
            thrazHealth = 80,
            treeHealth = 50,
            numberOfTrees = 5,
            excavatorMinSpawnTurn = 2,
            excavatorSpawnAmount = 1,
            excavatorMaxCount = 2,
            excavatorSpawnCooldown = 3,
            enableToxicSmoke = false,
            enableDrones = false,
            fireOrbDamage = 25,
            fireOrbEffectDuration = 2,
            earthOrbDamage = 20,
            airOrbDamage = 15,
            waterOrbDamage = 20
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase2)
        {
            thrazHealth = 100,
            treeHealth = 55,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 2,
            excavatorSpawnAmount = 1,
            excavatorMaxCount = 3,
            excavatorSpawnCooldown = 3,
            enableToxicSmoke = false,
            enableDrones = false,
            fireOrbDamage = 27,
            fireOrbEffectDuration = 2,
            earthOrbDamage = 22,
            airOrbDamage = 17,
            waterOrbDamage = 22
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase3)
        {
            thrazHealth = 150,
            treeHealth = 65,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 2,
            excavatorMaxCount = 4,
            excavatorSpawnCooldown = 2,
            enableToxicSmoke = false,
            enableDrones = true,
            droneMinSpawnTurn = 3,
            droneSpawnAmount = 1,
            droneMaxCount = 2,
            droneCooldown = 3,
            fireOrbDamage = 30,
            fireOrbEffectDuration = 3,
            earthOrbDamage = 25,
            airOrbDamage = 20,
            waterOrbDamage = 25
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase4)
        {
            thrazHealth = 170,
            treeHealth = 70,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 2,
            excavatorMaxCount = 5,
            excavatorSpawnCooldown = 2,
            enableToxicSmoke = false,
            enableDrones = true,
            droneMinSpawnTurn = 3,
            droneSpawnAmount = 1,
            droneMaxCount = 2,
            droneCooldown = 3,
            fireOrbDamage = 32,
            fireOrbEffectDuration = 3,
            earthOrbDamage = 27,
            airOrbDamage = 22,
            waterOrbDamage = 27
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase5)
        {
            thrazHealth = 190,
            treeHealth = 80,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 2,
            excavatorMaxCount = 5,
            excavatorSpawnCooldown = 2,
            enableToxicSmoke = true,
            smokeCooldown = 3,
            smokeMinSpawnTurn = 4,
            enableDrones = true,
            droneMinSpawnTurn = 3,
            droneSpawnAmount = 1,
            droneMaxCount = 3,
            droneCooldown = 3,
            fireOrbDamage = 35,
            fireOrbEffectDuration = 4,
            earthOrbDamage = 30,
            airOrbDamage = 25,
            waterOrbDamage = 30
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase6)
        {
            thrazHealth = 200,
            treeHealth = 90,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 3,
            excavatorMaxCount = 5,
            excavatorSpawnCooldown = 2,
            enableToxicSmoke = true,
            smokeCooldown = 3,
            smokeMinSpawnTurn = 4,
            enableDrones = true,
            droneMinSpawnTurn = 4,
            droneSpawnAmount = 1,
            droneMaxCount = 3,
            droneCooldown = 3,
            fireOrbDamage = 40,
            fireOrbEffectDuration = 4,
            earthOrbDamage = 35,
            airOrbDamage = 30,
            waterOrbDamage = 35
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase7)
        {
            thrazHealth = 220,
            treeHealth = 100,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 3,
            excavatorMaxCount = 6,
            excavatorSpawnCooldown = 2,
            enableToxicSmoke = true,
            smokeCooldown = 2,
            smokeMinSpawnTurn = 4,
            enableDrones = true,
            droneMinSpawnTurn = 4,
            droneSpawnAmount = 1,
            droneMaxCount = 3,
            droneCooldown = 2,
            fireOrbDamage = 45,
            fireOrbEffectDuration = 4,
            earthOrbDamage = 40,
            airOrbDamage = 35,
            waterOrbDamage = 40
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase8)
        {
            thrazHealth = 240,
            treeHealth = 110,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 3,
            excavatorMaxCount = 7,
            excavatorSpawnCooldown = 2,
            enableToxicSmoke = true,
            smokeCooldown = 2,
            smokeMinSpawnTurn = 3,
            enableDrones = true,
            droneMinSpawnTurn = 4,
            droneSpawnAmount = 1,
            droneMaxCount = 4,
            droneCooldown = 2,
            fireOrbDamage = 50,
            fireOrbEffectDuration = 5,
            earthOrbDamage = 45,
            airOrbDamage = 40,
            waterOrbDamage = 45
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase9)
        {
            thrazHealth = 260,
            treeHealth = 120,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 3,
            excavatorMaxCount = 7,
            excavatorSpawnCooldown = 2,
            enableToxicSmoke = true,
            smokeCooldown = 2,
            smokeMinSpawnTurn = 3,
            enableDrones = true,
            droneMinSpawnTurn = 5,
            droneSpawnAmount = 1,
            droneMaxCount = 4,
            droneCooldown = 2,
            fireOrbDamage = 55,
            fireOrbEffectDuration = 5,
            earthOrbDamage = 50,
            airOrbDamage = 45,
            waterOrbDamage = 50
        });

        phaseSettingsList.Add(new PhaseSettings(Phase.Phase10)
        {
            thrazHealth = 300,
            treeHealth = 140,
            numberOfTrees = 6,
            excavatorMinSpawnTurn = 1,
            excavatorSpawnAmount = 4,
            excavatorMaxCount = 8,
            excavatorSpawnCooldown = 2,
            enableToxicSmoke = true,
            smokeCooldown = 2,
            smokeMinSpawnTurn = 3,
            enableDrones = true,
            droneMinSpawnTurn = 5,
            droneSpawnAmount = 1,
            droneMaxCount = 4,
            droneCooldown = 2,
            fireOrbDamage = 60,
            fireOrbEffectDuration = 5,
            earthOrbDamage = 55,
            airOrbDamage = 50,
            waterOrbDamage = 55
        });
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
            if (gridManager != null)
            {
                gridManager.treeHealth = settings.treeHealth;
                gridManager.qtd_arvore = settings.numberOfTrees;
            }

            if (thrazEngine != null)
            {
                thrazEngine.maxHealth = settings.thrazHealth;

                ThrazEngine.SummonableEnemy excavatorEnemy = thrazEngine.summonableEnemiesConfig.summonableEnemies.Find(e => e.enemyPrefab.name.Contains("Escavadeira"));
                if (excavatorEnemy != null)
                {
                    excavatorEnemy.enemySpawnCount = settings.excavatorSpawnAmount;
                    excavatorEnemy.enemySpawnLimit = settings.excavatorMaxCount;
                    excavatorEnemy.enemySpawnCooldown = settings.excavatorSpawnCooldown;
                    excavatorEnemy.enemySpawnMinimumRound = settings.excavatorMinSpawnTurn;
                }

                thrazEngine.toxicSmokeConfig.enabled = settings.enableToxicSmoke;
                thrazEngine.toxicSmokeConfig.toxicSmokeCooldown = settings.smokeCooldown;
                thrazEngine.toxicSmokeConfig.toxicSmokeMinimumRound = settings.smokeMinSpawnTurn;

                thrazEngine.protectionDroneConfig.enabled = settings.enableDrones;
                thrazEngine.protectionDroneConfig.droneMinimumRound = settings.droneMinSpawnTurn;
                thrazEngine.protectionDroneConfig.dronesToSummonPerTurn = settings.droneSpawnAmount;
                thrazEngine.protectionDroneConfig.numberOfDrones = settings.droneMaxCount;
                thrazEngine.protectionDroneConfig.droneCooldown = settings.droneCooldown;
            }

            if (orbManager != null)
            {
                OrbSettings fireOrb = orbManager.orbSettingsList.Find(o => o.orbType == OrbType.Fire);
                if (fireOrb != null)
                {
                    fireOrb.damage = settings.fireOrbDamage;
                    fireOrb.effectDuration = settings.fireOrbEffectDuration;
                }
                OrbSettings earthOrb = orbManager.orbSettingsList.Find(o => o.orbType == OrbType.Earth);
                if (earthOrb != null)
                {
                    earthOrb.damage = settings.earthOrbDamage;
                }
                OrbSettings airOrb = orbManager.orbSettingsList.Find(o => o.orbType == OrbType.Air);
                if (airOrb != null)
                {
                    airOrb.damage = settings.airOrbDamage;
                }
                OrbSettings waterOrb = orbManager.orbSettingsList.Find(o => o.orbType == OrbType.Water);
                if (waterOrb != null)
                {
                    waterOrb.damage = settings.waterOrbDamage;
                }
            }

            Debug.Log("Configurações aplicadas para a fase: " + currentPhase);
        }
        else
        {
            Debug.LogError("Configurações não encontradas para a fase: " + currentPhase);
        }
    }
}
