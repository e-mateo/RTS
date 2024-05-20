using UnityEngine;

[CreateAssetMenu(fileName = "UA_Economy", menuName = "RTS/UtilitySystem/UA_Economy", order = 1)]
public class UA_Economy : UtilityAction
{
    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        //TO DO
        return 0f;
    }
}
