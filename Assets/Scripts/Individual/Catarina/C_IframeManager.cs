using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class C_IframeManager : MonoBehaviour
{
    [Header("Iframe")]
    [SerializeField] private float _iframeDuration;
    [SerializeField] private LayerMask _ignoreLayerDuringIframe;
    public UnityEvent startIFrameEvent;
    public UnityEvent stopIFrameEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Invunerability()
    {
        int layerNumber = GetLayerFromMask(_ignoreLayerDuringIframe);
        startIFrameEvent.Invoke();
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), layerNumber, true);
        yield return new WaitForSeconds(_iframeDuration);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), layerNumber, false);
        stopIFrameEvent.Invoke();
    }
    private int GetLayerFromMask(LayerMask mask)
    {
        int layerNumber = 0;
        int layer = mask.value;
        while (layer > 1)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber;
    }
    public void StartIframe()
    {
        StartCoroutine(Invunerability());
    }
}
