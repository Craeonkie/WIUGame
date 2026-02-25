using UnityEngine;
using UnityEngine.Events;

public abstract class C_BossAbility : MonoBehaviour
{
    protected abstract void GameSetUp();
    protected abstract void GameTearDown();
    protected abstract void GameLogic();

    protected bool startAbility = false;

    protected void Update()
    {
        if (this.startAbility)
        {
            GameLogic();
        }
    }

    protected virtual void OnFinish()
    {

        GameTearDown();
    }
    protected void StartAbility()
    {
        this.enabled = true;
    }

    protected virtual void OnEnable()
    {
        C_FriendBossPhase2.StopAbility += GameTearDown;
    }

    protected virtual void OnDisable()
    {
        C_FriendBossPhase2.StopAbility += GameTearDown;
    }
}
