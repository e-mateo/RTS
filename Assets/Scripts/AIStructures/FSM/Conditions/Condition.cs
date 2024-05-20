using UnityEngine;

public abstract class Condition : ScriptableObject
{
    virtual public void Init(WorldState worldState) { }
    virtual public bool UpdateCondition(float updateFrequency, WorldState worldState) { return false; }
}
