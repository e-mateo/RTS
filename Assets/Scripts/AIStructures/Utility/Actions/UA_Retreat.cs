using UnityEngine;

[CreateAssetMenu(fileName = "UA_Retreat", menuName = "RTS/UtilitySystem/UA_Retreat", order = 1)]
public class UA_Retreat : UtilityAction
{
    [SerializeField] float distanceMinFromFactoryToRetreat = 60f;
    int TroupNeeded;
    float squadForceToExit;
    Factory nearestFacto;
    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        AIController aIController = controller as AIController;

        if (squad == null) return 0f;

        if (squad.SquadState == ESquadState.RETREAT && squad.NeedMoreTroups && squad.SquadForce <= squadForceToExit)
        {
            return 1.0f;
        }

        if (squad.SquadState == ESquadState.ATTACK_SQUAD && aIController.StrategicState == EStraticState.EXPLORATION)
        {
            if ((float)squad.SquadForce < (float)squad.TargetAttackingSquad.SquadForce * 0.4f)
            {
                nearestFacto = worldState.GetNearestFactory(ETeam.Red, squad.InvisibleLeader.transform.position);
                if(Vector3.Distance(nearestFacto.transform.position, squad.InvisibleLeader.transform.position) > distanceMinFromFactoryToRetreat)
                {
                    TroupNeeded = squad.TargetAttackingSquad.SquadForce - squad.SquadForce;
                    return 1f;
                }
            }
        }

        return 0f;
    }

    public override void OnEnter(UnitController controller, WorldState worldState, Squad squad)
    {
        base.OnEnter(controller, worldState, squad);
        squad.NeedMoreTroups = true;
        squadForceToExit = squad.SquadForce + TroupNeeded;
        squad.SetSquadState(ESquadState.RETREAT);
        squad.SetSquadTarget(nearestFacto.gameObject);
        squad.troupNeeded = TroupNeeded;
    }

    public override void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad)
    {
        base.OnUpdate(updateFrequency, controller, worldState, squad);


    }

    public override void OnExit(UnitController controller, WorldState worldState, Squad squad)
    {
        base.OnExit(controller, worldState, squad);
        squad.NeedMoreTroups = false;
        squad.troupNeeded = 0;

    }
}