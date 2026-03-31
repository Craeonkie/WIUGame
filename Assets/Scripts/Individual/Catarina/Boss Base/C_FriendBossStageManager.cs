using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class C_FriendBossStageManager : MonoBehaviour
{
    [SerializeField] private string _SceneName;
    [SerializeField] private GameObject _BoardCollider;


    [SerializeField] private GameObject _enemy;
    [SerializeField] private GameObject _enemyTPPoint;
    [SerializeField] private Vector3 RotateAngle;

    [SerializeField] private GameObject _player;

    [SerializeField] J_CutsceneManager _CutsceneManager;


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

        StartPhaseTransitionCutScene();
        _CutsceneManager.PlayCutscene(1);

    }

    private void OnEnable()
    {
        C_FriendBoss.TransitionPhase1Action += EnteringPhase2;
        C_CupManager._EndGame += GameEnded;
        C_Catapult.CatapultEnabled += ChangeToTopDownView;
        C_Catapult.CatapultDisable += ChangeFromTopDownView;
    }

    private void OnDisable()
    {
        C_FriendBoss.TransitionPhase1Action -= EnteringPhase2;
        C_CupManager._EndGame -= GameEnded;
        C_Catapult.CatapultEnabled -= ChangeToTopDownView;
        C_Catapult.CatapultDisable -= ChangeFromTopDownView;
        if (_player != null)
        {
            _player.SetActive(false);
        }
    }

    private void ChangeToTopDownView()
    {
        var _ply =  _player.GetComponent<PlayerController>();
        _ply.ToggleTopDownCamera(true);
        _player.SetActive(false);
    }

    private void ChangeFromTopDownView()
    {
        var _ply = _player.GetComponent<PlayerController>();
        _ply.ToggleTopDownCamera(false);
        _player.SetActive(true);
    }
    private void GameEnded ()
    {
        Debug.Log("Game ended");
        _CutsceneManager.PlayCutscene(2);

    }

    public void FinishLvl()
    {
        if (J_GameManager.Instance == null)
        {
            Debug.LogWarning("u r not starting from the start scene! Make sure start scene have the manager!");
            return;
        }
        J_GameManager.Instance.SetCurrentScene(this._SceneName);
    }

    public void StartPhaseTransitionCutScene()
    {
        Vector3 pos = _enemyTPPoint.transform.position;
        pos.y = _enemy.transform.position.y; 
        _enemy.transform.position = pos;
        _enemy.transform.rotation = Quaternion.Euler(RotateAngle);
    }

    private void Start()
    {
        _CutsceneManager.PlayCutscene(0);
        _player.SetActive(false);

    }
}
