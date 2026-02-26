using UnityEngine;

public class C_CatapultManager : MonoBehaviour
{
    public bool UsingCatapult{ get; set; }
    [SerializeField] private string _ThrowableTagName;

    public static event System.Action UseCatapult;
    public static event System.Action<GameObject> CatapultSetObj;
    private GameObject _obj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UsingCatapult = false;
        C_Catapult.CatapultDisable += IsNotUsingCatapult;
    }

    private void IsNotUsingCatapult()
    {
        UsingCatapult = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (UsingCatapult) return;
        if (other.gameObject.CompareTag(_ThrowableTagName))
        {
            if (other.transform.parent == null)
            {
                UseCatapult?.Invoke();
                CatapultSetObj?.Invoke(other.gameObject);
                UsingCatapult = true;
                
            }
        }
    }
}
