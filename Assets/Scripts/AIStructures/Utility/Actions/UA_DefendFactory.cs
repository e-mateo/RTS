using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UA_DefendFactory", menuName = "RTS/UtilitySystem/UA_DefendFactory", order = 1)]
public class UA_DefendFactory : UtilityAction
{
    Factory selectedFactory = null;
    public override float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return 0f;

        AIController aIController = controller as AIController;

        float multiplierStrategic = 1f;
        switch (aIController.StrategicState)
        {
            case EStraticState.EXPLORATION:
                multiplierStrategic = 0.8f;
                break;
            case EStraticState.AGRESSIVE:
                multiplierStrategic = 0.6f;
                break;
            case EStraticState.DEFENSIVE:
                multiplierStrategic = 0.9f;
                break;
        }

        float biggerDifferencePower = 0;

        foreach (Factory AIFactory in worldState.AIFactories)
        {
            List<Squad> AIProtecting = AIFactory.AIProtectingFactory;
            List<Unit> playerUnitAroundFactory = worldState.GetUnitAroundFactory(AIFactory, ETeam.Blue);
            float PowerPlayer = ComputePlayerPowerAroundFactory(playerUnitAroundFactory);
            float PowerAI = ComputeAIPowerAroundFactory(AIProtecting);

            if(PowerPlayer > 0 && PowerAI < PowerPlayer)
            {
                if(!selectedFactory || (PowerPlayer - PowerAI) > biggerDifferencePower)
                {
                    selectedFactory = AIFactory;
                    biggerDifferencePower = PowerPlayer - PowerAI;
                }
            }
        }

        if (selectedFactory)
            return multiplierStrategic;

        return 0f;
    }

    public float ComputePlayerPowerAroundFactory(List<Unit> playerUnitAroundFactory)
    {
        if(playerUnitAroundFactory == null || playerUnitAroundFactory.Count == 0)
            return 0f;

        float power = 0;
        foreach (Unit unit in playerUnitAroundFactory)
        {
            if(unit != null)
                power += unit.Cost;
        }
        return power;
    }


    public float ComputeAIPowerAroundFactory(List<Squad> AIProtecting)
    {
        float power = 0;
        foreach (Squad squad in AIProtecting)
        {
            if(squad != null)
                power += squad.SquadForce;
        }
        return power;
    }


    public override void OnEnter(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return;

        squad.SetSquadTarget(selectedFactory?.gameObject);
        squad.SetSquadState(ESquadState.DEFEND_FACTORY);
    }

    public override void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return;

    }

    public override void OnExit(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return;

        squad.SetSquadTarget(null);
        selectedFactory = null;
    }
}
