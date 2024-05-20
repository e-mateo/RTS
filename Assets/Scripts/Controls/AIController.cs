using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EStraticState 
{ 
    EXPLORATION,
    AGRESSIVE,
    DEFENSIVE,
}

public sealed class AIController : UnitController
{
    EStraticState strategicState;
    public EStraticState StrategicState { get { return strategicState; } set { strategicState = value; } }

    [SerializeField] Transform possibleFactoriesGO;

    List<Vector3> possibleFactoriesLocations = new List<Vector3>();

    public bool HasPossibleFactoryLocations { get { return possibleFactoriesLocations.Count > 0; } }

    #region MonoBehaviour methods

    protected override void Awake()
    {
        base.Awake();
        GetComponentInChildren<UtilitySystem>().Controller = this;

        for(int i =0; i < possibleFactoriesGO.childCount; i++)
        {
            possibleFactoriesLocations.Add(possibleFactoriesGO.GetChild(i).transform.position);
        }
    }

    protected override void Start()
    {
        base.Start();
        possibleFactoriesLocations = possibleFactoriesLocations.OrderBy(v => Vector3.Distance(v, FactoryList[0].transform.position)).ToList();
    }

    protected override void Update()
    {
        base.Update();
    }

    #endregion

    #region Create Unit/Squad Methods

    //Returns the cost of the created unit
    public int CreateUnitAtFactory(Factory factory, Squad squad)
    {
        if(!factory) return 0;

        SelectFactory(factory);

        //Select a random unit with the ones available
        GameObject[] availableUnits = factory.GetFactoryData.AvailableUnits;
        int randomBuildUnit = Random.Range(0, availableUnits.Length);

        if (SelectedFactory.RequestUnitBuild(randomBuildUnit))
        {
            //Build the random unit if possible
            SelectedFactory.SetSquadForLastedUnit(squad); //The factory saved the squad in which built units need to go with a queue
            return factory.GetUnitCost(randomBuildUnit);
        }
        else
        {
            //Test every available unit to see if another one can be built
            for (int i = 0; i < availableUnits.Length; i++)
            {
                if (SelectedFactory.RequestUnitBuild(i))
                {
                    SelectedFactory.SetSquadForLastedUnit(squad); //The factory saved the squad in which built units need to go with a queue
                    return factory.GetUnitCost(i);
                }
            }
        }

        return 0;
    }

    public override void AddUnit(Unit unit, Squad squad)
    {
        base.AddUnit(unit, squad);

        //Add the unit in its squad
        if (squad)
        {
            squad.AddUnit(unit);
            return;
        }

        //If the squad that the unit was supposed to go is destroyed (for example if all of the units in the squad are dead)
        //Add the unit to the nearest squad or create a new one if no squad are available
        if(squadList.Count > 0)
        {
            //Find the nearest squad
            float nearest = float.MaxValue;
            Squad nearestSquad = null;
            foreach (Squad squadInList in squadList)
            {
                float distance = Vector3.Distance(squadInList.InvisibleLeader.transform.position, unit.transform.position);
                if(distance < nearest)
                {
                    nearest = distance;
                    nearestSquad = squadInList;
                }
            }
            nearestSquad.AddUnit(unit);
        }
        else
        {
            //Create a new squad
            Squad newSquad = CreateNewSquad();
            newSquad.AddUnit(unit);
            newSquad.gameObject.SetActive(true);
        }
    }

    public Squad CreateSquad(int buildPointToUse, float probabilityTank)
    {
        Squad newSquad = CreateNewSquad(Vector3.zero, Quaternion.identity);

        while (buildPointToUse > 0) //Create units until we used every build point for the creation of the squad
        {
            //Select the factory according to if we want to build a tank or not
            Factory factory = SelectAFactory(probabilityTank);
            if (!factory) return newSquad;
            
            //Create the unit at the selected factory
            SelectFactory(factory);
            int cost = CreateUnitAtFactory(factory, newSquad);
            if (cost == 0) return newSquad;

            buildPointToUse -= cost;
            newSquad.IncomingForce += cost;
            newSquad.numberOfTroupsAtBeginning++;
        }

        return newSquad;    
    }

    public int FillSquad(Squad squad, int buildPointToUse, float probabilityTank)
    {
        int forceAddedToTheSquad = 0;
        while (buildPointToUse > 0) //Fill the squad until we used every build point
        {
            Factory factory = SelectAFactory(probabilityTank);
            if (!factory) return forceAddedToTheSquad;

            SelectFactory(factory);
            int cost = CreateUnitAtFactory(factory, squad);
            buildPointToUse -= cost;

            if (cost == 0) return forceAddedToTheSquad;

            forceAddedToTheSquad += cost;
        }

        return forceAddedToTheSquad;
    }

    #endregion

    #region Create Factory Methods

    public bool CreateNewFactory(bool bIsHeavyHQ)
    {
        for(int i = FactoryList.Count - 1; i >= 0; i--)
        {
            if (FactoryList[i].CurrentState == Factory.State.Available)
            {
                SelectFactory(FactoryList[i]);
                if (FactoryList[i].GetFactoryCost(bIsHeavyHQ ? 1 : 0) > TotalBuildPoints)
                    continue;

                if (RequestFactoryBuild(bIsHeavyHQ ? 1 : 0, possibleFactoriesLocations[0]))
                {
                    possibleFactoriesLocations.RemoveAt(0);
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Select Factory
    private Factory SelectAFactory(float probabilityTank)
    {
        Factory factory;
        float probaHeavyTroup = Random.Range(0f, 1.0f);
        if (probaHeavyTroup < probabilityTank)
        {
            factory = SelectStrongFactory();
            if (factory == null)
                factory = SelectLightFactory();
        }
        else
        {
            factory = SelectLightFactory();
            if (factory == null)
                factory = SelectStrongFactory();
        }
        return factory;
    }

    private Factory SelectLightFactory()
    {
        Factory bestFactory = null;
        int minQueueBuildingUnit = int.MaxValue;

        foreach (Factory factory in FactoryList)
        {
            if (factory.GetFactoryData.TypeId == 0) //Light Factory
            {
                if (factory.BuildQueueCount < minQueueBuildingUnit && factory.CanBuild())
                {
                    minQueueBuildingUnit = factory.BuildQueueCount;
                    bestFactory = factory;
                }
            }
        }

        return bestFactory;
    }

    private Factory SelectStrongFactory()
    {
        Factory bestFactory = null;
        int minQueueBuildingUnit = int.MaxValue;

        foreach (Factory factory in FactoryList)
        {
            if (factory.GetFactoryData.TypeId == 1) //Strong Factory
            {
                if (factory.BuildQueueCount < minQueueBuildingUnit && factory.CanBuild())
                {
                    minQueueBuildingUnit = factory.BuildQueueCount;
                    bestFactory = factory;
                }
            }
        }

        return bestFactory;
    }
    #endregion
}
