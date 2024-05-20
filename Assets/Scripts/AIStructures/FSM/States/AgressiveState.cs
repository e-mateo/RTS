using FSMMono;


public class AgressiveState : StateBehavior
{
    public void SetAgressiveState()
    {
        Controller.StrategicState = EStraticState.AGRESSIVE;
    }
}
