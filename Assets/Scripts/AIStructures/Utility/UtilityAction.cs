using UnityEngine;

public abstract class UtilityAction : ScriptableObject
{
    virtual public void OnEnter(UnitController controller, WorldState worldState, Squad squad) { }
    virtual public void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad) { }
    virtual public void OnExit(UnitController controller,WorldState worldState, Squad squad) { }

    virtual public float ComputePriority(UnitController controller,WorldState worldState, Squad squad) { return 0; }
}
