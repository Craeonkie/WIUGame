using UnityEngine;

[CreateAssetMenu(fileName = "GameManagerData", menuName = "Scriptable Objects/GameManagerData")]
public class GameManagerData : ScriptableObject
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