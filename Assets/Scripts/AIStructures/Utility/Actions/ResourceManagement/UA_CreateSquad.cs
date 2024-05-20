using UnityEngine;

[CreateAssetMenu(fileName = "UA_CreateSquad", menuName = "RTS/UtilitySystem/UA_CreateSquad", order = 1)]
public class UA_CreateSquad : UtilityAction
{
    [SerializeField] int goldNeededToCreateASquadInExplo =  5;
    [SerializeField] int goldNeededToCreateASquadInAgressif = 20;
    [SerializeField] int goldNeededToCreateASquadInPassif = 10;

    [SerializeField] int MaxBuildPointToCreateSquadExplo = 40;
    [SerializeField] int MaxBuildPointToCreateSquadAgressive = 75;
    [SerializeField] int MaxBuildPointToCreateSquadPassive = 50;

    [SerializeField] int MaxPowerLerp = 120;

    float minPower = 5f;

    override public float ComputePriority(UnitController controller, WorldState worldState, Squad squad = null)
    {
        AIController aIController = controller as AIController;

        if (WorldState.Instance.AreAllFactoryFull(ETeam.Red))
            return 0f;

        foreach(Squad AISquad in WorldState.Instance.enemySquads)
        {
            if (AISquad && AISquad.NeedMoreTroups)
                return 0f;
        }

        float power = worldState.ComputeEnemyPower();

        if (power < minPower)
            return 1f;

        if (power < 20f)
        {
            if (aIController.StrategicState == EStraticState.EXPLORATION && aIController.TotalBuildPoints >= goldNeededToCreateASquadInExplo)
                return 0.8f;
            else if (aIController.StrategicState == EStraticState.AGRESSIVE && aIController.TotalBuildPoints >= goldNeededToCreateASquadInAgressif)
                return 0.8f;
            else if (aIController.StrategicState == EStraticState.DEFENSIVE && aIController.TotalBuildPoints >= goldNeededToCreateASquadInPassif)
                return 0.8f;

            return 0;
        }
        else
        {
            if (aIController.StrategicState == EStraticState.EXPLORATION && aIController.TotalBuildPoints >= Mathf.Lerp(goldNeededToCreateASquadInExplo, MaxBuildPointToCreateSquadExplo, power / MaxPowerLerp))
                return 0.8f;
            else if (aIController.StrategicState == EStraticState.AGRESSIVE && aIController.TotalBuildPoints >= Mathf.Lerp(goldNeededToCreateASquadInAgressif, MaxBuildPointToCreateSquadAgressive, power / MaxPowerLerp))
                return 0.8f;
            else if (aIController.StrategicState == EStraticState.DEFENSIVE && aIController.TotalBuildPoints >= goldNeededToCreateASquadInPassif)
                return 0.8f;

            return 0;
        }

    }

    public override void OnUpdate(float updateFrequency, UnitController controller, WorldState worldState, Squad squad)
    {
        AIController aIController = controller as AIController;

        if (aIController.TotalBuildPoints <= 0)
            return;

        Squad SquadCreated = null;
        
        if (aIController.StrategicState == EStraticState.EXPLORATION)
            SquadCreated = aIController.CreateSquad(Mathf.Clamp(aIController.TotalBuildPoints, 0, MaxBuildPointToCreateSquadExplo), 0.1f);
        else if (aIController.StrategicState == EStraticState.AGRESSIVE)
            SquadCreated = aIController.CreateSquad(Mathf.Clamp(aIController.TotalBuildPoints, 0, MaxBuildPointToCreateSquadAgressive), 0.5f);
        else if (aIController.StrategicState == EStraticState.DEFENSIVE)
           SquadCreated =aIController.CreateSquad(Mathf.Clamp(aIController.TotalBuildPoints, 0, MaxBuildPointToCreateSquadPassive), 0.3f);
        
        if (SquadCreated == null || SquadCreated.numberOfTroupsAtBeginning == 0)
            Destroy(SquadCreated.gameObject);
    }
}
