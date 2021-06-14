using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using LucidSightTools;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerSpaceshipController : ExampleNetworkedEntityView
{
    public delegate void OnPlayerShipActivated(PlayerSpaceshipController shipController);
    public static event OnPlayerShipActivated onPlayerShipActivated;

    public delegate void OnPlayerShipDeactivated(PlayerSpaceshipController shipController);
    public static event OnPlayerShipDeactivated onPlayerShipDeactivated;

    [Header("Spaceship Settings")]
    public PlayerShipDataController shipData;

    [Header("Physics")]
    public Rigidbody spaceshipRigidbody;

    [Header("Shooting")]
    public ObjectPoolBehaviour projectileObjectPool;
    public Transform projectileSpawnTransform;
    private float nextShot = 0.0f;
    public Transform shipModel;

    private const string FireKey = "fireKey";

    private float timeStuck = 0;

    private Vector3 remoteSteering; //Cached values for our current steering value - REMOTE ONLY
    private Vector3 rawRemoteSteeringInput;
    private Vector3 spawnPosition;

    [SerializeField]
    private Damageable damageable;

    [SerializeField]
    private UIHooks healthUIHooks;

    [SerializeField]
    private PlayerEffectsController effectsController;

    public int TeamIndex
    {
        get
        {
            return teamIndex;
        }
    }
    private int teamIndex = -1;
    private string lastAttacker = "";

    public Color defaultColor = Color.white;
    public Color[] teamColors;

    public bool IsAlive { get; private set; } = true;

    private Collider[] colliders;

    private void OnEnable()
    {
        StarBossGameManager.onUpdateClientTeam += OnTeamUpdated;
    }

    private void OnDisable()
    {
        StarBossGameManager.onUpdateClientTeam -= OnTeamUpdated;
        onPlayerShipDeactivated?.Invoke(this);
    }

    protected override void Start()
    {
        base.Start();

        colliders = GetComponentsInChildren<Collider>(true);
        UpdateHitPointUI();
    }

    private void OnTeamUpdated(int team, string id)
    {
        //If we're in team death match, we need our team index
        if (!StarBossGameManager.Instance.IsCoop && id.Equals(OwnerId))
        {
            SetTeam(team);
        }
    }

    protected override void Update()
    {
        base.Update();
        CheckForStuckState();
    }

    public void ResetDamage()
    {
        damageable.ResetDamage();

        if (IsMine)
        {
            UpdateHitPointUI();
        }
    }

    public void InitializeObjectForRemote()
    {
        //Arrange this prefab to work well as a remote view but disabling certain scripts (rather than have a unique second prefab)
        if (TryGetComponent(out PlayerInput playerInput))
        {
            Destroy(playerInput);
        }
        if (TryGetComponent(out PlayerSpaceshipInputBehaviour inputBehaviour))
        {
            Destroy(inputBehaviour);
        }
        if (TryGetComponent(out PlayerCameraController cameraController))
        {
            Destroy(cameraController);
        }
        if (TryGetComponent(out UIHooks uiHooks))
        {
            Destroy(uiHooks);
        }

        gameObject.tag = "OtherShip";
        gameObject.layer = 11;  //This is "OtherShip" in the physics layer
    }

    public override void InitiView(ColyseusNetworkedEntity entity)
    {
        base.InitiView(entity);
        damageable.remoteEntityID = entity.id;
        onPlayerShipActivated?.Invoke(this);

        //If we're in team death match, we need our team index
        if (!StarBossGameManager.Instance.IsCoop)
        {
            SetTeam(StarBossGameManager.Instance.GetTeamIndex(OwnerId));
        }

        if (IsMine)
        {
            spawnPosition = transform.position;
            UpdateHitPointUI();
        }
    }

    private void SetTeam(int idx)
    {
        teamIndex = idx;
        if (teamIndex >= 0)
        {
            effectsController.SetShipColor(teamColors[teamIndex]);
        }
    }

    protected override void ProcessViewSync()
    {
        base.ProcessViewSync();
        if (!myTransform.localRotation.Equals(proxyStates[0].rot))
        {
            Vector3 desiredForward = proxyStates[0].rot * Vector3.forward;
            float ang = Vector3.Angle(myTransform.forward, desiredForward);
            if (ang != 0)
            {
                Vector3 cross = Vector3.Cross(myTransform.forward, desiredForward);
                if (cross.y != 0)
                {
                    float roll = Mathf.Lerp(0, 1, ang / 15);    //We only really turn by ~15 degrees at most. if that changes, this should also change
                    rawRemoteSteeringInput.z = roll * (cross.y < 0 ? 1 : -1);
                }
                else
                {
                    rawRemoteSteeringInput.z = 0;
                }
            }
            else
            {
                rawRemoteSteeringInput = Vector3.zero;
            }
        }
        remoteSteering = Vector3.Slerp(remoteSteering, rawRemoteSteeringInput, Time.deltaTime * 1.5f);
        VisualSpaceshipTurn(remoteSteering);
    }

    private void ToggleShipColliders(bool active)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = active;
        }
    }

    private void FixedUpdate()
    {
        if (IsMine && IsAlive)
        {
            MoveSpaceship();
            TurnSpaceship();
        }

        CalculateShootingLogic();
    }

    private void CheckForStuckState()
    {
        // Don't need to perform this check if it's not our ship or the ship has been destroyed
        if (IsMine == false || IsAlive == false) return;

        if (spaceshipRigidbody.velocity.sqrMagnitude < 1)
        {
            timeStuck += Time.deltaTime;

            // Reset ship after 5 seconds
            if (timeStuck >= 5)
            {
                // Reset the ship's position and rotation
                transform.localPosition = transform.position + Vector3.up * 5;
                transform.rotation = Quaternion.identity;

                timeStuck = 0;
            }
        }
        else
        {
            timeStuck = 0;
        }
    }

    void MoveSpaceship()
    {
        Vector3 desVel = transform.forward * shipData.SpaceShipData.thrustAmount * (Mathf.Max(shipData.SpaceShipData.thrustInput, .2f));
        spaceshipRigidbody.velocity = Vector3.Slerp(spaceshipRigidbody.velocity, desVel, positionLerpSpeed * Time.fixedDeltaTime);
    }

    void TurnSpaceship()
    {
        Vector3 newTorque = new Vector3(shipData.SpaceShipData.steeringInput.x * shipData.SpaceShipData.pitchSpeed, -shipData.SpaceShipData.steeringInput.z * shipData.SpaceShipData.yawSpeed, 0);
        spaceshipRigidbody.AddRelativeTorque(newTorque);

        spaceshipRigidbody.rotation =
            Quaternion.Slerp(spaceshipRigidbody.rotation, Quaternion.Euler(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0)), .5f);

        VisualSpaceshipTurn(shipData.SpaceShipData.steeringInput);
    }

    void VisualSpaceshipTurn(Vector3 steeringInput)
    {
        shipModel.localEulerAngles = new Vector3(steeringInput.x * shipData.SpaceShipData.leanAmount_Y
            , shipModel.localEulerAngles.y, steeringInput.z * shipData.SpaceShipData.leanAmount_X);
    }

    void CalculateShootingLogic()
    {
        if (IsAlive == false || state == null)
        {
            return;
        }

        // Check if this ship should be firing
        if (!IsMine && state.attributes.TryGetValue(FireKey, out string fireVal))
        {
            shipData.SpaceShipData.shootInput = string.Equals(fireVal, "true");
        }

        if(shipData.SpaceShipData.shootInput == true)
        {
            if(Time.time > nextShot)
            {
                ShootProjectile();
                nextShot = Time.time + shipData.SpaceShipData.shootRate;
            }
        }
    }

    void ShootProjectile()
    {
        
        GameObject newProjectile = projectileObjectPool.GetPooledObject();
        newProjectile.transform.position = projectileSpawnTransform.position;
        newProjectile.transform.rotation = projectileSpawnTransform.rotation;
        newProjectile.SetActive(true);

        ProjectileBehaviour projBehavior = newProjectile.GetComponent<ProjectileBehaviour>();

        if (projBehavior)
        {
            projBehavior.isMine = IsMine;
            projBehavior.owner = this;
        }
    }

    public void ReceiveDamage(string damagerID, int amount)
    {
        ExampleManager.RFC(this, "ReceiveDamageRFC", new object[]{ Id, damagerID, amount });
    }

    public void ReceiveDamageRFC(string entityID, string damagerID, int amount)
    {
        if (entityID.Equals(Id))
        {
            //We got hit, pass this damage to Damageable
            damageable.ApplyDamage(new Damageable.DamageMessage()
            {
                amount = amount,
                damageSource = transform.position,
                damager = damagerID,
            });
            //Cache this for kill tracking - May want to clear after x seconds
            lastAttacker = damagerID;
            UpdateHitPointUI();
        }
    }

    public void OnShipDeath()
    {
        effectsController.ExplodeShip();

        ToggleShipColliders(false);

        IsAlive = false;

        if (IsMine)
        {
            //Disable controls, reset after a time, register a death
            if (TryGetComponent(out PlayerInput playerInput))
            {
                playerInput.DeactivateInput();
            }

            spaceshipRigidbody.velocity = Vector3.zero;
            StarBossGameManager.Instance.RegisterKill(lastAttacker, Id); ;
        }

        StartCoroutine(DelayReset());
    }

    private IEnumerator DelayReset()
    {
        yield return new WaitForSeconds(3.0f);
        damageable.ResetDamage();
        if (IsMine)
        {
            PositionAtSpawn();

            lastAttacker = "";
            if (TryGetComponent(out PlayerInput playerInput))
            {
                playerInput.ActivateInput();
            }
            UpdateHitPointUI();
        }
        yield return new WaitForSeconds(6.0f);

        IsAlive = true;

        ToggleShipColliders(true);

        effectsController.ResetShip();
        
    }

    public void PositionAtSpawn()
    {
        transform.position = GetSpawnPosition();

        if (StarBossGameManager.Instance.IsCoop == false)
        {
            // have the ship look at the spawn center
            transform.LookAt(StarBossGameManager.Instance.tdmSpawnCenter);
        }
        else
        {
            transform.LookAt(spawnPosition);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        if (StarBossGameManager.Instance.IsCoop)
        {
            Vector3 randomPt = Random.insideUnitSphere * 10;
            randomPt.y = 0;
            return spawnPosition + randomPt;
        }

        return StarBossGameManager.Instance.GetTDMSpawnPoint(TeamIndex);
    }

    private void UpdateHitPointUI()
    {
        if (healthUIHooks)
        {
            healthUIHooks.SetHealth(damageable.currentHitPoints, damageable.maxHitPoints);
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        WormAI worm = other.gameObject.GetComponentInParent<WormAI>();

        if (worm)
        {
            Damageable.DamageMessage message = new Damageable.DamageMessage()
            {
                amount = 1,
                damageSource = transform.position,
                damager = "boss"
            };

            damageable.ApplyDamage(message);

            UpdateHitPointUI();
        }
    }
}
