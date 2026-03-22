using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;

public class EnemySpawner : MonoBehaviour
{
    private enum EnemyTypes
    {
        Patrol,
        TargetAndGuards,
        Stationary
    }
    [Tooltip("Assign the enemy squad type here")]
    [SerializeField] private EnemyTypes enemyTypes;
    
    [Tooltip("Assign the amount of enemies in squad here")]
    [SerializeField] private int EnemyAmount = 3;
    [Tooltip("Assign the squads waypoints here")]
    [SerializeField] private List<Transform> waypoints;
    [Tooltip("Assign the spacing between enemies here")]
    [SerializeField] private float FormationSpacing = 2f;
    [Tooltip("Assign the base enemy prefab here")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("Assign the target marker prefab here")]
    [SerializeField] private GameObject targetMarkerPrefab;
    [Tooltip("Objective SOs to link to the target spawned in the script")]
    [SerializeField, ShowField(nameof(IsTargetSpawner))] private TargetObjectivesSO[] linkedObjectives; 

    [Header("Enemy Type Data")]
    [Tooltip("Assign the patrolData SO here")]
    [SerializeField] private AIDataSO patrolData;
    [Tooltip("Assign the targetData SO here")]
    [SerializeField] private AIDataSO targetData;
    [Tooltip("Assign the guardData SO here")]
    [SerializeField] private AIDataSO guardData;
    [Tooltip("Assign the stationaryData SO here")]
    [SerializeField] private AIDataSO stationaryData;

    public bool IsTargetSpawner => enemyTypes is EnemyTypes.TargetAndGuards;
    
    private List<GameObject> squadMembers = new List<GameObject>();

    void Start()
    {
        // spawns enemies in formation and adds them to a list
        for (int i = 0; i < EnemyAmount; i++)
        {
            Vector3 spawnPosition;
            Vector2 offset = Vector2.zero;
            // commander spawn position
            if (i == 0)
            { 
                spawnPosition = transform.position;
            }
            else
            {
                // row which follower is in
                int row = (i - 1) / 2;
                // which column the follower is in (left or right)
                int column;
                if (i % 2 == 0)
                {
                    column = -1;
                }
                else
                {
                    column = 1;
                }
                //Calculates offset for all enemies formations
                offset = new Vector2(column * FormationSpacing, -(row + 1) * FormationSpacing);
                spawnPosition = transform.position + new Vector3(offset.x, 0, offset.y);
            }
            
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            squadMembers.Add(enemy);
            // assigns formation offsets to enemies 
            AIStateMachine controller = enemy.GetComponent<AIStateMachine>();
            controller.formationOffset = offset;
        }

        // if no enemies spawned then return
        if (squadMembers.Count == 0)
        {
            return;
        }
        
        // First enemy spawned is Commander
        GameObject commander = squadMembers[0];
        AIStateMachine commanderController = commander.GetComponent<AIStateMachine>();
        
        CommanderController commanderControllerController = commander.GetComponent<CommanderController>();
        commanderControllerController.SetAsCommander();

        // Assigns commanders followers
        for (int i = 1; i < squadMembers.Count; i++)
        {
            GameObject follower = squadMembers[i];
            AIStateMachine followerController = follower.GetComponent<AIStateMachine>();
            followerController.commander = commander.transform;
            commanderControllerController.Followers.Add(followerController);
        }

        ChangeEnemyTypes(commanderController);
    }

    private void ChangeEnemyTypes(AIStateMachine commanderController)
    {
        switch (enemyTypes)
        {
            case EnemyTypes.Patrol:
                // First enemy becomes patrol commander
                commanderController.SetTypeToPatrol();
                commanderController.Waypoints = waypoints;
                AIInventory commanderInventory = squadMembers[0].GetComponentInChildren<AIInventory>();
                commanderInventory.SetAIData(patrolData);
                // remaining enemies become patrol followers
                for (int i = 1; i < squadMembers.Count; i++)
                {
                    AIStateMachine patrolFollower = squadMembers[i].GetComponent<AIStateMachine>();
                    patrolFollower.SetTypeToPatrol();
                    AIInventory patrolFollowerInventory = squadMembers[i].GetComponentInChildren<AIInventory>();
                    patrolFollowerInventory.SetAIData(patrolData);
                }

                break;
            
            case EnemyTypes.TargetAndGuards:
                // First enemy becomes target
                commanderController.SetTypeToTarget(linkedObjectives);
                commanderController.Waypoints = waypoints;
                AIInventory targetInventory = squadMembers[0].GetComponentInChildren<AIInventory>();
                targetInventory.SetAIData(targetData);
                // Instantiates marker for target
                Instantiate(targetMarkerPrefab, squadMembers[0].transform);
                // register spawned target
                AIStateMachine.TargetsAndGuards.Add(commanderController);
                // remaining enemies become guards
                for (int i = 1; i < squadMembers.Count; i++)
                {
                    AIStateMachine guard = squadMembers[i].GetComponent<AIStateMachine>();
                    guard.SetTypeToGuard();
                    guard.SetProtectedTarget(commanderController);
                    AIInventory guardInventory = squadMembers[i].GetComponentInChildren<AIInventory>();
                    guardInventory.SetAIData(guardData);
                    // register spawned guards
                    AIStateMachine.TargetsAndGuards.Add(guard);
                }
                
                break;
            
            case EnemyTypes.Stationary:
                // all enemies become stationary
                for (int i = 0; i < squadMembers.Count; i++)
                {
                    AIStateMachine stationary = squadMembers[i].GetComponent<AIStateMachine>();
                    stationary.SetTypeToStationary();
                    AIInventory stationaryInventory = squadMembers[i].GetComponentInChildren<AIInventory>();
                    stationaryInventory.SetAIData(stationaryData);
                }

                break;
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.dodgerBlue;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
