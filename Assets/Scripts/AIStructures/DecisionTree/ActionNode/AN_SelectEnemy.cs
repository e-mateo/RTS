using UnityEngine;

public class AN_SelectEnemy : ActionNode
{
    override public void OnEnter(Unit unit)
    {
        Unit nearestUnit = FindNearestUnit(unit);
        if (nearestUnit != null)
        {
            unit.UnitTarget = nearestUnit;
        }
    }

    override public void OnUpdate(float frequency, Unit unit)
    {
        Unit nearestUnit = FindNearestUnit(unit);
        if (nearestUnit != null)
        {
            unit.UnitTarget = nearestUnit;
        }
    }

    public Unit FindNearestUnit(Unit unit)
    {
        float minDistance = float.MaxValue;
        Unit nearestUnit = null;
        if (!unit.Squad.TargetAttackingSquad)
            return null;

        foreach (Unit targetUnit in unit.Squad.TargetAttackingSquad.Units)
        {
            if(!targetUnit)
                continue;

            float distance = Vector3.Distance(transform.position, targetUnit.transform.position);
            if (distance < minDistance)
            {
                nearestUnit = targetUnit;
                minDistance = distance;
            }
        }
        return nearestUnit;
    }
}