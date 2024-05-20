using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ESquadState
{ 
    NONE,
    EXPLORATION,
    CAPTURE_LAB,
    ATTACK_FACTORY,
    DEFEND_FACTORY,
    ATTACK_SQUAD,
    RETREAT,
}


public class Squad : MonoBehaviour
{
    [Header("SquadData")]
    bool needMoreTroups = false;
    public int troupNeeded;
    bool initialized = false;
    [HideInInspector] public bool bLockAttack = false;
    int squadForce;
    [HideInInspector] public int IncomingForce = 0;
    [HideInInspector] public int numberOfTroupsAtBeginning = 0;
    float targetSpeed;
    float distanceMinimumRetreat = 100f;
    float timerLockAttack = 2f;
    float currentTimerLockAttack;
    ESquadState squadState;
    ETeam squadTeam;
    [HideInInspector] public Squad TargetAttackingSquad;
    List<Unit> units = new List<Unit>();


    [Header("Controllers")]
    UtilitySystem utilitySystem;
    public UnitController controller;


    [Header("Movement")]
    Vector3 targetLocation;
    Transform invisibleLeader;
    GameObject targetGameObject;
    NavMeshAgent navMeshAgentLeader;
    Formation formation = null;
    [SerializeField] List<Formation> possibleFormations;

    public bool NeedMoreTroups { get { return needMoreTroups; } set { needMoreTroups = value; } }
    public bool IsInitiliazed { get { return initialized; } }
    public int SquadForce { get { return squadForce + IncomingForce; } }
    public ESquadState SquadState { get { return squadState; } set { squadState = value; } }
    public Vector3 TargetLocation { get { return targetLocation; } }
    public List<Unit> Units { get { return units; } }
    public Transform InvisibleLeader { get { return invisibleLeader; } }
    public GameObject TargetGameObject { get { return targetGameObject; } }
    public ETeam SquadTeam { get { return squadTeam; } set { squadTeam = value; } }

