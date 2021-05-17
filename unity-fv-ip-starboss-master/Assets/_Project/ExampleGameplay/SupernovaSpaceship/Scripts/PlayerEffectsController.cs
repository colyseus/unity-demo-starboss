using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerEffectsController : MonoBehaviour
{
    //[Header("Spaceship Settings")]
    private SpaceshipData data;

    public ParticleSystem[] hitEffects;
    public Transform trailParent;

    private TrailRenderer[] trails;

    public Renderer[] shipRenderers;
    public GameObject shipRoot;  //When the ship is destroyed, we'll disable this transform
    public ParticleSystem explosionEffect;

    private void Start()
    {
        trails = trailParent.GetComponentsInChildren<TrailRenderer>();

        PlayerSpaceshipController controller = GetComponent<PlayerSpaceshipController>();

        // Get updated reference to this ships data
        if (controller)
            data = controller.shipData.SpaceShipData;
    }

    void Update()
    {
        ModifyTrail();
        ModifyBoostEmission();
    }

    private void ModifyBoostEmission()
    {
        //throw new NotImplementedException();
    }

    void ModifyTrail()
    {
        foreach (TrailRenderer tr in trails)
        {
            tr.time = (data.thrustInput) * .35f;
        }
    }

    public void DisplayDamageEffect()
    {
        int randIdx = UnityEngine.Random.Range(0, hitEffects.Length);
        if (hitEffects[randIdx].isPlaying)
        {
            DisplayDamageEffect();  //Try another
        }
        else
        {
            hitEffects[randIdx].Play();
        }
    }

    public void ExplodeShip()
    {
        explosionEffect.Play();
        shipRoot.SetActive(false);
    }

    public void ResetShip()
    {
        explosionEffect.Stop();
        shipRoot.SetActive(true);
    }

    public void SetShipColor(Color color)
    {
        for (int i = 0; i < shipRenderers.Length; ++i)
        {
            shipRenderers[i].material.color = color;
        }
    }
}
