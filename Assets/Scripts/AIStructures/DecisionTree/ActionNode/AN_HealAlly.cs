public class AN_HealAlly : ActionNode
{
    override public void OnEnter(Unit unit)
    {
        unit.ShouldMoveInSquad = false;
        unit.EntityTarget = unit.UnitTarget;
    }

    override public void OnUpdate(float frequency, Unit unit)
    {
        if (!unit.UnitTarget) return;

        if(unit.NavMeshAgent.isOnNavMesh)
            unit.NavMeshAgent.destination = unit.UnitTarget.transform.position;

        if (unit.CanRepair(unit.UnitTarget))
        {
            unit.ComputeRepairing();
            if (unit.NavMeshAgent.isOnNavMesh)
                unit.NavMeshAgent.isStopped = true;
        }
        else
        {
            if (unit.NavMeshAgent.isOnNavMesh)
                unit.NavMeshAgent.isStopped = false;
        }
    }

    override public void OnExit(Unit unit)
    {
        unit.ShouldMoveInSquad = true;
    }
}