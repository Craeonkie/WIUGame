using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class J_ShaderProperty
{
    public string PropertyName;
    public enum PROPERTYTYPE
    {
        COLOUR,
        INT,
        FLOAT,
        VECTOR
    }

    public PROPERTYTYPE Type;

    public J_ShaderPropertyDataValue OriginalValue;
    public IEnumerator TransitionCoroutine;

    public J_ShaderPropertyTransition[] Transitions;
    public int CurrentTransitionIndex;

    public J_ShaderPropertyTransition FindTransitionByName(string name)
    {
        for (int i = 0; i < Transitions.Length; ++i)
        {
            if (Transitions[i].TransitionName == name)
                return Transitions[i];
        }

        return null;
    }
}

[System.Serializable]
public class J_ShaderPropertyTransition
{
    public string TransitionName;

    public J_ShaderPropertyDataValue StartValue;
    public J_ShaderPropertyDataValue EndValue;

    public float TransitionDuration;
    public AnimationCurve TransitionCurve;

    public UnityEvent OnTransitionStart;
    public UnityEvent OnTransitionFinish;

    public bool IsCompleted;
}

[System.Serializable]
public class J_ShaderPropertyDataValue
{
    public Color Colour;
    public int Int;
    public float Float;
    public Vector4 Vector;
}
