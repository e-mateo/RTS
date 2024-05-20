using UnityEngine;

[CreateAssetMenu(fileName = "Cond_ToAgressive", menuName = "RTS/FSM/Conditions/Cond_ToAgressive", order = 1)]
public class Cond_ToAgressive : Condition
{
    [SerializeField] int minPower;
    [SerializeField] float percentLabWorld = 0.33f;

    public override void Init(WorldState worldState)
    {
        
    }

    public override bool UpdateCondition(float updateFrequency, WorldState worldState)
    {
        float power = worldState.ComputeEnemyPower();

        //Has more power than the player and control enough labs
        if (power > minPower && power > (float)worldState.ComputeAllyPower() * 1.2f)
        {
            if((float)worldState.enemyLabs.Count > percentLabWorld * (float)worldState.labs.Count)
                return true;
        }

        return false;
    }
}