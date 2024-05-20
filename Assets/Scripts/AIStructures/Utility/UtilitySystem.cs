using System.Collections.Generic;
using UnityEngine;

public class UtilitySystem : MonoBehaviour
{
    [SerializeField] float updateFrequencyRevaluation;
    float currentTimeUpdate = 0f;
    Squad squad; //As our utility system is used for squads, we keep the squad to access it in utility action
    UnitController controller;
    [SerializeField] UtilityAction currentAction = null; //Is marked as SerializedField only to see the current action for debugging
    [SerializeField] List<UtilityAction> actions = new List<UtilityAction>();

    public UnitController Controller { get { return controller; }  set { controller = value; } }
    public UtilityAction CurrentAction { get { return currentAction; } }

    delegate void OnUtilityChanged();
    OnUtilityChanged onUtilityChanged;

    #region MonoBehavior
    void Start()
    {
        for(int i = 0; i < actions.Count; i++)
        {
            if (actions[i] != null)
                actions[i] = Instantiate<UtilityAction>(actions[i]); //Instanciate scriptable objects
        }

        if (actions.Count > 0)
            currentAction = actions[0];

        squad = GetComponent<Squad>();
        if(squad != null)
        {
            onUtilityChanged += UilityChanged;
            onUtilityChanged.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (actions.Count == 0)
            return;

        currentTimeUpdate -= Time.deltaTime;

        if (currentTimeUpdate <= 0f)
        {
            currentTimeUpdate = updateFrequencyRevaluation;
            UtilityAction BestAction = GetBestAction();
            if (BestAction && BestAction != currentAction)
            {
                currentAction?.OnExit(controller, WorldState.Instance, squad);
                currentAction = BestAction;
                currentAction?.OnEnter(controller, WorldState.Instance, squad);
                onUtilityChanged?.Invoke();
                return;
            }
        }

        currentAction?.OnUpdate(Time.deltaTime, controller, WorldState.Instance, squad);
    }
    #endregion


    private void UilityChanged()
    {
        squad?.UpdateUnitSquadDecisionTree();
    }

    private UtilityAction GetBestAction()
    {
        UtilityAction BestAction = null;
        float BestPriority = 0f;

        foreach(UtilityAction action in actions)
        {
            if (!action) continue;

            float Priority = action.ComputePriority(controller, WorldState.Instance, squad);
            if (Priority > BestPriority || BestAction == null)
            {
                BestAction = action;
                BestPriority = Priority;
            }
        }

        return BestAction;
    }
}
