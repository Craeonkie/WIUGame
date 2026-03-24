using UnityEngine;

public class testmanager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void debug(string msg)
    {
        Debug.Log(msg);
    }

    [ContextMenu("spawn")]
    public void spawn()
    {
        J_SpawnManager2.Instance.SpawnItemOnce("test2");
    }

    [ContextMenu("stop spawning")]
    public void stopspawning()
    {
        J_SpawnManager2.Instance.StopSpawning("test");
    }

    [ContextMenu("stop all spawning")]
    public void stopallspawning()
    {
        J_SpawnManager2.Instance.StopAllSpawning();
    }

    [ContextMenu("releaseitempool")]
    public void releaseitempool()
    {
        J_SpawnManager2.Instance.ReleaseItemPool("test2");
    }

    [ContextMenu("releaseall")]
    public void releaseallitems()
    {
        J_SpawnManager2.Instance.ReleaseAllItems();
    }
}
