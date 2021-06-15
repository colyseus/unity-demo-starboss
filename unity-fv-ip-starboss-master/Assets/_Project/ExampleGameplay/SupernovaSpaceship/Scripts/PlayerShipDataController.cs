using UnityEngine;

public class PlayerShipDataController : MonoBehaviour
{
    private SpaceshipData clonedData;

#pragma warning disable 0649
    [Header("Reference Data")]
    [SerializeField]
    private SpaceshipData spaceshipData;
#pragma warning restore 0649
    public SpaceshipData SpaceShipData
    {
        get { return clonedData; }
    }

    private void Awake()
    {
        clonedData = SpaceshipData.Clone(spaceshipData);
    }
}