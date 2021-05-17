using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    private CinemachineFreeLook playerCamera;

    [Header("Spaceship Settings")]
    public PlayerShipDataController data;

    IEnumerator Start()
    {
        while (!StarBossGameManager.Instance.JoinComplete)
        {
            yield return new WaitForEndOfFrame();
        }
        playerCamera = Camera.main.transform.parent.GetComponentInChildren<CinemachineFreeLook>();
    }

    void Update()
    {
        if (!StarBossGameManager.Instance.JoinComplete)
            return;

        MoveCamera();
        ModifyFOV();
    }

    void MoveCamera()
    {
        playerCamera.m_XAxis.Value = data.SpaceShipData.cameraTurnAmount * data.SpaceShipData.steeringInput.z;
    }

    void ModifyFOV()
    {
        playerCamera.m_Lens.FieldOfView = 40 + (data.SpaceShipData.thrustInput * 10);
    }
}
