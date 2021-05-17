using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spaceship Data", menuName = "Scriptable Objects/Spaceship Data", order = 1)]
public class SpaceshipData : ScriptableObject
{

    [Header("Movement")]
    public float thrustAmount;
    [HideInInspector]public float thrustInput;

    public float yawSpeed;
    public float pitchSpeed;
    [HideInInspector] public Vector3 steeringInput;

    public float leanAmount_X;
    public float leanAmount_Y;

    [Header("Shooting")]
    public float shootRate;
    [HideInInspector] public bool shootInput;

    [Header("Camera")]
    public float cameraTurnAmount;

    public void UpdateInputData(Vector3 newSteering, float newThrust, bool newShoot)
    {
        steeringInput = newSteering;
        thrustInput = newThrust;
        shootInput = newShoot;
    }

    public static SpaceshipData Clone(SpaceshipData origData)
    {
        
        SpaceshipData data = CreateInstance<SpaceshipData>();
        data.cameraTurnAmount = origData.cameraTurnAmount;
        data.leanAmount_X = origData.leanAmount_X;
        data.leanAmount_Y = origData.leanAmount_Y;
        data.pitchSpeed = origData.pitchSpeed;
        data.shootInput = origData.shootInput;
        data.shootRate = origData.shootRate;
        data.steeringInput = origData.steeringInput;
        data.thrustAmount = origData.thrustAmount;
        data.thrustInput = origData.thrustInput;
        data.yawSpeed = origData.yawSpeed;

        return data;
    }
}
