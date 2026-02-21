using UnityEngine;
using UnityEngine.Events;

public abstract class C_BossAbility : MonoBehaviour
{
    public static event System.Action onFinishActivity;
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
        if (onFinishActivity != null)
        {
            onFinishActivity.Invoke();
        }
        GameTearDown();
    }
}
