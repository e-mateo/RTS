using UnityEngine;

[CreateAssetMenu(fileName = "UA_WaitForMoreUnit", menuName = "RTS/UtilitySystem/UA_WaitForMoreUnit", order = 1)]
public class UA_WaitForMoreUnit : UtilityAction
{
    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad) 
    {
        if(squad == null)
            return 0f;

        if (!squad.IsInitiliazed || squad.Units.Count == 0)
            return 0.9f;

        return 0; 
    }

}
