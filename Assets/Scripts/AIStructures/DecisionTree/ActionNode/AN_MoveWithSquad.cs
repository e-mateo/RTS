
public class AN_MoveWithSquad : ActionNode
{
    override public void OnEnter(Unit unit)
    {
        unit.ShouldMoveInSquad = true;
    }

    override public void OnUpdate(float frequency, Unit unit) 
    {

    }

    override public void OnExit(Unit unit) 
    { 

    }
}
