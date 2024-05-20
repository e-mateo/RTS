using UnityEngine;

[CreateAssetMenu(fileName = "UA_AttackAIGroup", menuName = "RTS/UtilitySystem/UA_AttackAIGroup", order = 1)]
public class UA_AttackAIGroup : UtilityAction
{
    [SerializeField] float detectionRadius = 20f;
    ESquadState previousState;
    GameObject previousSquadTarget;
    Vector3 previousSquadTargetPos;
    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        if(squad == null) return 0f;

        if(squad.bLockAttack || squad.SquadState == ESquadState.ATTACK_FACTORY)
        {
            return 0f;
        }

        if (squad.SquadState == ESquadState.ATTACK_SQUAD)
        {
            if (squad.TargetAttackingSquad == null)
                return 0f;

            if (squad.TargetAttackingSquad.Units.Count > 0 && Vector3.Distance(squad.TargetAttackingSquad.InvisibleLeader.transform.position, squad.InvisibleLeader.transform.position) < detectionRadius * 2)
                return 0.95f;
            return 0f;
        }
        Squad nearestSquad = worldState.GetNearestSquad(squad, out float DistanceNearestSquad);
        if (worldState.influenceMap.AreThereEnemiesAround(ETeam.Red, detectionRadius, squad.InvisibleLeader.position, out float distanceFromEnemy) || DistanceNearestSquad < detectionRadius) 
        {
            return 0.95f;
        }

        return 0f;
    }

    public override void OnEnter(UnitController controller, WorldState worldState, Squad squad)
    {
        if(squad == null) return;

        previousState = squad.SquadState;
        squad.SquadState = ESquadState.ATTACK_SQUAD;
        squad.TargetAttackingSquad = worldState.GetNearestSquad(squad, out float distance);

        if (squad.TargetAttackingSquad == null) return;

        previousSquadTarget = squad.TargetGameObject;
        if (!previousSquadTarget)
            previousSquadTargetPos = squad.TargetLocation;

        squad.SetSquadTarget(squad.TargetAttackingSquad.InvisibleLeader.gameObject);
        squad.UpdateUnitSquadDecisionTree();
    }

    public override void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad)
    {
        if (squad == null || squad.TargetAttackingSquad == null) return;

        if (squad.SquadForce < squad.TargetAttackingSquad.IncomingForce)
        {
            squad.NeedMoreTroups = true;
        }
        else
        {
            squad.NeedMoreTroups = false;
        }
    }

    public override void OnExit(UnitController controller, WorldState worldState, Squad squad)
    {
        if (squad == null) return;

        squad.NeedMoreTroups = false;

        squad.TargetAttackingSquad = null;
        if(previousSquadTarget)
            squad.SetSquadTarget(previousSquadTarget);
        else
            squad.SetSquadTarget(previousSquadTargetPos);
        squad.SquadState = previousState;
        squad.UpdateUnitSquadDecisionTree();
    }
}
