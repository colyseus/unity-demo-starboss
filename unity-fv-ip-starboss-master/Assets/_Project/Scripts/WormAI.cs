using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using LucidSightTools;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[System.Serializable] public class CameraEvent : UnityEvent<bool> { }
[System.Serializable] public class EffectEvent : UnityEvent<bool,bool> { }
[System.Serializable] public class ParticleEvent : UnityEvent<bool,int> { }

public class WormAI : MonoBehaviour, IMessageReceiver
{

    [HideInInspector] public CameraEvent OnBossReveal;
    [HideInInspector] public EffectEvent GroundContact;
    [HideInInspector] public ParticleEvent GroundDetection;

    [Header("Pathing")]
    [SerializeField] CinemachineSmoothPath path = default;
    [SerializeField] CinemachineDollyCart cart = default;
    [SerializeField] LayerMask terrainLayer = default;
    PlayerSpaceshipController playerShip;

    [HideInInspector]
    public UIHooks ui;

    [HideInInspector] public Vector3 startPosition, peakPosition, endPosition;

    RaycastHit hitInfo;
    int totalHealth;
    int currentHealth;
    Damageable[] damageables;

    private DamageVisual[] damageVisuals;

    private Action<int> reportDamage;
    private Action pathCompleted;

    private Vector3 originalPosition;
    private quaternion originalRotation;
    
    private bool ready = false;

    public bool PathInProgress { get; private set; } = false;

    private Vector3 pathRandomVariation = new Vector3(75.0f, 0.0f, 75.0f);

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    private void Init()
    {
        if (ready) return;

        ready = true;

        ui = GetComponent<UIHooks>();

        damageables = GetComponentsInChildren<Damageable>(true);
        damageVisuals = GetComponentsInChildren<DamageVisual>(true);

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        ResetWorm(0);
    }

    public void ResetWorm(int health)
    {
        if (!ready)
        {
            Init();
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;

        int healthPerSection = Mathf.FloorToInt(health / damageables.Length);

        totalHealth = 0;

        foreach (Damageable damageable in damageables)
        {
            damageable.SetMaxHitPoints(healthPerSection);

            totalHealth += healthPerSection;
        }

        currentHealth = totalHealth;

        foreach (DamageVisual damageVisual in damageVisuals)
        {
            damageVisual.ResetVisual();
        }

        ui.SetHealth(currentHealth, totalHealth);
    }

    public void SetWormCallbacks(Action<int> reportDamageCallback, Action pathCompletedCallback)
    {
        reportDamage = reportDamageCallback;
        pathCompleted = pathCompletedCallback;
    }

    public void BeginWormBehavior()
    {
        AI();
    }

    public void StopWormBehavior()
    {
        PathInProgress = false;

        StopAllCoroutines();
    }

    void AI()
    {
        // Update the worm's path to target the player
        UpdatePath();
        
        // Run the worm's behavior routine
        StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        PathInProgress = true;

        //play leaving ground effect

        yield return new WaitUntil(() => cart.m_Position >= 0.06f);
        GroundContact.Invoke(true, true);
        yield return new WaitUntil(() => cart.m_Position >= 0.23f);
        GroundContact.Invoke(false, true);

        // wait to reenter ground

        yield return new WaitUntil(() => cart.m_Position >= 0.60f);
        GroundContact.Invoke(true, false);
        yield return new WaitUntil(() => cart.m_Position >= 0.90f);
        GroundContact.Invoke(false, false);
        OnBossReveal.Invoke(false);

        // wait a beat to come out of ground again
        yield return new WaitForSeconds(1.5f);

        PathInProgress = false;

        pathCompleted?.Invoke();
    }

    public void UpdateHealthUI(int health)
    {
        ui.SetHealth(health, totalHealth);
    }

    public void OnReceiveMessage(MessageType type, object sender, object msg)
    {
        //if (type == MessageType.DAMAGED)
        //{
            Damageable damageable = sender as Damageable;
            Damageable.DamageMessage message = (Damageable.DamageMessage)msg;
            currentHealth -= message.amount;

            //LSLog.LogImportant($"Worm - Take Damage - Amount = {message.amount} Current Health = {currentHealth}", LSLog.LogColor.maroon);

            reportDamage?.Invoke(message.amount);

            //if (currentHealth <= 0)
            //    ui.ReloadScene();

            //ui.SetHealth(currentHealth, totalHealth);

        //}
    }

    public void UpdatePathStartEnd(Vector3 startPosition, Vector3 peakPosition, Vector3 endPosition)
    {
        this.startPosition = startPosition;
        this.peakPosition = peakPosition;
        this.endPosition = endPosition;
    }

    /// <summary>
    /// Update the worm's path using the variable values in <see cref="pathRandomVariation"/> for the X and Z axis
    /// </summary>
    private void UpdatePath()
    {
        if (Physics.Raycast(startPosition, Vector3.down, out hitInfo, 1000, terrainLayer.value))
        {
            startPosition = hitInfo.point;
 
        }

        if (Physics.Raycast(endPosition, Vector3.down, out hitInfo, 1000, terrainLayer.value))
        {
            endPosition = hitInfo.point;
            GroundDetection.Invoke(false, hitInfo.transform.CompareTag("Terrain") ? 0 : 1);
        }

        path.m_Waypoints[0].position = startPosition + (Vector3.down * 15);
        path.m_Waypoints[1].position = peakPosition;
        path.m_Waypoints[2].position = endPosition + (Vector3.down * 45);

        path.InvalidateDistanceCache();
        cart.m_Position = 0;

        //speed
        cart.m_Speed = cart.m_Path.PathLength / 1500;

        OnBossReveal.Invoke(true);
    }

    private void OnCollisionEnter(Collision other)
    {
        // Moved player/boss collision detection to the player ship

        //if (other.transform.TryGetComponent(out Damageable damageable))
        //{
        //    Damageable.DamageMessage message = new Damageable.DamageMessage()
        //    {
        //        amount = 1,
        //        damageSource = transform.position
        //    };
        //    damageable.ApplyDamage(message);
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPosition, 1);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPosition, 1);

    }
}
