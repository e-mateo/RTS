using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "UA_AttackFactory", menuName = "RTS/UtilitySystem/UA_AttackFactory", order = 1)]
public class UA_AttackFactory : UtilityAction
{
    [SerializeField] float TimeBeforeCanAttack = 60f;
    [SerializeField] float DistanceMinimumAttackInExplo = 75f;


    Factory selectedFactory = null;
    public override float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return 0f;

        AIController aIController = controller as AIController;

        if (aIController.StrategicState == EStraticState.DEFENSIVE)
            return 0f;


        if(Time.realtimeSinceStartup < TimeBeforeCanAttack)
            return 0f;

        if(aIController.StrategicState == EStraticState.AGRESSIVE)
        {
            foreach (Squad squadAI in controller.squadList)
            {
                if(squadAI == null || squad == null)
                    continue;

                if(squadAI != squad && squadAI.TargetGameObject && squadAI?.TargetGameObject?.GetComponent<Factory>())
                {
                    selectedFactory = squadAI.TargetGameObject.GetComponent<Factory>();
                    return 0.9f;
                }
            }
        }


        float nearestDistance = float.MaxValue;
        Factory nearest = null;
        foreach (Factory factory in WorldState.Instance.playerFactories)
        {
            float distance = Vector3.Distance(squad.InvisibleLeader.transform.position, factory.transform.position);
            if(distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = factory;
            }
        }

        selectedFactory = nearest;

        if (nearest != null)
        {
            if(aIController.StrategicState == EStraticState.AGRESSIVE)
            {
                return 0.9f;
            }
            else if (aIController.StrategicState == EStraticState.EXPLORATION)
            {
                if (nearestDistance > DistanceMinimumAttackInExplo)
                    return 0f;

                List<Unit> playerUnitsAroundFactory = worldState.GetUnitAroundFactory(nearest, ETeam.Blue);
                int ForceAroundFactory = 0;

                for (int i = 0; i < playerUnitsAroundFactory.Count; i++)
                {
                    if (playerUnitsAroundFactory[i] != null)
                        ForceAroundFactory += playerUnitsAroundFactory[i].GetComponent<Unit>().Cost;
                }

                if (ForceAroundFactory < squad.SquadForce)
                    return 0.9F;
            }
        }

        return 0f;
    }

    public override void OnEnter(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad || !selectedFactory) return;

        squad.SetSquadTarget(selectedFactory.gameObject);
        squad.SetSquadState(ESquadState.ATTACK_FACTORY);
    }

    public override void OnExit(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return;

        squad.SetSquadTarget(null);
    }
}