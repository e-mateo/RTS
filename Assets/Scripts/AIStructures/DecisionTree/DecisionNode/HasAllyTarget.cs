
public class HasAllyTarget : DecisionNode
{
    public override bool Evaluate(Unit unit)
    {
        return unit.IsTargetAlly();
    }
}