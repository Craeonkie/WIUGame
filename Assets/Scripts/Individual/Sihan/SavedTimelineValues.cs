using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class SavedTimelineValues : MonoBehaviour
{
    [SerializeField] private PlayableDirector _phaseDirector;
    [SerializeField] private GameObject[] _saveObjects;
    private List<Vector3> _savedPositions = new List<Vector3>();
    private List<Quaternion> _savedRotations = new List<Quaternion>();
    private List<Vector3> _savedScales = new List<Vector3>();

    public void SaveValues()
    {
        if (_saveObjects.Length == 0) return;

        for (int i = 0; i < _saveObjects.Length; i++)
        {
            _savedPositions[i] = _saveObjects[i].transform.position;
            _savedRotations[i] = _saveObjects[i].transform.rotation;
            _savedScales[i] = _saveObjects[i].transform.localScale;
        }
    }

    public void LoadValues()
    {
        if (_saveObjects.Length == 0) return;

        _phaseDirector.Stop();

        for (int i = 0; i < _saveObjects.Length; i++)
        {
            _saveObjects[i].transform.position = _savedPositions[i];
            _saveObjects[i].transform.rotation = _savedRotations[i];
            _saveObjects[i].transform.localScale = _savedScales[i];

            if (_saveObjects[i].TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (_saveObjects[i].TryGetComponent(out NavMeshAgent agent))
            {
                agent.Warp(transform.position);
            }
        }
    }
}
