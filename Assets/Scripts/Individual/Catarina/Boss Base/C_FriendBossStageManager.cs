using Unity.AI.Navigation;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.AI;

public class C_FriendBossStageManager : MonoBehaviour
{
    [SerializeField] private string _SceneName;
    [SerializeField] private GameObject _BoardCollider;
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

        if (_BoardCollider != null)
        {
            _BoardCollider.SetActive(false);
        }
    }

    private void OnEnable()
    {
        C_FriendBoss.TransitionPhase1Action += EnteringPhase2;
        C_CupManager._EndGame += GameEnded;
    }

    private void OnDisable()
    {
        C_FriendBoss.TransitionPhase1Action -= EnteringPhase2;
        C_CupManager._EndGame -= GameEnded;
    }

    private void GameEnded ()
    {
        Debug.Log("Game ended");
        if (J_GameManager.Instance == null)
        {
            Debug.LogWarning("u r not starting from the start scene! Make sure start scene have the manager!");
            return;
        }
        J_GameManager.Instance.SetCurrentScene(this._SceneName);
    }
}
