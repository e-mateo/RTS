using UnityEngine;

[CreateAssetMenu(fileName = "Factory_Data", menuName = "RTS/FactoryData", order = 1)]
public class FactoryDataScriptable : EntityDataScriptable
{
    [Header("Spawn Unit Settings")]
    public int NbSpawnSlots = 10;
    public int SpawnRadius = 12;
    public int RadiusOffset = 4;

    [Header("Available Entities")]
    public GameObject[] AvailableUnits = null;
    public GameObject[] AvailableFactories = null;

    [Header("FX")]
    public GameObject DeathFXPrefab = null;

    [Header("Size")]
    public Vector2 FactorySize;

}
