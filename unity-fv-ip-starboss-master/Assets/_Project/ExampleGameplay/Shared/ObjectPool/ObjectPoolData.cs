using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Object Pool Data", menuName = "Scriptable Objects/Object Pool Data", order = 1)]
public class ObjectPoolData : ScriptableObject
{
    [Header("Settings")]
    public GameObject objectToPool;
    public int amountToPool;
    public bool shouldExpand;
}
