using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cond_ToExploration", menuName = "RTS/FSM/Conditions/Cond_ToExploration", order = 1)]
public class Cond_ToExploration : Condition
{
    [SerializeField][Range(0f, 1f)] float percentLessPowerThanPlayer;
    [SerializeField][Range(0f, 1f)] float percentLessLabThanPlayer = 0.75f;


    override public void Init(WorldState worldState)
    {
        base.Init(worldState);
    }

    override public bool UpdateCondition(float updateFrequency, WorldState worldState)
    {
        if((GameServices.GetControllerByTeam(ETeam.Red) as AIController).StrategicState == EStraticState.DEFENSIVE)
        {
            foreach (Factory AIFactory in worldState.AIFactories)
            {
                if (AIFactory)
                {
                    List<Unit> playerUnitAroundFactory = worldState.GetUnitAroundFactory(AIFactory, ETeam.Blue);
                    if (playerUnitAroundFactory != null && playerUnitAroundFactory.Count > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        else if((GameServices.GetControllerByTeam(ETeam.Red) as AIController).StrategicState == EStraticState.AGRESSIVE)
        {
            if ((float)worldState.ComputeEnemyPower() < (float)worldState.ComputeAllyPower() * percentLessPowerThanPlayer || worldState.enemyLabs.Count < worldState.allyLabs.Count * percentLessLabThanPlayer)
            {
                return true;
            }
        }


        return false;
    }
}