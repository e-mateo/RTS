
public class CanHeal : DecisionNode
{
    public override bool Evaluate(Unit unit)
    {
        if (unit.GetUnitData.CanRepair)
        {
            foreach (Unit ally in unit.Squad.Units)
            {
                if (ally && ally.NeedsRepairing() && ally != unit)
                    return true;
            }
        }

        return false;
    }
}