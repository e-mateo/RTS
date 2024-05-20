using UnityEngine;

[CreateAssetMenu(fileName = "UA_CreateFactory", menuName = "RTS/UtilitySystem/UA_CreateFactory", order = 1)]
public class UA_CreateFactory : UtilityAction
{
    [SerializeField] float UnitPowerNeededToBuild = 10f;
    [SerializeField] float MultiplierUnitPowerNeededByNumberFactory = 15f;
    [SerializeField] float TimeBetweenFactoryCreation = 3f;
    [SerializeField] float TimeBetweenFactoryTryCreation = 2f;

    float Timer = 0;

    float TimerFocusBuilding = 120;
    float LastTimeConstruct = 0;
    bool isHeavyHQ = false;


    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        AIController aIController = controller as AIController;

        if (!aIController.HasPossibleFactoryLocations)
            return 0;   

        if (controller.GetFactoryList.Count == 1)
            return 1.0f;

        if(Time.time - LastTimeConstruct > TimerFocusBuilding && aIController.StrategicState != EStraticState.AGRESSIVE)
            return 1.0f;

        int power = 0;
        foreach (Squad squadInList in controller.squadList)
        {
            if(squadInList != null) 
                power += squadInList.SquadForce;
        }
        if (power == 0)
            return 0;

        float morePointNeeded = (float)(controller.GetFactoryList.Count - 1) * MultiplierUnitPowerNeededByNumberFactory;
        if (morePointNeeded == 0)
            morePointNeeded = 1;

        if (power / (UnitPowerNeededToBuild + morePointNeeded) >= 1f)
            return aIController.StrategicState == EStraticState.AGRESSIVE ? 0.5f : 1.0f;

        return 0f;
    }

    override public void OnEnter(UnitController controller, WorldState worldState, Squad squad) 
    {
        Timer = 0f;
    }
    override public void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad) 
    {
        AIController aIController = controller as AIController;

        Timer -= updateFrequency;

        if (worldState.GetLightFactory(ETeam.Red).Count / (worldState.GetHeavyFactory(ETeam.Red).Count + 1) >= 2)
            isHeavyHQ = true;
        else
            isHeavyHQ = false;

        if (Timer <= 0)
        {
            if(aIController.CreateNewFactory(isHeavyHQ))
            {

                Timer = TimeBetweenFactoryCreation;
                LastTimeConstruct = Time.time;
            }
            else
            {
                Timer = TimeBetweenFactoryTryCreation;
            }
        }
    }
    override public void OnExit(UnitController controller, WorldState worldState, Squad squad) 
    {

    }
}


