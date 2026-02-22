using UnityEngine;

public class C_RenderQueue : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material _Material;
    [SerializeField] private int _RenderQueueIndex = 0;

    private void Start()
    {
        if (_Material == null) return; 
        _Material.renderQueue = _RenderQueueIndex;
    }
}
