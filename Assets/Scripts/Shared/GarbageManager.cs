using UnityEngine;

[CreateAssetMenu(fileName = "GarbageManager", menuName = "Scriptable Objects/GarbageManager")]
public class GarbageManager : ScriptableObject
{

    [SerializeField] private bool _isUIOpen;
    public bool isUIOpen 
    { 
        get => _isUIOpen; 
        set => _isUIOpen = value; 
    }

    [SerializeField] private bool _isMenuOpen;
    public bool isMenuOpen 
    { 
        get => _isMenuOpen; 
        set => _isMenuOpen = value; 
    }
}