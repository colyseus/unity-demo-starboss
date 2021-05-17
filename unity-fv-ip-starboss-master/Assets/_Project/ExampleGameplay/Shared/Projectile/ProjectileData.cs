using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Data", menuName = "Scriptable Objects/Projectile Data", order = 1)]
public class ProjectileData : ScriptableObject
{
    [Header("Movement")]
    public float movementSpeed;

    [Header("Auto Remove Projectile")]
    public float autoRemoveCountdown;

    public LayerMask layerMask;
}