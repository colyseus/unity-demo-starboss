using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [Header("Settings")]
    public ProjectileData data;

    public bool isMine = false;
    public PlayerSpaceshipController owner;

    void OnEnable()
    {
        StartCoroutine(TimerToAutoRemoveProjectile());
    }

    void FixedUpdate()
    {
        Vector3 tempPos = transform.position;
        MoveProjectile();
        if (Physics.Linecast(tempPos, transform.position, out RaycastHit hitInfo, data.layerMask))
        {
            if (hitInfo.transform.TryGetComponent(out Damageable damageable))
            {
                PlayerSpaceshipController pc = hitInfo.transform.gameObject.GetComponent<PlayerSpaceshipController>();

                if (hitInfo.transform.CompareTag("Enemy") || (hitInfo.transform.CompareTag("OtherShip") && StarBossGameManager.Instance.IsCoop == false && pc && StarBossGameManager.Instance.AreUsersSameTeam(owner, pc) == false))
                {
                    RemoveProjectile();

                    damageable.ApplyDamage(new Damageable.DamageMessage()
                    {
                        amount = isMine ? 2 : 0, // only apply damage if it came from our us and not another player
                        damageSource = transform.position,
                        damager = owner.Id,
                        isRFC = hitInfo.transform.CompareTag("OtherShip")   //If we're shooting an enemy ship, we need to broadcast an RFC instead of running local damage updates
                    });
                }
            }
        }

    }

    void MoveProjectile()
    {
        Vector3 tempVect = transform.forward * data.movementSpeed * Time.deltaTime;
        transform.position += tempVect;
    }

    IEnumerator TimerToAutoRemoveProjectile()
    {
        yield return new WaitForSeconds(data.autoRemoveCountdown);
        RemoveProjectile();
    }

    void RemoveProjectile()
    {
        gameObject.SetActive(false);
    }

}
