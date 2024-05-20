using UnityEngine;

public class AN_DestroyFactory : ActionNode
{
    Factory factory;
    bool bIsAttacking = false;

    override public void OnEnter(Unit unit)
    {
        unit.ShouldMoveInSquad = false;
        factory = unit.Squad?.TargetGameObject?.GetComponent<Factory>();
        if(factory != null )
        {
            Vector2 offSetTarget = Random.insideUnitCircle * unit.GetUnitData.AttackDistanceMax;
            Vector3 extent = factory.GetComponent<BoxCollider>().size / 2f;
            unit.SetTargetPos(new Vector3(offSetTarget.x + extent.x, 0, offSetTarget.y + extent.z) + factory.transform.position);
        }
    }
    override public void OnUpdate(float frequency, Unit unit)
    {
        if (factory == null) return;

        if(!bIsAttacking && unit.CanAttack(factory))
        {
            unit.SetAttackTarget(factory);
            if(unit.NavMeshAgent.isOnNavMesh)
                unit.NavMeshAgent.isStopped = false;
            bIsAttacking = true;
        }
        else if(bIsAttacking)
        {
            unit.ComputeAttack();
        }
    }
    override public void OnExit(Unit unit)
    {
        unit.ShouldMoveInSquad = true;
        unit.SetAttackTarget(null);
        bIsAttacking = false;
        factory = null;
    }
}
