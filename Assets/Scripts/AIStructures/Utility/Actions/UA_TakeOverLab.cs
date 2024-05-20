using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UA_TakeOverLab", menuName = "RTS/UtilitySystem/UA_TakeOverLab", order = 1)]
public class UA_TakeOverLab : UtilityAction
{
    [SerializeField] float minRadiusRandomIntermediate = 5f;
    [SerializeField] float maxRadiusRandomIntermediate = 20f;
    [SerializeField] float distanceThreshold = 10f;

    bool useIntermediatePos;
    bool hasReachedIntermediate;

    TargetBuilding targetLab;

    Vector3 lab2squad;
    Vector3 intermediatePos;

    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return 0f;

        AIController aIController = controller as AIController;

        if (targetLab && targetLab.GetTeam() == ETeam.Red)
            return 0f;

        if (worldState.labs.Count > worldState.allyLabs.Count + worldState.enemyLabs.Count) //There are still unused labs
        {
            return 0.8f;
        }
        else if (worldState.allyLabs.Count > 0)
        {
            if (aIController.StrategicState == EStraticState.EXPLORATION)
                return 0.8f;
            else if (aIController.StrategicState == EStraticState.DEFENSIVE)
                return 0.6f;
            else
                return 0.2f;
        }

        return 0.8f;
    }

    override public void OnEnter(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return;

        hasReachedIntermediate = false;
        squad.SetSquadState(ESquadState.CAPTURE_LAB);
        targetLab = SelectLab(worldState, squad);
        if (targetLab == null) return;

        targetLab.AISquadsCapturing.Add(squad);
        if(worldState.fogOfWarManager == null || worldState.fogOfWarManager.IsLabVisible(ETeam.Red, targetLab))
        {
            squad.SetSquadTarget(targetLab.gameObject);
            useIntermediatePos = false;
        }
        else
        {
            lab2squad = squad.InvisibleLeader.transform.position - targetLab.gameObject.transform.position;

            Vector3 dir = Quaternion.AngleAxis(Random.Range(-90f, 90f), Vector3.up) * lab2squad.normalized;
            dir.Normalize();
            dir *= Random.Range(minRadiusRandomIntermediate, maxRadiusRandomIntermediate);

            intermediatePos = targetLab.gameObject.transform.position + dir;

            squad.SetSquadTarget(intermediatePos);
            useIntermediatePos = true;
            hasReachedIntermediate = false;
        }
    }

    public override void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad || targetLab == null) return;

        if (useIntermediatePos && !hasReachedIntermediate)
        {
            if (squad.HasReachedTarget(5f) || (worldState.fogOfWarManager && worldState.fogOfWarManager.IsLabVisible(ETeam.Red, targetLab)))
            {
                hasReachedIntermediate = true;
                squad.SetSquadTarget(targetLab.gameObject);
            }
        }
    }

    override public void OnExit(UnitController controller, WorldState worldState, Squad squad)
    {
        if (!squad) return;

        if (targetLab)
            targetLab.AISquadsCapturing.Remove(squad);
        squad.SetSquadTarget(null);
        targetLab = null;
        hasReachedIntermediate = false;
    }

    TargetBuilding SelectLab(WorldState worldState, Squad squad)
    {
        List<TargetBuilding> VisibleLabs = worldState.GetVisibleLabs(ETeam.Red);

        TargetBuilding NearestLab = null;
        float nearest = float.MaxValue;
        foreach(TargetBuilding target in VisibleLabs)
        {
            if(target && target.GetTeam() != ETeam.Red && target.AISquadsCapturing.Count == 0 && Vector3.Distance(squad.InvisibleLeader.transform.position, target.transform.position) < nearest)
            {
                NearestLab = target;
                nearest = Vector3.Distance(squad.InvisibleLeader.transform.position, target.transform.position);
            }
        }

        if(NearestLab != null)
            return NearestLab;

        List<TargetBuilding> UnvisibleLabs = worldState.GetUnvisibleLabs(ETeam.Red);

        foreach (TargetBuilding target in UnvisibleLabs)
        {
            if (target && target.GetTeam() != ETeam.Red && target.AISquadsCapturing.Count == 0 && Vector3.Distance(squad.InvisibleLeader.transform.position, target.transform.position) < nearest)
            {
                NearestLab = target;
                nearest = Vector3.Distance(squad.InvisibleLeader.transform.position, target.transform.position);
            }
        }

        return NearestLab;
    }

}