using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Unit : BaseEntity
{
    [SerializeField] bool shouldMoveInSquad = true;

    [SerializeField]
    UnitDataScriptable UnitData = null;

    Transform BulletSlot;
    float LastActionDate = 0f;
    public BaseEntity EntityTarget = null;
    TargetBuilding CaptureTarget = null;
    public Unit UnitTarget = null;

    public NavMeshAgent NavMeshAgent;
    Squad squad;

    [Header("Influence")]
    [SerializeField] private float influence;
    [SerializeField] private float radiusInfluence;

    DecisionTree CurrentDecisionTree;
    [SerializeField] List<DecisionTree> possibleDecisionTrees;

    public bool ShouldMoveInSquad { get { return shouldMoveInSquad; } set { shouldMoveInSquad = value; } }
    public UnitDataScriptable GetUnitData { get { return UnitData; } }
    public int Cost { get { return UnitData.Cost; } }
    public int GetTypeId { get { return UnitData.TypeId; } }
    public float Influence { get { return influence; } }
    public float RadiusInfluence { get { return radiusInfluence; } }
    public Squad Squad { get { return squad; } set { squad = value; } }

    public bool initInSquad;

    public void SwitchDecisionTree(ESquadState squadState)
    {
        if (CurrentDecisionTree)
            CurrentDecisionTree.Desactivate();

        foreach (DecisionTree tree in possibleDecisionTrees)
        {
            if(tree.CanLaunch(squadState))
            {
                CurrentDecisionTree = tree;
                CurrentDecisionTree.Activate();
                return;
            }
        }

    }

    override public void Init(ETeam _team)
    {
        if (IsInitialized)
            return;

        base.Init(_team);

        HP = UnitData.MaxHP;
        OnDeadEvent += Unit_OnDead;
    }

    void Unit_OnDead()
    {
        if (IsCapturing())
            StopCapture();

        if (GetUnitData.DeathFXPrefab)
        {
            GameObject fx = Instantiate(GetUnitData.DeathFXPrefab, transform);
            fx.transform.parent = null;
        }

        squad.RemoveUnit(this);
        Destroy(gameObject);
    }
    #region MonoBehaviour methods
    override protected void Awake()
    {
        base.Awake();

        NavMeshAgent = GetComponent<NavMeshAgent>();
        BulletSlot = transform.Find("BulletSlot");

        // fill NavMeshAgent parameters
        NavMeshAgent.speed = GetUnitData.Speed;
        NavMeshAgent.angularSpeed = GetUnitData.AngularSpeed;
        NavMeshAgent.acceleration = GetUnitData.Acceleration;

        possibleDecisionTrees = GetComponentsInChildren<DecisionTree>(true).ToList();
    }
    override protected void Start()
    {
        // Needed for non factory spawned units (debug)
        if (!IsInitialized)
            Init(Team);

        base.Start();
    }
    override protected void Update()
    {
        if (NavMeshAgent && !NavMeshAgent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(NavMeshAgent.transform.position, out NavMeshHit hit, Mathf.Infinity, NavMesh.AllAreas))
            {
                NavMeshAgent.transform.position = hit.position;
                NavMeshAgent.enabled = false;
                NavMeshAgent.enabled = true;
            }
        }
        // Attack / repair task debug test $$$ to be removed for AI implementation
        if (EntityTarget != null)
        {
            if (EntityTarget.GetTeam() != GetTeam())
                return;// ComputeAttack();
            else
                ComputeRepairing();
        }
	}
    #endregion

    #region IRepairable
    override public bool NeedsRepairing()
    {
        return HP < GetUnitData.MaxHP;
    }
    override public void Repair(int amount)
    {
        HP = Mathf.Min(HP + amount, GetUnitData.MaxHP);
        base.Repair(amount);
    }
    override public void FullRepair()
    {
        Repair(GetUnitData.MaxHP);
    }
    #endregion

    #region Tasks methods : Moving, Capturing, Targeting, Attacking, Repairing ...

    // $$$ To be updated for AI implementation $$$

    // Moving Task
    public void SetTargetPos(Vector3 pos)
    {
        if (shouldMoveInSquad) return;

        if (EntityTarget != null)
            EntityTarget = null;

        if (CaptureTarget != null)
            StopCapture();

        if (NavMeshAgent && NavMeshAgent.isOnNavMesh)
        {
            NavMeshAgent.SetDestination(pos);
            NavMeshAgent.isStopped = false;
        }
    }

    public void SetUnitFormationTarget(Vector3 pos)
    {
        if (!shouldMoveInSquad) return;

        if (EntityTarget != null)
            EntityTarget = null;

        if (CaptureTarget != null)
            StopCapture();

        NavMeshPath path = new NavMeshPath();
        Vector3 PosToSquadLeader = squad.InvisibleLeader.transform.position - pos;
        Vector3 BasePos = pos;

        float lerp = 0.2f;

        if (NavMeshAgent && NavMeshAgent.isOnNavMesh)
        {
            while(!NavMeshAgent.CalculatePath(pos, path) && lerp < 1f)
            {
                Vector3 DirLerp = PosToSquadLeader * lerp;
                pos = BasePos + DirLerp;
                lerp += 0.2f;
            }

            NavMeshAgent.SetDestination(pos);
            NavMeshAgent.isStopped = false;
        }
    }

    // Targetting Task - attack
    public void SetAttackTarget(BaseEntity target)
    {
        if (CanAttack(target) == false)
            return;

        if (CaptureTarget != null)
            StopCapture();

        if (target.GetTeam() != GetTeam())
            StartAttacking(target);
    }

    // Targetting Task - capture
    public void SetCaptureTarget(TargetBuilding target)
    {
        if (CanCapture(target) == false)
            return;

        if (EntityTarget != null)
            EntityTarget = null;

        if (IsCapturing())
            StopCapture();

        if (target.GetTeam() != GetTeam())
            StartCapture(target);
    }

    // Targetting Task - repairing
    public void SetRepairTarget(BaseEntity entity)
    {
        if (CanRepair(entity) == false)
            return;

        if (CaptureTarget != null)
            StopCapture();

        if (entity.GetTeam() == GetTeam())
            StartRepairing(entity);
    }
    public bool CanAttack(BaseEntity target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.AttackDistanceMax * GetUnitData.AttackDistanceMax)
            return false;

        return true;
    }

    public bool IsTargetEnemy()
    {
        if (UnitTarget && UnitTarget.GetTeam() != GetTeam())
            return true;

        return false;
    }

    public bool IsTargetAlly()
    {
        if (UnitTarget && UnitTarget.GetTeam() == GetTeam())
            return true;

        return false;
    }

    // Attack Task
    public void StartAttacking(BaseEntity target)
    {
        EntityTarget = target;
    }
    public void ComputeAttack()
    {
        if (CanAttack(EntityTarget) == false)
            return;

        if (NavMeshAgent && NavMeshAgent.isOnNavMesh)
            NavMeshAgent.isStopped = true;

        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.AttackFrequency)
        {
            LastActionDate = Time.time;
            // visual only ?
            if (UnitData.BulletPrefab)
            {
                GameObject newBullet = Instantiate(UnitData.BulletPrefab, BulletSlot);
                newBullet.transform.parent = null;
                newBullet.GetComponent<Bullet>().ShootToward(EntityTarget.transform.position - transform.position, this);
            }
            // apply damages
            int damages = Mathf.FloorToInt(UnitData.DPS * UnitData.AttackFrequency);
            EntityTarget.AddDamage(damages);
        }
    }
    public bool CanCapture(TargetBuilding target)
    {
        if (target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.CaptureDistanceMax * GetUnitData.CaptureDistanceMax)
            return false;

        return true;
    }

    // Capture Task
    public void StartCapture(TargetBuilding target)
    {
        if (CanCapture(target) == false)
            return;

        if (NavMeshAgent && NavMeshAgent.isOnNavMesh)
            NavMeshAgent.isStopped = true;

        CaptureTarget = target;
        CaptureTarget.StartCapture(this);
    }
    public void StopCapture()
    {
        if (CaptureTarget == null)
            return;

        CaptureTarget.StopCapture(this);
        CaptureTarget = null;
    }

    public bool IsCapturing()
    {
        return CaptureTarget != null;
    }

    // Repairing Task
    public bool CanRepair(BaseEntity target)
    {
        if (GetUnitData.CanRepair == false || target == null)
            return false;

        // distance check
        if ((target.transform.position - transform.position).sqrMagnitude > GetUnitData.RepairDistanceMax * GetUnitData.RepairDistanceMax)
            return false;

        return true;
    }
    public void StartRepairing(BaseEntity entity)
    {
        if (GetUnitData.CanRepair)
        {
            EntityTarget = entity;
        }
    }

    // $$$ TODO : add repairing visual feedback
    public void ComputeRepairing()
    {
        if (CanRepair(EntityTarget) == false)
            return;

        if (NavMeshAgent && NavMeshAgent.isOnNavMesh)
            NavMeshAgent.isStopped = true;

        transform.LookAt(EntityTarget.transform);
        // only keep Y axis
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0f;
        eulerRotation.z = 0f;
        transform.eulerAngles = eulerRotation;

        if ((Time.time - LastActionDate) > UnitData.RepairFrequency)
        {
            LastActionDate = Time.time;

            // apply reparing
            int amount = Mathf.FloorToInt(UnitData.RPS * UnitData.RepairFrequency);
            EntityTarget.Repair(amount);
        }
    }
    #endregion
}
