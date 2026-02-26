using UnityEngine;

public class J_Indicator : MonoBehaviour
{
    [SerializeField] private float _minimumViewDistance;
    [SerializeField] private GameObject[] _indicatorObjects;
    private GameObject _player;

    private bool _currentVisibility;
    private bool _previousVisibility;
    
    private void Start()
    {
        _player = GameObject.FindWithTag("PlayerTag");
        _currentVisibility = false;
        _previousVisibility = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Get the distance
        if ((_player.transform.position - transform.position).magnitude <= _minimumViewDistance)
        {
            _currentVisibility = true;
        }
        else
        {
            _currentVisibility = false;
        }

        ToggleVisibility();
    }

    private void ToggleVisibility()
    {
        if (_currentVisibility != _previousVisibility)
        {
            for (int i = 0; i < _indicatorObjects.Length; ++i)
            {
                _indicatorObjects[i].SetActive(_currentVisibility);
            }

            _previousVisibility = _currentVisibility;
        }
    }

    private void OnDrawGizmos()
    {
        if (_player != null)
        {
            if (_currentVisibility)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, _player.transform.position);
        }

    }
}
