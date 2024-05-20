using UnityEngine;

public class AN_SelectAlly : ActionNode
{
    override public void OnEnter(Unit unit)
    {
        Unit nearestUnit = FindNearestUnit(unit);

        if (nearestUnit != null)
            unit.UnitTarget = nearestUnit;
    }

    override public void OnUpdate(float frequency, Unit unit)
    {
        Unit nearestUnit = FindNearestUnit(unit);

        if (nearestUnit != null)
            unit.UnitTarget = nearestUnit;
    }

    public Unit FindNearestUnit(Unit unit)
    {
        float minDistance = float.MaxValue;
        Unit nearestUnit = null;
        foreach (Unit ally in unit.Squad.Units)
        {
            if (!ally || !ally.NeedsRepairing())
                continue;

            float distance = Vector3.Distance(transform.position, ally.transform.position);
            if (distance < minDistance)
            {
                nearestUnit = ally;
                minDistance = distance;
            }
        }
        return nearestUnit;
    }
}