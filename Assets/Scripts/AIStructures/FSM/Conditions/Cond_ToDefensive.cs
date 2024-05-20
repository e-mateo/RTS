using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cond_ToDefensive", menuName = "RTS/FSM/Conditions/Cond_ToDefensive", order = 1)]

public class Cond_ToDefensive : Condition
{
    //Go to defensive state if our power is less important than the enemy and if we already have enough building to farm
    override public void Init(WorldState worldState)
    {
        base.Init(worldState);
    }

    override public bool UpdateCondition(float updateFrequency, WorldState worldState) 
    {
        //If the player really more power than the enemy don't go to defense state
        if (worldState.ComputeEnemyPower() > 1.5f * worldState.ComputeAllyPower())
            return false;

        //If some factories are under attack go to defense state
        foreach (Factory AIFactory in worldState.AIFactories)
        {
            if(AIFactory != null)
            {
                List<Unit> playerUnitAroundFactory = worldState.GetUnitAroundFactory(AIFactory, ETeam.Blue);
                if (playerUnitAroundFactory != null && playerUnitAroundFactory.Count > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public float ComputePlayerPowerAroundFactory(List<Unit> playerUnitAroundFactory)
    {
        if (playerUnitAroundFactory == null || playerUnitAroundFactory.Count == 0)
            return 0f;

        float power = 0;
        foreach (Unit unit in playerUnitAroundFactory)
        {
            if(unit)
                power += unit.Cost;
        }
        return power;
    }
}