    #region MonoBehavior
    private void Awake()
    {
        for(int i = 0; i < possibleFormations.Count; i++)
        {
            if (possibleFormations[i])
                possibleFormations[i] = Instantiate<Formation>(possibleFormations[i]); //Instanciate Scriptable Objects
        }

        formation = possibleFormations[Random.Range(0, possibleFormations.Count)];
        SquadState = ESquadState.NONE;
        invisibleLeader = transform.GetChild(0).transform;
        utilitySystem = GetComponent<UtilitySystem>();
        utilitySystem.enabled = false;
    }
    void Start()
    {
        navMeshAgentLeader = invisibleLeader.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (navMeshAgentLeader && !navMeshAgentLeader.isOnNavMesh)
        {
            if(NavMesh.SamplePosition(navMeshAgentLeader.transform.position, out NavMeshHit hit, Mathf.Infinity, NavMesh.AllAreas))
            {
                navMeshAgentLeader.transform.position = hit.position;
                navMeshAgentLeader.enabled = false;
                navMeshAgentLeader.enabled = true;
            }
        }

        if (!formation)
            return;

        SetFormationTargetPositionToSquadUnit();
        UpdateFormationSpeed();

        if(currentTimerLockAttack > 0f)
        {
            currentTimerLockAttack -= Time.deltaTime;
            if(currentTimerLockAttack < 0f)
            {
                bLockAttack = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (controller)
            WorldState.Instance?.RemoveSquad(this, controller.GetTeam());
    }
    #endregion

    #region SquadMethod

    public void AddUnit(Unit unit)
    {
        if(units.Count == 0)
        {
            NavMesh.SamplePosition(unit.transform.position, out NavMeshHit hit, Mathf.Infinity, NavMesh.AllAreas);
            GameObject newInvisibleLeader = Instantiate(invisibleLeader.gameObject, hit.position, unit.transform.rotation, transform);
            Destroy(invisibleLeader.gameObject);
            invisibleLeader = newInvisibleLeader.transform;
            navMeshAgentLeader = invisibleLeader.GetComponent<NavMeshAgent>();

        }

        units.Add(unit);
        unit.Squad = this;
        unit.transform.SetParent(transform, true);
        unit.SwitchDecisionTree(squadState);
        unit.NavMeshAgent.enabled = true;
        unit.initInSquad = false;


        squadForce += unit.GetUnitData.Cost;
        if(IncomingForce > 0)
        {
            IncomingForce -= unit.GetUnitData.Cost;
            if (IncomingForce < 0)
                IncomingForce = 0;
        }
        controller.Power += unit.GetUnitData.Cost;

        formation.UpdateFormation(units.Count);
        UpdateFormationTargetSpeed();

        if(!initialized && units.Count >= numberOfTroupsAtBeginning && controller is AIController)
        {
            initialized = true;
            utilitySystem.enabled = true;
        }
    }

    public void RemoveUnit(Unit unit) 
    {
        units.Remove(unit);

        UpdateFormationTargetSpeed();

        squadForce -= unit.GetUnitData.Cost;
        controller.Power -= unit.GetUnitData.Cost;

        if (utilitySystem && utilitySystem.Controller)
            utilitySystem.Controller.Power = squadForce;

        if (units.Count <= 0)
            Destroy(this.gameObject);

        if (!initialized)
        {
            initialized = true;
            utilitySystem.enabled = true;
        }
    }

    public void SetController(UnitController controller)
    {
        if(!controller)
            return;

        this.controller = controller;
        WorldState.Instance.AddSquad(this, controller.GetTeam());
        utilitySystem.Controller = controller;
        if (controller is PlayerController)
        {
            utilitySystem.enabled = true;
            initialized = true;
        }
    }

    public void UpdateUnitSquadDecisionTree()
    {
        foreach (Unit unit in units)
        {
            unit?.SwitchDecisionTree(squadState);
        }
    }
    public void SetSquadState(ESquadState State)
    {
        if (controller is PlayerController && squadState == ESquadState.ATTACK_SQUAD)
        {
            //Lock attack to make player units retreat if they were fighting and that the selected location is far enough
            if (Vector3.Distance(targetLocation, invisibleLeader.transform.position) > distanceMinimumRetreat)
            {
                bLockAttack = true;
                currentTimerLockAttack = timerLockAttack;
                Debug.Log("Lock Attack");
            }
        }

        squadState = State;
        UpdateUnitSquadDecisionTree();
    }

    #endregion

    #region MovementMethod
    public void SetSquadTarget(Vector3 Target)
    {
        targetLocation = Target;

        if (navMeshAgentLeader.isOnNavMesh)
        {
            navMeshAgentLeader.isStopped = false;
            navMeshAgentLeader.destination = targetLocation;
        }
    }

    public void SetSquadTarget(GameObject Target)
    {
        targetGameObject = Target;

        if(targetGameObject != null)
        {
            targetLocation = Target.transform.position;
            if (navMeshAgentLeader.isOnNavMesh)
            {
                navMeshAgentLeader.isStopped = false;
                navMeshAgentLeader.destination = targetLocation;
            }
        }
        else
        {
            targetLocation = Vector3.zero;
            if (navMeshAgentLeader.isOnNavMesh)
            {
                navMeshAgentLeader.isStopped = true;
                navMeshAgentLeader.destination = targetLocation;
            }
        }
    }

    private void SetFormationTargetPositionToSquadUnit()
    {
        if (invisibleLeader == null && units.Count > 0 && units[0])
        {
            NavMesh.SamplePosition(units[0].transform.position, out NavMeshHit hit, Mathf.Infinity, NavMesh.AllAreas);
            GameObject newInvisibleLeader = Instantiate(invisibleLeader.gameObject, hit.position, units[0].transform.rotation, transform);
            Destroy(invisibleLeader.gameObject);
            invisibleLeader = newInvisibleLeader.transform;
            navMeshAgentLeader = invisibleLeader.GetComponent<NavMeshAgent>();
        }

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] && units[i].ShouldMoveInSquad)
                units[i]?.SetUnitFormationTarget(formation.GetWorldSlot(invisibleLeader, i));
        }
    }

    public bool HasReachedTarget(float radiusValidateTarget)
    {
        if(!navMeshAgentLeader.isOnNavMesh)
            return false;

        return (navMeshAgentLeader.remainingDistance < radiusValidateTarget);
    }



    private void UpdateFormationTargetSpeed()
    {
        float minSpeed = float.MaxValue;
        foreach(Unit unit in units)
        {
            if(unit && unit.GetUnitData.Speed < minSpeed)
            {
                minSpeed = unit.GetUnitData.Speed;
            }
        }

        targetSpeed = minSpeed;
    }

    private void UpdateFormationSpeed()
    {
        float speed = targetSpeed;
        float maxDistance = 5f;
        for(int i = 0; i < units.Count;i++)
        {
            if (units[i] == null)
                continue;

            float distance = Vector3.Distance(units[i].transform.position, units[i].NavMeshAgent.destination);
            if (distance > maxDistance && units[i].initInSquad)
            {
                maxDistance = distance;
                speed = Mathf.Lerp(0, targetSpeed, 5f / maxDistance);
            }
            else if (distance < maxDistance && !units[i].initInSquad)
            {
                units[i].initInSquad = true;
                Debug.Log("initialized");
            }
        }

        navMeshAgentLeader.speed = speed;
    }

    public void SetFormation(int formationID)
    {
        if (formationID < possibleFormations.Count)
            formation = possibleFormations[formationID];

        formation?.UpdateFormation(units.Count);
    }

    #endregion


    #region Gizmo

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < units.Count; i++)
        {
            Gizmos.DrawCube(units[i].NavMeshAgent.destination, Vector3.one * 4);
        }

        Gizmos.color = Color.red;
        if(invisibleLeader)
         Gizmos.DrawSphere(invisibleLeader.transform.position, 2f);
    }

    #endregion

}