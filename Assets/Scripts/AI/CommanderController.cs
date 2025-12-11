using System.Collections.Generic;
using UnityEngine;

public class CommanderController : MonoBehaviour
{
    [Header("Commander Settings")]
    [Tooltip("Is this enemy a Commander?")]
    [SerializeField] private bool isCommander = false;
    public bool IsCommander => isCommander;
    [Tooltip("If enemy is a commander set list of followers here")]
    [SerializeField] private List<AIStateMachine> followers = new List<AIStateMachine>();
    public List<AIStateMachine> Followers => followers;
    [Tooltip("if enemy is a commander set how close the squad has to be before starting to move")]
    [SerializeField] private float formationTolerance = 5f;
    [HideInInspector] public bool hasGroupedUp = false;


    // Checks if all followers of the commander are in their offset position
    public bool AllFollowersInFormation()
    {
        foreach (AIStateMachine follower in followers)
        {
            float distance = Vector3.Distance(follower.transform.position, transform.position);
            if (distance > formationTolerance)
                return false;
        }
        return true;
    }

    // Makes the closest follower of the old commander, the new commander and no longer a follower
    public void PromoteNewCommander()
    {
        // if there are no followers, then exit early
        if (followers.Count == 0)
        {
            return;
        }
        
        AIStateMachine newCommander = followers[0];
        float closestDistance = Vector3.Distance(transform.position, newCommander.transform.position);

        // loops through all the followers to find the closest one to the commander
        foreach (AIStateMachine follower in followers)
        {
            float dist =  Vector3.Distance(transform.position, follower.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                newCommander = follower;
            }
        }
        // Removes new commander from follow list, and assign old commanders variables to the new one
        followers.Remove(newCommander);
        AIStateMachine oldCommander = GetComponent<AIStateMachine>();
        newCommander.Waypoints = oldCommander.Waypoints;
        int currentWaypoint = oldCommander.currentWaypoint;

        foreach (AIStateMachine follower in followers)
        {
            follower.commander = newCommander.transform;
            follower.ChangeState(new FollowCommanderState(follower, follower.agent, newCommander.transform, follower.formationOffset));
        }
        
        CommanderController newController = newCommander.gameObject.AddComponent<CommanderController>();
        newController.followers = new List<AIStateMachine>(followers);
        newController.formationTolerance = formationTolerance;
        
        newCommander.ChangeState(new PatrolState(newCommander, newCommander.agent, newCommander.Waypoints, currentWaypoint));
        
        // Destroys the old commander controller
        Destroy(this);
    }
}
