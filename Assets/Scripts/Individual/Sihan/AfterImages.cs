using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImages : MonoBehaviour
{
    [SerializeField] private GameObject _boneRoot;
    [SerializeField] private SkinnedMeshRenderer[] _meshRenderers;
    [SerializeField] private float _fadeInDuration;
    [SerializeField] private float _holdDuration;
    [SerializeField] private float _fadeOutDuration;
    [SerializeField] private List<Material> _materials = new List<Material>();
    [SerializeField] private float _targetAlpha;

    public void Initialise(Dictionary<string, Transform> bones, float fadeInDuration, float holdDuration, float fadeOutDuration, float targetAlpha, Color color)
    {
        Transform[] allBones = _boneRoot.GetComponentsInChildren<Transform>();

        foreach (Transform bone in allBones)
        {
            if (bones.ContainsKey(bone.name))
            {
                bone.position = bones[bone.name].position;
                bone.rotation = bones[bone.name].rotation;
            }
        }

        _materials.Clear();
        foreach (var renderer in _meshRenderers)
        {
            foreach (var mat in renderer.materials)
            {
                mat.SetColor("_Color", color);
                _materials.Add(mat);
            }
        }

        _targetAlpha = targetAlpha;

        StartCoroutine(FadeLoop(fadeInDuration, holdDuration, fadeOutDuration));
    }

    public IEnumerator FadeLoop(float fadeInDuration, float holdDuration, float fadeOutDuration)
    {
        yield return StartCoroutine(FadeStart(fadeInDuration));

        float timer = 0;

        while (timer < holdDuration)
        {
            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        yield return StartCoroutine(FadeEnd(fadeOutDuration));

        foreach (var mat in _materials)
        {
            Destroy(mat);
        }
        _materials.Clear();

        Destroy(gameObject);
    }

    public IEnumerator FadeStart(float fadeInDuration)
    {
        float timer = 0;

        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;

            if (timer > fadeInDuration) timer = fadeInDuration;

            float percentage = timer / fadeInDuration;

            UpdateMaterials(percentage);

            yield return null;
        }
    }

    public IEnumerator FadeEnd(float fadeOutDuration)
    {
        float timer = fadeOutDuration;

        while (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;

            if (timer < 0) timer = 0;

            float percentage = timer / fadeOutDuration;

            UpdateMaterials(percentage);

            yield return null;
        }
    }

    private void UpdateMaterials(float value)
    {
        foreach (var mat in _materials)
        {
            mat.SetFloat("_Alpha", value * _targetAlpha);
        }
    }
}
