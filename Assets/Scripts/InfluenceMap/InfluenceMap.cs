using System;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceNode : Node
{
    public ETeam team;
    public float value = 0f;

    public bool SetValue(ETeam f, float v)
    {
        if (f == ETeam.Neutral)
        {
            team = f;
            value = v;
            return true;
        }

        if (f == team)
        {
            value += v;
            return true;
        }
        else if (v > value)
        {
            value = v;
            team = f;
            return true;
        }
        return false;
    }
}

public enum EInfluenceAttenuation
{
    LINEAR,
    SQUAREROOT,
    POWER,
}


public class InfluenceMap : Graph
{
    // Singleton access
    static InfluenceMap _Instance = null;
    static public InfluenceMap Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<InfluenceMap>();
            return _Instance;
        }
    }

    public float UpdateFrequency = 0.5f;
    private float LastUpdateTime = float.MinValue;

    // Units used to generate influence map
    public List<Unit> UnitList;

    private bool IsGraphCreated = false;
    private bool IsInitialized = false;

    [SerializeField] private EInfluenceAttenuation InfluenceAttenuation;


    private void Awake()
    {
        CreateTiledGrid();
        OnGraphCreated += () => { IsGraphCreated = true; };
    }

    private void Update()
    {
        if (!IsGraphCreated)
            return;

        if (!IsInitialized)
        {
            UnitList.Clear();
            UnitList.AddRange(FindObjectsOfType<Unit>());
            ComputeInfluence();
            IsInitialized = true;
        }

        if (Time.time - LastUpdateTime > UpdateFrequency)
        {
            if (HaveUnitsMoved())
                ComputeInfluence();

            LastUpdateTime = Time.time;
        }
    }

    public Tuple<ETeam, float> GetInfluence(Vector3 position)
    {
        InfluenceNode node = GetNode(position) as InfluenceNode;
        return Tuple.Create(node.team, node.value);
    }
    public bool AreThereEnemiesAround(ETeam EnemyTeam, float RadiusToCheck, Vector3 position, out float DistanceFromEnemy)
    {
        DistanceFromEnemy = 0f;
        //Create Queue and List for Breadth First Search
        Queue<InfluenceNode> QueueList = new Queue<InfluenceNode>();
        List<InfluenceNode> VisitedList = new List<InfluenceNode>();

        //Setup the Source node in the BFS
        InfluenceNode StartingNode = GetNode(position) as InfluenceNode;
        if(StartingNode == null)
            return false;

        QueueList.Enqueue(StartingNode);
        VisitedList.Add(StartingNode);

        while (QueueList.Count != 0)
        {
            InfluenceNode CurrentNode = QueueList.Dequeue();

            foreach (InfluenceNode Neighbour in CurrentNode.Neighbours)
            {
                if (VisitedList.Contains(Neighbour))
                    continue;

                float DistanceFromSourceNode = Vector3.Distance(Neighbour.Position, position);
                if (DistanceFromSourceNode > RadiusToCheck)
                    continue;

                if(Neighbour.team == EnemyTeam && Neighbour.value > 0)
                {
                    DistanceFromEnemy = DistanceFromSourceNode;
                    return true;
                }

                VisitedList.Add(Neighbour);
                QueueList.Enqueue(Neighbour);
            }
        }

        return false;
    }

    private bool HaveUnitsMoved()
    {
        //TODO

        return true;
    }

    protected override Node CreateNode()
    {
        return new InfluenceNode();
    }

    #region Influence Map
    public void AddUnit(Unit u)
    {
        if (UnitList.Contains(u))
            return;
        UnitList.Add(u);
    }

    public void RemoveUnit(Unit u)
    {
        UnitList.Remove(u);
    }

    public Queue<InfluenceNode> QueueList = new Queue<InfluenceNode>();
    public List<InfluenceNode> VisitedList = new List<InfluenceNode>();

    public void ComputeInfluence()
    {
        // Reset all influence nodes
        foreach (InfluenceNode node in NodeList)
            node.SetValue(ETeam.Neutral, 0f);

        UnitList.Clear();
        UnitList.AddRange(FindObjectsOfType<Unit>());

        foreach (Unit unit in UnitList)
        {
            InfluenceNode UnitNode = (GetNode(unit.transform.position)) as InfluenceNode;
            ComputeInfluenceAroundUnit(unit, UnitNode);
        }
    }

    private void ComputeInfluenceAroundUnit(Unit unit, InfluenceNode SourceNode)
    {
        if(unit == null || SourceNode == null) return;

        //Create Queue and List for Breadth First Search
        Queue<InfluenceNode> QueueList = new Queue<InfluenceNode>();
        List<InfluenceNode> VisitedList = new List<InfluenceNode>();

        //Setup the Source node in the BFS
        QueueList.Enqueue(SourceNode);
        VisitedList.Add(SourceNode);
        SourceNode.SetValue(unit.GetTeam(), unit.Influence);

        while (QueueList.Count != 0)
        {
            InfluenceNode CurrentNode = QueueList.Dequeue();

            foreach (InfluenceNode Neighbour in CurrentNode.Neighbours)
            {
                if (VisitedList.Contains(Neighbour))
                    continue;

                float DistanceFromSourceNode = Vector3.Distance(Neighbour.Position, SourceNode.Position) / (float)SquareSize;
                if (DistanceFromSourceNode > unit.RadiusInfluence)
                    continue;

                float Value = GetAttenuationValue(unit.Influence, DistanceFromSourceNode);
                Neighbour.SetValue(SourceNode.team, Value);
                VisitedList.Add(Neighbour);
                QueueList.Enqueue(Neighbour);
            }
        }
    }

    private float GetAttenuationValue(float BaseInfluence, float DistanceFromUnit)
    {
        switch (InfluenceAttenuation)
        {
            case EInfluenceAttenuation.LINEAR:
                return BaseInfluence / (1f + DistanceFromUnit);
            case EInfluenceAttenuation.SQUAREROOT:
                return BaseInfluence / Mathf.Sqrt(1f + DistanceFromUnit);
            case EInfluenceAttenuation.POWER:
                return BaseInfluence / Mathf.Pow(1f + DistanceFromUnit, 2f);
            default:
                break;
        }

        return 0f;
    }

    #endregion

    #region Gizmos

    // Draw influence map result as colored cubes using Gizmos
    protected override void DrawNodesGizmo()
    {
        for (int i = 0; i < NodeList.Count; i++)
        {
            InfluenceNode node = NodeList[i] as InfluenceNode;
            if (node != null)
            {
                Color nodeColor = node.team switch
                {
                    ETeam.Blue => Color.blue,
                    ETeam.Red => Color.red,
                    ETeam.Neutral => Color.black,
                    _ => throw new System.NotImplementedException()
                };

                nodeColor.a = Mathf.Max(node.value, 0.1f);
                Gizmos.color = nodeColor;
                Gizmos.DrawCube(new Vector3(node.Position.x, node.Position.y, node.Position.z), Vector3.one * SquareSize * 0.95f);
            }
        }
    }
    #endregion
}

