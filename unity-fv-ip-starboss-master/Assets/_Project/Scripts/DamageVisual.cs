using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DamageVisual : MonoBehaviour
{
    MeshRenderer meshRenderer;
    [ColorUsage(true,true)]
    public Color hitColor;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

    }

    public void ResetVisual()
    {
        meshRenderer.material.color = Color.white;
    }

    public void OnDamage()
    {
        meshRenderer.material.color = Color.white;
        meshRenderer.material.DOColor(hitColor, 0.1f).OnComplete(() => meshRenderer.material.DOColor(Color.white, 0.1f));
    }
    public void OnDeath()
    {
        //print("here");
        meshRenderer.material.DOColor(Color.grey, 0.2f);
    }
}
