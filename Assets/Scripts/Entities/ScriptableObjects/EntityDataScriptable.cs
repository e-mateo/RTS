using UnityEngine;

public class EntityDataScriptable : ScriptableObject
{
    [Header("Build Data")]
    public int TypeId = 0;
    public string Caption = "Unknown Unit";
    public int Cost = 1;
    public float BuildDuration = 1f;

    [Header("Health Points")]
    public int MaxHP = 100;
}
