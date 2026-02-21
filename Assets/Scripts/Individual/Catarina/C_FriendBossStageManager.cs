using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class C_FriendBossStageManager : MonoBehaviour
{

    private void EnteringPhase2()
    {
        var navmesh = FindFirstObjectByType<NavMeshSurface>();
        if (navmesh != null)
        {
            navmesh.enabled = false;
        }
        else
        {
            Debug.LogWarning("Nav mesh surface cannot be found");

        }
        var navmeshAgent = FindFirstObjectByType<NavMeshAgent>();
        if (navmeshAgent != null)
        {
            navmeshAgent.enabled = false;
        }
        else
        {
            Debug.LogWarning("Nav mesh agent cannot be found");
        }


    }

    private void OnEnable()
    {
        C_FriendBoss.TransitionPhase1Action += EnteringPhase2;
    }

    private void OnDisable()
    {
        C_FriendBoss.TransitionPhase1Action -= EnteringPhase2;
    }
}
