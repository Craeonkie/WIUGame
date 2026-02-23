using UnityEngine;

public class J_CarryItem : MonoBehaviour
{
    [SerializeField] private Transform _pillowDragPosition;
    [SerializeField] private Vector3 _detectionOffset;
    [SerializeField] private float _detectionRadius;
    [SerializeField] private LayerMask _layerToCheck;

    private static bool _isEnabled;

    private J_Pillow _currentPillow;
    private J_Pillow _nearestPillow;

    public static System.Action<J_Pillow> OnCarry;

    private void OnEnable()
    {
        PlayerController.OnInteract += Interact;
        J_BossBehaviour.OnStealPillow += ForceDropPillow;
    }

    private void OnDisable()
    {
        PlayerController.OnInteract -= Interact;
        J_BossBehaviour.OnStealPillow -= ForceDropPillow;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _isEnabled = true;
    }

    public static void Enable()
    {
        _isEnabled = true;
    }

    public static void Disable()
    {
        _isEnabled = false;
    }

    private void Interact()
    {
        if (!_isEnabled)
            return;

        // Overlap sphere forward
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.TransformDirection(_detectionOffset), _detectionRadius, _layerToCheck);
        float distance = float.MaxValue;

        _nearestPillow = null;

        // Drop pillow
        if (hits.Length == 0 && _currentPillow != null)
        {
            Debug.Log("Drop Pillow!");
            DropPillow();
        }
        else if (hits.Length > 0)
        {
            // Check nearby radius
            for (int i = 0; i < hits.Length; i++)
            {
                Debug.Log(hits[i].name);

                // Check if there are any stackable pillows
                if (hits[i].GetComponent<J_Pillow>().HasPillowAbove())
                    continue;

                if (hits[i].GetComponent<J_Pillow>() == _currentPillow)
                    continue;

                // Calculate the distance
                if ((hits[i].transform.position - (transform.position + transform.forward)).magnitude < distance)
                {
                    distance = (hits[i].transform.position - (transform.position + transform.forward)).magnitude;
                    _nearestPillow = hits[i].gameObject.GetComponent<J_Pillow>(); 
                }
            }


            // Get the nearby pillow
            if (_nearestPillow == null)
            {
                DropPillow();
                return;
            }

            // Check if current pillow is null
            if (_currentPillow == null)
            {
                Debug.Log("Carry Pillow!");

                // Pick up new pillow
                CarryPillow(_nearestPillow);
            }
            else
            {
                Debug.Log("Stack Pillow!");

                // Stack the pillows on top of each other
                StackPillow(_nearestPillow);
            }
        }
        else
        {
            Debug.Log("Nothing happened! No pillow in hand and no pillows nearby!");
        }
    }

    private void CarryPillow(J_Pillow pillow)
    {
        if (_currentPillow == null)
        {
            Debug.Log("Pillow was held!");

            _currentPillow = pillow;
            _currentPillow.GetCarried();
            pillow.transform.parent = _pillowDragPosition;
            pillow.transform.localPosition = Vector3.zero;
            pillow.transform.localRotation = Quaternion.identity;

            OnCarry?.Invoke(_currentPillow);
        }
        else
        {
            Debug.Log("Carry pillow was called but Current pillow isn't null!");
        }
    }

    private void DropPillow()
    {
        if (_currentPillow == null)
        {
            Debug.Log("Current pillow is null but LetGoOfPillow was called!");
            return;
        }

        _currentPillow.transform.parent = null;
        _currentPillow.GetDropped();
        _currentPillow = null;
        OnCarry?.Invoke(_currentPillow);
    }

    private void StackPillow(J_Pillow pillowBelow)
    {
        pillowBelow.Stack(_currentPillow);
        _currentPillow.GetStacked();
        _currentPillow = null;
        OnCarry?.Invoke(_currentPillow);
    }

    private void ForceDropPillow()
    {
        if (_currentPillow == null)
        {
            Debug.Log("Current pillow is null! But the boss is trying to steal it");
            return;
        }

        _currentPillow.transform.parent = null;
        _currentPillow.GetDropped();
        J_SpawnManager.Instance.Release("Pillow", _currentPillow.gameObject);
        _currentPillow = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(_detectionOffset), _detectionRadius);
    }
}
