
public class AN_RepairFactory : ActionNode
{
    override public void OnEnter(Unit unit)
    {
        unit.ShouldMoveInSquad = false;
        unit.EntityTarget = unit.Squad.TargetGameObject.GetComponent<Factory>();
        unit.NavMeshAgent.destination = unit.UnitTarget.transform.position;
    }

    override public void OnUpdate(float frequency, Unit unit)
    {
        if (!unit.UnitTarget) return;

        if (unit.CanRepair(unit.UnitTarget))
        {
            unit.ComputeRepairing();
            if(unit.NavMeshAgent.isOnNavMesh)
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

        if (unit.NavMeshAgent.isOnNavMesh)
            unit.NavMeshAgent.isStopped = false;
    }
}
