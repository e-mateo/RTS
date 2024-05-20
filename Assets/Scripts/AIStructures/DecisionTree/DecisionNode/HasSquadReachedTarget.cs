using UnityEngine;

public class HasSquadReachedTarget : DecisionNode
{
    [SerializeField] float radiusValidateTarget = 10f;
    public override bool Evaluate(Unit unit)
    {
        if(unit.Squad == null)
            return false;

        return unit.Squad.HasReachedTarget(radiusValidateTarget);
    }
}