
public class AN_CaptureLab : ActionNode
{
    TargetBuilding Lab;

    override public void OnEnter(Unit unit)
    {
        unit.ShouldMoveInSquad = false;
        Lab = unit.Squad?.TargetGameObject?.GetComponent<TargetBuilding>();
        if (Lab != null)
        {
            unit.SetTargetPos(Lab.transform.position);
        }
    }

    override public void OnUpdate(float frequency, Unit unit)
    {
        if (!Lab) return;

        if (!unit.IsCapturing() && unit.NavMeshAgent.isOnNavMesh && unit.NavMeshAgent.remainingDistance < 5f)
        {
            unit.SetCaptureTarget(Lab);
        }
    }

    override public void OnExit(Unit unit)
    {
        unit.ShouldMoveInSquad = true;
    }
}