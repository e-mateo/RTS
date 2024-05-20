using FSMMono;


public class DefensiveState : StateBehavior
{
    public void SetDefensiveState()
    {
        Controller.StrategicState = EStraticState.DEFENSIVE;
    }

}
