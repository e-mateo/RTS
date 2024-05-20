
public class CanRepairFactory : DecisionNode
{
    public override bool Evaluate(Unit unit)
    {
        Factory factory = unit.Squad?.TargetGameObject?.GetComponent<Factory>();

        if (factory == null)
            return false;

        if (unit.GetUnitData.CanRepair && factory.NeedsRepairing())
        {
            if (factory && factory.NeedsRepairing())
                return true;
        }

        return false;
    }
}
