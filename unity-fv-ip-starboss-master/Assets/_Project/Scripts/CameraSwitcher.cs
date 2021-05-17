using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CameraSwitcher : MonoBehaviour
{

    public Transform bossCamera;
    public CinemachineTargetGroup targetGroup;
    public WormAI bossAI;

    void Start()
    {
        bossAI.OnBossReveal.AddListener((x) => BossReveal(x));
    }

    public void FocusOnBoss(InputAction.CallbackContext value)
    {
        bossCamera.gameObject.SetActive(value.performed);
    }

    public void BossReveal(bool state)
    {
        //if(state)
            //StartCoroutine(AutomaticActivation());

        DOVirtual.Float(targetGroup.m_Targets[1].weight, state ? 1 : 0, state ? 2.5f : 1f, ChangeBossTargetWeight);

        IEnumerator AutomaticActivation()
        {
            bossCamera.gameObject.SetActive(true);
            yield return new WaitForSeconds(3);
            bossCamera.gameObject.SetActive(false);
        }

    }

    void ChangeBossTargetWeight(float weight)
    {
        targetGroup.m_Targets[1].weight = weight;
    }
}
