using FSMMono;


public class ExplorationState : StateBehavior
{
    public void SetExplorationState()
    {
        Controller.StrategicState = EStraticState.EXPLORATION;
    }
}

