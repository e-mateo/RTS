
public class HasEnemyTarget : DecisionNode
{
    public override bool Evaluate(Unit unit)
    {
        return unit.IsTargetEnemy();
    }
}