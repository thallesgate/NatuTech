using UnityEngine;

[System.Serializable]
public class SummonableEnemy
{
    public GameObject enemyPrefab;
    public int maxCount;
    public int minTurnsBetweenSummons;

    [HideInInspector]
    public int currentCount = 0;
    [HideInInspector]
    public int turnsSinceLastSummon;

    public void Initialize()
    {
        turnsSinceLastSummon = minTurnsBetweenSummons;
    }
}
