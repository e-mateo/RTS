using UnityEngine;

public class AN_PatrolAroundFactory : ActionNode
{
    Factory factory;
    [SerializeField] float randomRadius = 7f;

    override public void OnEnter(Unit unit)
    {
        unit.ShouldMoveInSquad = false;
        factory = unit.Squad.TargetGameObject.GetComponent<Factory>();
        if (factory != null)
        {
            unit.SetTargetPos(GetRandomPosAroundFactory(randomRadius));
        }
    }
    override public void OnUpdate(float frequency, Unit unit)
    {
        if (!factory) return;

        if (unit.NavMeshAgent.isOnNavMesh && unit.NavMeshAgent.remainingDistance < 3f)
        {
            unit.SetTargetPos(GetRandomPosAroundFactory(randomRadius));
        }
    }
    override public void OnExit(Unit unit)
    {
        unit.ShouldMoveInSquad = true;
    }

    private Vector3 GetRandomPosAroundFactory(float radius)
    {
        if(factory == null) 
            return Vector3.zero;

        Vector2 randomCircle = Random.insideUnitCircle * radius;
        Vector3 extent = factory.GetComponent<BoxCollider>().size / 2f;

        return new Vector3(factory.transform.position.x + extent.x + randomCircle.x, factory.transform.position.y, factory.transform.position.z + extent.z + randomCircle.y);
    }
}
