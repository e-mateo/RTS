using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UA_CreateUnit", menuName = "RTS/UtilitySystem/UA_CreateUnit", order = 1)]
public class UA_CreateUnit : UtilityAction
{
    [SerializeField] int MinPointToCreateHeavyTroups = 10;
    [SerializeField] float UnitPowerNeededToBuild = 10f;
    [SerializeField] float MultiplierUnitPowerNeededByNumberFactory = 2.5f;
    [SerializeField] float TimeBetweenTwoSquadCreation = 5f;

    float Timer;
    float distanceLetEnemyGo = 40f;
    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        foreach (Squad AISquad in WorldState.Instance.enemySquads)
        {
            if(AISquad == null || AISquad.TargetGameObject == null) continue;

            if (AISquad.NeedMoreTroups && Vector3.Distance(AISquad.InvisibleLeader.transform.position, AISquad.TargetGameObject.transform.position) < distanceLetEnemyGo)
                return 1.0f;
        }

        return 0f;
    }

    public override void OnEnter(UnitController controller, WorldState worldState, Squad squad)
    {
        base.OnEnter(controller, worldState, squad);
        Timer = 0f;
    }

    public override void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad = null)
    {
        AIController aIController = controller as AIController;

        List<Squad> squadNeedTroups = new List<Squad>();
        foreach (Squad AISquad in WorldState.Instance.enemySquads)
        {
            if (AISquad && AISquad.NeedMoreTroups)
            {
                if(controller.TotalBuildPoints < 5)
                {
                    AISquad.NeedMoreTroups = false; //Reset to not get block with 0 build point and all squads wanting to get troups and doing nothing
                    AISquad.troupNeeded = 0; 
                }
                else
                {
                    squadNeedTroups.Add(AISquad);
                }
            }

        }


        foreach (Squad AISquad in squadNeedTroups)
        {
            if (AISquad == null) continue;

            int forceAdded = 0;
            int pointToUse = AISquad.troupNeeded;

            if (aIController.StrategicState == EStraticState.EXPLORATION)
                forceAdded = aIController.FillSquad(AISquad, pointToUse, 0.1f);
            else if (aIController.StrategicState == EStraticState.AGRESSIVE)
                forceAdded = aIController.FillSquad(AISquad, pointToUse, 0.5f);
            else if (aIController.StrategicState == EStraticState.DEFENSIVE)
                forceAdded = aIController.FillSquad(AISquad, pointToUse, 0.3f);

            AISquad.troupNeeded -= forceAdded;
        }
    }
}
