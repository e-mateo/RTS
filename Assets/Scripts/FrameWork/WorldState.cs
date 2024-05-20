using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class WorldState : MonoBehaviour
{
    public List<Squad> allySquads = new List<Squad>();
    public List<Squad> enemySquads = new List<Squad>();

    public List<Unit> allyAloneUnits = new List<Unit>();
    public List<Unit> enemyAloneUnits = new List<Unit>();

    public List<TargetBuilding> labs = new List<TargetBuilding>();
    public List<TargetBuilding> allyLabs = new List<TargetBuilding>();
    public List<TargetBuilding> enemyLabs = new List<TargetBuilding>();

    public List<Factory> playerFactories = new List<Factory>();
    public List<Factory> AIFactories = new List<Factory>();

    public FogOfWarManager fogOfWarManager;
    public InfluenceMap influenceMap;

    Dictionary<Factory, List<Unit>> PlayerUnitsAroundFactory = new Dictionary<Factory, List<Unit>>();
    Dictionary<Factory, List<Unit>> AIUnitsAroundFactory = new Dictionary<Factory, List<Unit>>();

    float TimerUpdateUnitAroundFactory = 5;
    float CurrentTimerUpdateUnitAroundFactory = 0;

    // Singleton access
    static WorldState _Instance = null;
    static public WorldState Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<WorldState>();
            return _Instance;
        }
    }

    #region MonoBehaviors
    void Start()
    {
        labs = FindObjectsOfType<TargetBuilding>().ToList();

        influenceMap = InfluenceMap.Instance;
        fogOfWarManager = FogOfWarManager.Instance;
    }


    void Update()
    {
        CurrentTimerUpdateUnitAroundFactory -= Time.deltaTime;

        if (CurrentTimerUpdateUnitAroundFactory <= 0)
        {
            PlayerUnitsAroundFactory.Clear();
            AIUnitsAroundFactory.Clear();

            foreach (Factory fact in playerFactories)
            {
                CheckUnitAroundFactory(20, fact);
            }

            foreach (Factory fact in AIFactories)
            {
                CheckUnitAroundFactory(20, fact);
            }

            CurrentTimerUpdateUnitAroundFactory = TimerUpdateUnitAroundFactory;
        }
    }

    #endregion

    #region SquadMethods
    public int ComputeAllyPower()
    {
        int power = 0;

        for (int i = 0; i < allySquads.Count; ++i)
            for (int j = 0; j < allySquads[i].Units.Count; ++j)
                power += allySquads[i].Units[j].Cost;

        return power;
    }

    public int ComputeEnemyPower()
    {
        int power = 0;

        for (int i = 0; i < enemySquads.Count; ++i)
            for (int j = 0; j < enemySquads[i].Units.Count; ++j)
                power += enemySquads[i].Units[j].Cost;

        return power;
    }

    public int ComputeAllySquadPower(Squad squad)
    {
        int power = 0;

        for (int i = 0; i < squad.Units.Count; ++i)
            power += squad.Units[i].Cost;

        return power;
    }

    public int ComputeEnemySquadPower(Squad squad)
    {
        int power = 0;

        for (int i = 0; i < squad.Units.Count; ++i)
            power += squad.Units[i].Cost;

        return power;
    }

    public int GetNumberUnit(ETeam Team)
    {
        int numberOfUnits = 0;
        List<Squad> listSquad = Team == ETeam.Blue ? allySquads : enemySquads;

        foreach (Squad squad in listSquad)
        {
            numberOfUnits += squad.Units.Count;
        }

        return numberOfUnits;
    }

    public void AddSquad(Squad squad, ETeam team)
    {
        List<Squad> listSquad = team == ETeam.Blue ? allySquads : enemySquads;
        listSquad.Add(squad);
        GameServices.GetControllerByTeam(team).squadList.Add(squad);
    }

    public void RemoveSquad(Squad squad, ETeam team)
    {
        List<Squad> listSquad = team == ETeam.Blue ? allySquads : enemySquads;
        listSquad.Remove(squad);
        GameServices.GetControllerByTeam(team).squadList.Remove(squad);
    }

    public void AddAloneUnit(Unit unit, ETeam team)
    {
        List<Unit> listUnits = team == ETeam.Blue ? allyAloneUnits : enemyAloneUnits;
        listUnits.Add(unit);
    }

    public void RemoveAloneUnit(Unit unit, ETeam team)
    {
        List<Unit> listUnits = team == ETeam.Blue ? allyAloneUnits : enemyAloneUnits;
        listUnits.Remove(unit);
    }

    public Squad GetNearestSquad(Squad squad, out float distanceToSquad)
    {
        int idSquad = -1;
        float nearestSquad = float.MaxValue;
        List<Squad> squadToSearch = squad.SquadTeam == ETeam.Blue ? enemySquads : allySquads;
        for (int i = 0; i < squadToSearch.Count; i++)
        {
            float distance = Vector3.Distance(squadToSearch[i].InvisibleLeader.transform.position, squad.InvisibleLeader.transform.position);
            if (distance < nearestSquad)
            {
                nearestSquad = distance;
                idSquad = i;
            }
        }
        distanceToSquad = nearestSquad;
        if (idSquad >= 0)
            return squadToSearch[idSquad];

        return null;
    }

    #endregion

    #region LabMethods

    public void TakeOverLab(TargetBuilding targetBuilding, ETeam team)
    {
        List<TargetBuilding> listTeamLabs = team == ETeam.Blue ? allyLabs : enemyLabs;
        List<TargetBuilding> otherTeamLabs = team == ETeam.Blue ? enemyLabs : allyLabs;

        if(otherTeamLabs.Contains(targetBuilding))
        {
            otherTeamLabs.Remove(targetBuilding);
        }

        listTeamLabs.Add(targetBuilding);
    }

    public TargetBuilding GetNearestLab(Squad squad)
    {
        TargetBuilding lab = null;
        float nearestLab = float.MaxValue;
        for (int i = 0; i < labs.Count; i++)
        {
            if (labs[i].GetTeam() != ETeam.Red)
            {
                float distance = Vector3.Distance(labs[i].transform.position, squad.InvisibleLeader.transform.position);
                if (distance < nearestLab)
                {
                    nearestLab = distance;
                    lab = labs[i];
                }
            }
        }

        return lab;
    }

    public List<TargetBuilding> GetVisibleLabs(ETeam team)
    {
        List<TargetBuilding> visibleLabs = new List<TargetBuilding>();
        foreach(TargetBuilding target in labs)
        {
            if(!fogOfWarManager)
            {
                visibleLabs.Add(target);
            }
            else if (fogOfWarManager.IsLabVisible(team, target))
            {
                visibleLabs.Add(target);    
            }
        }

        return visibleLabs;
    }

    public List<TargetBuilding> GetUnvisibleLabs(ETeam team)
    {
        List<TargetBuilding> unvisibleLabs = new List<TargetBuilding>();
        foreach (TargetBuilding target in labs)
        {
            if (fogOfWarManager && !fogOfWarManager.IsLabVisible(team, target))
            {
                unvisibleLabs.Add(target);
            }
        }

        return unvisibleLabs;
    }

    #endregion

    #region FactoryMethod

    public void AddFactory(Factory factory, ETeam team)
    {
        List<Factory> listFactory = team == ETeam.Blue ? playerFactories : AIFactories;
        listFactory.Add(factory);
    }

    public void RemoveFactory(Factory factory, ETeam team)
    {
        List<Factory> listFactory = team == ETeam.Blue ? playerFactories : AIFactories;
        listFactory.Remove(factory);
    }

    public bool AreAllFactoryFull(ETeam team)
    {
        List<Factory> factories = team == ETeam.Blue ? playerFactories : AIFactories;
        foreach( Factory fact in factories)
        {
            if(!fact.IsFull)
                return false;
        }

        return true;
    }

    public List<Factory> GetLightFactory(ETeam team)
    {
        List<Factory> factories = team == ETeam.Blue ? playerFactories : AIFactories;
        List<Factory> lightFactories = new List<Factory>();
        foreach( Factory fact in factories)
        {
            if(fact.GetFactoryData.TypeId == 0)
                lightFactories.Add(fact);
        }

        return lightFactories;
    }

    public List<Factory> GetHeavyFactory(ETeam team)
    {
        List<Factory> factories = team == ETeam.Blue ? playerFactories : AIFactories;
        List<Factory> heavyFactories = new List<Factory>();
        foreach (Factory fact in factories)
        {
            if (fact.GetFactoryData.TypeId == 1)
                heavyFactories.Add(fact);
        }

        return heavyFactories;
    }

    #endregion

    private void CheckUnitAroundFactory(float radius, Factory fact)
    {
        List<Unit> playerUnits = new List<Unit>();
        List<Unit> aiUnits = new List<Unit>();

        Collider[] units = Physics.OverlapSphere(fact.transform.position, radius, LayerMask.GetMask("Unit"));
        foreach (Collider collider in units)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit.GetTeam() == ETeam.Blue)
                playerUnits.Add(unit);
            else
                aiUnits.Add(unit);  
        }
        PlayerUnitsAroundFactory.Add(fact, playerUnits);
        AIUnitsAroundFactory.Add(fact, aiUnits);
    }

    public List<Unit> GetUnitAroundFactory(Factory fact, ETeam team)
    {
        if(team == ETeam.Blue)
        {
            if(PlayerUnitsAroundFactory.TryGetValue(fact, out List<Unit> units))
            {
                return units;
            }
        }
        else if (team == ETeam.Red)
        {
            if (AIUnitsAroundFactory.TryGetValue(fact, out List<Unit> units))
            {
                return units;
            }
        }

        return null;
    }

    public Factory GetNearestFactory(ETeam team, Vector3 position)
    {
        List<Factory> factories = team == ETeam.Blue ? playerFactories : AIFactories;
        Factory nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (Factory fact in factories)
        {
            float distance = Vector3.Distance(position, fact.transform.position);
            if(distance < nearestDistance)
            {
                nearest = fact;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}
