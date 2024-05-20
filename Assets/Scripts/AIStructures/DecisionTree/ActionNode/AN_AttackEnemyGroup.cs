
public class AN_AttackEnemyGroup : ActionNode
{
    override public void OnEnter(Unit unit)
    {
        unit.ShouldMoveInSquad = false;
        unit.EntityTarget = unit.UnitTarget;
    }

    override public void OnUpdate(float frequency, Unit unit)
    {
        if (!unit.UnitTarget || !unit.NavMeshAgent.isOnNavMesh) return;

        unit.NavMeshAgent.destination = unit.UnitTarget.transform.position;

        if (unit.CanAttack(unit.UnitTarget))
        {
            unit.ComputeAttack();
            unit.NavMeshAgent.isStopped = true;
        }
        else
        {
            unit.NavMeshAgent.isStopped = false;
        }
    }

    override public void OnExit(Unit unit)
    {
        unit.ShouldMoveInSquad = true;
    }
}