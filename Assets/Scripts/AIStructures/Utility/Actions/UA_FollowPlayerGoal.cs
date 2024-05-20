using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UA_FollowPlayerGoal", menuName = "RTS/UtilitySystem/UA_FollowPlayerGoal", order = 1)]
public class UA_FollowPlayerGoal : UtilityAction
{
    public override float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        if (squad.bLockAttack)
            return 1;

        return 0.75f;
    }


    public override void OnEnter(UnitController controller, WorldState worldState, Squad squad)
    {

    }
}
