using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class ShaderItem
{
    public string ItemName;
    public J_ShaderData ShaderData;
    public bool ResetValueOnDestroy = true;


    public J_ShaderProperty[] ShaderProperties;
    
    public void Init()
    {
        for (int i = 0; i < ShaderProperties.Length; ++i)
        {
            if (ShaderProperties[i].Type == J_ShaderProperty.PROPERTYTYPE.COLOUR)
                ShaderProperties[i].OriginalValue.Colour = ShaderData.ShaderMaterial.GetColor(ShaderProperties[i].PropertyName);
            else if (ShaderProperties[i].Type == J_ShaderProperty.PROPERTYTYPE.INT)
                ShaderProperties[i].OriginalValue.Int = ShaderData.ShaderMaterial.GetInt(ShaderProperties[i].PropertyName);
            else if (ShaderProperties[i].Type == J_ShaderProperty.PROPERTYTYPE.FLOAT)
                ShaderProperties[i].OriginalValue.Float = ShaderData.ShaderMaterial.GetFloat(ShaderProperties[i].PropertyName);
            else
                ShaderProperties[i].OriginalValue.Vector = ShaderData.ShaderMaterial.GetVector(ShaderProperties[i].PropertyName);
        }
    }


    public void ResetToTransitionValue(string propertyName, bool endValue = false)
    {
        J_ShaderProperty property = FindPropertyByName(propertyName);
        if (property == null)
        {
            Debug.LogError("This property doesn't exist!");
            return;
        }

        if (property.CurrentTransitionIndex >= property.Transitions.Length)
        {
            Debug.LogError("Transition index out of range!");
            return;
        }

        J_ShaderPropertyTransition transition = property.Transitions[property.CurrentTransitionIndex];
        J_ShaderPropertyDataValue value = endValue ? transition.EndValue : transition.StartValue;


        if (property.Type == J_ShaderProperty.PROPERTYTYPE.COLOUR)
        {
            ShaderData.ShaderMaterial.SetColor(propertyName, value.Colour);
        }
        else if (property.Type == J_ShaderProperty.PROPERTYTYPE.INT)
        {
            ShaderData.ShaderMaterial.SetInt(propertyName, value.Int);
        }
        else if (property.Type == J_ShaderProperty.PROPERTYTYPE.FLOAT)
        {
            ShaderData.ShaderMaterial.SetFloat(propertyName, value.Float);
        }
        else
        {
            ShaderData.ShaderMaterial.SetVector(propertyName, value.Vector);
        }
    }
    public void ResetToTransitionValue(string propertyName, string transitionName, bool endValue = false)
    {
        J_ShaderProperty property = FindPropertyByName(propertyName);
        if (property == null)
        {
            Debug.LogError("This property doesn't exist!");
            return;
        }

        J_ShaderPropertyTransition transition = property.FindTransitionByName(transitionName);
        if (transition == null)
        {
            Debug.LogError("This transition doesn't exist!");
            return;
        }

        J_ShaderPropertyDataValue value = endValue ? transition.EndValue : transition.StartValue;


        if (property.Type == J_ShaderProperty.PROPERTYTYPE.COLOUR)
        {
            ShaderData.ShaderMaterial.SetColor(propertyName, value.Colour);
        }
        else if (property.Type == J_ShaderProperty.PROPERTYTYPE.INT)
        {
            ShaderData.ShaderMaterial.SetInt(propertyName, value.Int);
        }
        else if (property.Type == J_ShaderProperty.PROPERTYTYPE.FLOAT)
        {
            ShaderData.ShaderMaterial.SetFloat(propertyName, value.Float);
        }
        else
        {
            ShaderData.ShaderMaterial.SetVector(propertyName, value.Vector);
        }
    }


    public void ResetTransitionIndex(string propertyName)
    {
        J_ShaderProperty property = FindPropertyByName(propertyName);
        if (property == null)
        {
            Debug.LogError("This property doesn't exist!");
            return;
        }

        property.CurrentTransitionIndex = 0;
    }


    public IEnumerator Transition(J_ShaderProperty property, J_ShaderPropertyTransition transition)
    {
        if (property.Type == J_ShaderProperty.PROPERTYTYPE.COLOUR)
            return TransitionColour(ShaderData.ShaderMaterial, property, transition);
        else if (property.Type == J_ShaderProperty.PROPERTYTYPE.INT)
            return TransitionInt(ShaderData.ShaderMaterial, property, transition);
        else if (property.Type == J_ShaderProperty.PROPERTYTYPE.FLOAT)
            return TransitionFloat(ShaderData.ShaderMaterial, property, transition);
        else
            return TransitionVector(ShaderData.ShaderMaterial, property, transition);
    }


    private IEnumerator TransitionColour(Material mat, J_ShaderProperty property, J_ShaderPropertyTransition transition)
    {
        float t = 0;
        float elapsedTime = 0f;
        float duration = transition.TransitionDuration;

        Color currentColour = mat.GetColor(property.PropertyName);
        transition.OnTransitionStart?.Invoke();

        while (elapsedTime < duration)
        {
            t = elapsedTime / transition.TransitionDuration;
            t = transition.TransitionCurve.Evaluate(t);

            currentColour = Color.Lerp(transition.StartValue.Colour, transition.EndValue.Colour, t);
            mat.SetColor(property.PropertyName, currentColour);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mat.SetColor(property.PropertyName, transition.EndValue.Colour);
        property.CurrentTransitionIndex = System.Array.IndexOf(property.Transitions, transition) + 1;
        property.TransitionCoroutine = null;

        transition.IsCompleted = true;
        transition.OnTransitionFinish?.Invoke();
    }
    private IEnumerator TransitionInt(Material mat, J_ShaderProperty property, J_ShaderPropertyTransition transition)
    {
        float t = 0;
        float elapsedTime = 0f;
        float duration = transition.TransitionDuration;

        int currentValue = mat.GetInteger(property.PropertyName);
        transition.OnTransitionStart?.Invoke();

        while (elapsedTime < duration)
        {
            t = elapsedTime / transition.TransitionDuration;
            t = transition.TransitionCurve.Evaluate(t);

            currentValue = (int)Mathf.Lerp(transition.StartValue.Int, transition.EndValue.Int, t);
            mat.SetInt(property.PropertyName, currentValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mat.SetInt(property.PropertyName, transition.EndValue.Int);
        property.CurrentTransitionIndex = System.Array.IndexOf(property.Transitions, transition) + 1;
        property.TransitionCoroutine = null;

        transition.IsCompleted = true;
        transition.OnTransitionFinish?.Invoke();
    }
    private IEnumerator TransitionFloat(Material mat, J_ShaderProperty property, J_ShaderPropertyTransition transition)
    {
        float t = 0;
        float elapsedTime = 0f;
        float duration = transition.TransitionDuration;

        float currentValue = mat.GetFloat(property.PropertyName);
        transition.OnTransitionStart?.Invoke();

        while (elapsedTime < duration)
        {
            t = elapsedTime / transition.TransitionDuration;
            t = transition.TransitionCurve.Evaluate(t);

            currentValue = Mathf.Lerp(transition.StartValue.Float, transition.EndValue.Float, t);
            mat.SetFloat(property.PropertyName, currentValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mat.SetFloat(property.PropertyName, transition.EndValue.Float);
        
        property.CurrentTransitionIndex = System.Array.IndexOf(property.Transitions, transition) + 1;
        property.TransitionCoroutine = null;

        transition.IsCompleted = true;
        transition.OnTransitionFinish?.Invoke();
    }
    private IEnumerator TransitionVector(Material mat, J_ShaderProperty property, J_ShaderPropertyTransition transition)
    {
        float t = 0;
        float elapsedTime = 0f;
        float duration = transition.TransitionDuration;

        Vector4 currentValue = mat.GetVector(property.PropertyName);
        transition.OnTransitionStart?.Invoke();

        while (elapsedTime < duration)
        {
            t = elapsedTime / transition.TransitionDuration;
            t = transition.TransitionCurve.Evaluate(t);

            currentValue = Vector4.Lerp(transition.StartValue.Vector, transition.EndValue.Vector, t);
            mat.SetVector(property.PropertyName, currentValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mat.SetVector(property.PropertyName, transition.EndValue.Vector);

        property.CurrentTransitionIndex = System.Array.IndexOf(property.Transitions, transition) + 1;
        property.TransitionCoroutine = null;

        transition.IsCompleted = true;
        transition.OnTransitionFinish?.Invoke();
    }


    public void Reset()
    {
        for (int i = 0; i < ShaderProperties.Length; i++)
        {
            if (ShaderProperties[i].Type == J_ShaderProperty.PROPERTYTYPE.COLOUR)
                ShaderData.ShaderMaterial.SetColor(ShaderProperties[i].PropertyName, ShaderProperties[i].OriginalValue.Colour);
            else if (ShaderProperties[i].Type == J_ShaderProperty.PROPERTYTYPE.INT)
                ShaderData.ShaderMaterial.SetInt(ShaderProperties[i].PropertyName, ShaderProperties[i].OriginalValue.Int);
            else if (ShaderProperties[i].Type == J_ShaderProperty.PROPERTYTYPE.FLOAT)
                ShaderData.ShaderMaterial.SetFloat(ShaderProperties[i].PropertyName, ShaderProperties[i].OriginalValue.Float);
            else
                ShaderData.ShaderMaterial.SetVector(ShaderProperties[i].PropertyName, ShaderProperties[i].OriginalValue.Vector);
        }
    }


    // Helper functions
    private J_ShaderProperty FindPropertyByName(string name)
    {
        for (int i = 0; i < ShaderProperties.Length; ++i)
        {
            if (ShaderProperties[i].PropertyName == name)
            {
                return ShaderProperties[i];
            }
        }

        return null;
    }
}


public class J_ShadersManager : MonoBehaviour
{
    public static J_ShadersManager Instance { get; private set; }

    [Header("Shaders")]
    [SerializeField] ShaderItem[] _shaderItems;

    [Header("Post-Processing Shaders")]
    [SerializeField] private Volume GlobalVolume;

    [Header("Vignette")]
    [SerializeField] private bool _setValueOnAwake;
    [SerializeField] private float _vignetteRadiusValue;
    [SerializeField] private float _vignetteFeatherValue;

    private VignetteVolume _vignetteVolume;
    private IEnumerator _vignetteRadiusCoroutine;
    private IEnumerator _vignetteFeatherCoroutine;
    [SerializeField] private J_ShaderPropertyTransition[] _vignetteTransitions;

    private float _originalVignetteRadiusValue;
    private float _originalVignetteFeatherValue;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < _shaderItems.Length; ++i)
        {
            _shaderItems[i].Init();
        }

        if (GlobalVolume.profile.TryGet<VignetteVolume>(out var vignetteSetting))
        {
            _vignetteVolume = vignetteSetting;
            _originalVignetteRadiusValue = _vignetteVolume.radius.value;
            _originalVignetteFeatherValue = _vignetteVolume.feather.value;

            if (_setValueOnAwake)
            {
                _vignetteVolume.radius.value = Mathf.Clamp(_vignetteRadiusValue, -2f, 2f);
                _vignetteVolume.feather.value = Mathf.Clamp(_vignetteFeatherValue, 0, 3f);
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void TriggerTransition(string shaderName, string propertyName)
    {
        ShaderItem shader = FindShaderByName(shaderName);
        if (shader == null)
        {
            Debug.LogError("This shader does not exist!");
            return;
        }

        J_ShaderProperty property = FindPropertyByName(shader, propertyName);
        if (property.CurrentTransitionIndex >= property.Transitions.Length)
        {
            Debug.LogError("Transition index out of range!");
            return;
        }

        J_ShaderPropertyTransition transition = property.Transitions[property.CurrentTransitionIndex];


        if (property.TransitionCoroutine != null)
            StopCoroutine(property.TransitionCoroutine);

        property.TransitionCoroutine = shader.Transition(property, transition);

        StartCoroutine(property.TransitionCoroutine);
    }
    public void TriggerTransition(string shaderName, string propertyName, string transitionName)
    {
        ShaderItem shader = FindShaderByName(shaderName);
        if (shader == null)
        {
            Debug.LogError("This shader does not exist!");
            return;
        }

        J_ShaderProperty property = FindPropertyByName(shader, propertyName);
        if (property == null)
        {
            Debug.LogError("This property doesn't exist!");
            return;
        }

        J_ShaderPropertyTransition transition = property.FindTransitionByName(transitionName);
        if (transition == null)
        {
            Debug.LogError("This transition doesn't exist!");
            return;
        }

        if (property.TransitionCoroutine != null)
            StopCoroutine(property.TransitionCoroutine);

        property.TransitionCoroutine = shader.Transition(property, transition);

        StartCoroutine(property.TransitionCoroutine);
    }


    public void StopTransition(string shaderName, string propertyName)
    {
        ShaderItem shader = FindShaderByName(shaderName);
        if (shader == null)
        {
            Debug.LogError("This shader does not exist!");
            return;
        }

        J_ShaderProperty property = FindPropertyByName(shader, propertyName);
        if (property == null)
        {
            Debug.LogError("This property doesn't exist!");
            return;
        }

        if (property.CurrentTransitionIndex >= property.Transitions.Length)
        {
            Debug.LogError("Transition index out of range!");
            return;
        }

        J_ShaderPropertyTransition transition = property.Transitions[property.CurrentTransitionIndex];

        if (property.TransitionCoroutine != null)
            StopCoroutine(property.TransitionCoroutine);
    }
    public void StopTransition(string shaderName, string propertyName, string transitionName)
    {
        ShaderItem shader = FindShaderByName(shaderName);
        if (shader == null)
        {
            Debug.LogError("This shader does not exist!");
            return;
        }

        J_ShaderProperty property = FindPropertyByName(shader, propertyName);
        if (property == null)
        {
            Debug.LogError("This property doesn't exist!");
            return;
        }

        if (property.CurrentTransitionIndex >= property.Transitions.Length)
        {
            Debug.LogError("Transition index out of range!");
            return;
        }

        J_ShaderPropertyTransition transition = property.Transitions[property.CurrentTransitionIndex];

        if (property.TransitionCoroutine != null)
            StopCoroutine(property.TransitionCoroutine);
    }


    public void ResetToTransitionValue(string shaderName, string propertyName, bool endValue = false)
    {
        ShaderItem shader = FindShaderByName(shaderName);
        if (shader == null)
        {
            Debug.LogError("This shader does not exist!");
            return;
        }

        shader.ResetToTransitionValue(propertyName, endValue);
    }
    public void ResetToTransitionValue(string shaderName, string propertyName, string transitionName, bool endValue = false)
    {
        ShaderItem shader = FindShaderByName(shaderName);
        if (shader == null)
        {
            Debug.LogError("This shader does not exist!");
            return;
        }

        shader.ResetToTransitionValue(propertyName, transitionName, endValue);
    }


    public void ResetTransitionIndex(string shaderName, string propertyName)
    {
        ShaderItem shader = FindShaderByName(shaderName);
        if (shader == null)
        {
            Debug.LogError("This shader does not exist!");
            return;
        }

        shader.ResetTransitionIndex(propertyName);
    }


    /// <summary>
    /// Transitions either the radius or the feather of the vignette
    /// </summary>
    /// <param name="transitionName"></param>
    /// <param name="transitionRadius"></param>
    public void TriggerVignetteRadiusTransition(string transitionName)
    {
        J_ShaderPropertyTransition transition = FindTransitionByName(transitionName, _vignetteTransitions);
        if (transition == null)
        {
            Debug.LogError("Transition name does not exist!");
            return;
        }


        if (_vignetteRadiusCoroutine != null)
        {
            StopCoroutine(_vignetteRadiusCoroutine);
        }
        _vignetteRadiusCoroutine = VignetteTransition(transition, true);
        StartCoroutine(_vignetteRadiusCoroutine);
    }

    public void TriggerVignetteFeatherTransition(string transitionName)
    {
        J_ShaderPropertyTransition transition = FindTransitionByName(transitionName, _vignetteTransitions);
        if (transition == null)
        {
            Debug.LogError("Transition name does not exist!");
            return;
        }


        if (_vignetteFeatherCoroutine != null)
        {
            StopCoroutine(_vignetteFeatherCoroutine);
        }
        _vignetteFeatherCoroutine = VignetteTransition(transition, false);
        StartCoroutine(_vignetteFeatherCoroutine);
    }

    private IEnumerator VignetteTransition(J_ShaderPropertyTransition transition, bool isRadius)
    {
        float t = 0;
        float elapsedTime = 0f;
        float duration = transition.TransitionDuration;

        float currentValue;

        currentValue = isRadius ? _vignetteVolume.radius.value : _vignetteVolume.feather.value;

        transition.OnTransitionStart?.Invoke();

        while (elapsedTime < duration)
        {
            t = elapsedTime / transition.TransitionDuration;
            t = transition.TransitionCurve.Evaluate(t);

            currentValue = Mathf.Lerp(transition.StartValue.Float, transition.EndValue.Float, t);

            if (isRadius)
                _vignetteVolume.radius.value = currentValue;
            else
                _vignetteVolume.feather.value = currentValue;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (isRadius)
        {
            _vignetteVolume.radius.value = transition.EndValue.Float;
            _vignetteRadiusCoroutine = null;
        }
        else
        {
            _vignetteVolume.feather.value = transition.EndValue.Float;
            _vignetteFeatherCoroutine = null;
        }

        transition.IsCompleted = true;
        transition.OnTransitionFinish?.Invoke();
    }


    private void OnDestroy()
    {
        for (int i = 0; i < _shaderItems.Length; i++) 
        {
            _shaderItems[i].Reset();
        }

        _vignetteVolume.radius.value = _originalVignetteRadiusValue;
        _vignetteVolume.feather.value = _originalVignetteFeatherValue;
    }


    // Helper function
    public ShaderItem FindShaderByName(string name)
    {
        for (int i = 0; i < _shaderItems.Length; ++i)
        {
            if (_shaderItems[i].ItemName == name)
            {
                return _shaderItems[i];
            }
        }

        return null;
    }
    private J_ShaderProperty FindPropertyByName(ShaderItem item, string name)
    {
        for (int i = 0; i < item.ShaderProperties.Length; ++i)
        {
            if (item.ShaderProperties[i].PropertyName == name)
            {
                return item.ShaderProperties[i];
            }
        }

        return null;
    }
    public J_ShaderPropertyTransition FindTransitionByName(string name, J_ShaderPropertyTransition[] transitions)
    {
        for (int i = 0; i < transitions.Length; ++i)
        {
            if (transitions[i].TransitionName == name)
                return transitions[i];
        }

        return null;
    }
}
