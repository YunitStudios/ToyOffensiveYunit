using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private enum EnemyTypes
    {
        Patrol,
        TargetAndGuards,
        Stationary
    }
    [SerializeField] private EnemyTypes enemyTypes;

    [SerializeField] private int EnemyAmount = 3;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<Transform> waypoints;

    void Start()
    {
        int currentEnemiesSpawned = 0;
        while (currentEnemiesSpawned < EnemyAmount)
        {
            GameObject instance = Instantiate(enemyPrefab, transform.position + Vector3.right * currentEnemiesSpawned, transform.rotation);
            currentEnemiesSpawned++;
            if (currentEnemiesSpawned == 1)
            {
                AIStateMachine controller = instance.GetComponent<AIStateMachine>();
                CommanderController commanderController = instance.GetComponent<CommanderController>();
                controller.Waypoints = waypoints;
            }
        }
    }
}
