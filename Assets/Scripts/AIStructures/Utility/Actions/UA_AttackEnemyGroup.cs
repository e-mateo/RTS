using UnityEngine;

[CreateAssetMenu(fileName = "UA_AttackEnemyGroup", menuName = "RTS/UtilitySystem/UA_AttackEnemyGroup", order = 1)]
public class UA_AttackEnemyGroup : UtilityAction
{
    [SerializeField] float detectionRadius = 20f;
    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad) 
    {
        if (!squad) return 0f;

        AIController aIController = controller as AIController;
       

        if (squad.SquadState == ESquadState.ATTACK_SQUAD)
        {
            if (squad.TargetAttackingSquad == null)
                return 0f;

            if (squad.TargetAttackingSquad.Units.Count > 0 && Vector3.Distance(squad.TargetAttackingSquad.InvisibleLeader.transform.position, squad.InvisibleLeader.transform.position) < detectionRadius * 2)
                return 1f;

            return 0f;
        }

        Squad nearestSquad = worldState.GetNearestSquad(squad, out float DistanceNearestSquad);
        if (worldState.influenceMap.AreThereEnemiesAround(ETeam.Blue, detectionRadius, squad.InvisibleLeader.position, out float distanceFromEnemy) || DistanceNearestSquad < detectionRadius)
        {
            return 1f;
        }

        return 0f;
    }

    public override void OnEnter(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return;

        squad.TargetAttackingSquad = worldState.GetNearestSquad(squad, out float distance);
        squad.SetSquadTarget(squad.TargetAttackingSquad.InvisibleLeader.gameObject);
        squad.SetSquadState(ESquadState.ATTACK_SQUAD);
    }

    public override void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad)
    {

    }

    public override void OnExit(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return;

        squad.SetSquadTarget(null);
        squad.TargetAttackingSquad = null;
        squad.NeedMoreTroups = false;
    }
}