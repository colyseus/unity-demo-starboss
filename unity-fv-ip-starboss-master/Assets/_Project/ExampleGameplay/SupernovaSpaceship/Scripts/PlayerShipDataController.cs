using UnityEngine;

public class PlayerShipDataController : MonoBehaviour
{
    private SpaceshipData clonedData;

    [Header("Reference Data")]
    [SerializeField]
    private SpaceshipData spaceshipData;

    public SpaceshipData SpaceShipData
    {
        get { return clonedData; }
    }

    private void Awake()
    {
        clonedData = SpaceshipData.Clone(spaceshipData);
    }
}