using UnityEngine;

public class DecisionTree : MonoBehaviour
{
    [SerializeField] float frequencyEvaluation;
    [SerializeField] ESquadState squateStateNeeded;

    float currentEvaluationTime = 0f;
    Unit unit;
    ActionNode currentActionNode = null;
    TreeNode rootNode;


    #region MonoBehaviors
    private void Awake()
    {
        rootNode = transform.GetChild(0).GetComponent<TreeNode>();
    }

    private void Start()
    {
        unit = GetComponentInParent<Unit>();
        BuildGraph(rootNode);
    }

    // Update is called once per frame
    void Update()
    {
        if (unit == null) return;

        currentEvaluationTime -= Time.deltaTime;
        if (currentEvaluationTime <= 0f) //Reevaluate the tree only at a fixed frequency
        {
            currentEvaluationTime = frequencyEvaluation;
            ActionNode selectedActionNode = GetCurrentActionNode();
            if(selectedActionNode != currentActionNode)
            {
                currentActionNode?.OnExit(unit);
                currentActionNode = selectedActionNode;
                currentActionNode?.OnEnter(unit);
                return;
            }
        }

        currentActionNode?.OnUpdate(Time.deltaTime, unit);
    }
    #endregion

    private void BuildGraph(TreeNode node)
    {
        DecisionNode decision = node as DecisionNode;
        ActionNode action = node as ActionNode;

        if (decision)
        {
            decision.trueNode.parentNode = node;
            decision.falseNode.parentNode = node;
            BuildGraph(decision.trueNode);
            BuildGraph(decision.falseNode);
        }
        else if(action)
        {
            action.parentNode = node;
        }
    }

    private ActionNode GetCurrentActionNode()
    {
        TreeNode currentNode = rootNode;
        ActionNode action = currentNode as ActionNode;

        while (action == null)
        {
            currentNode = GetNextNode(currentNode);
            action = currentNode as ActionNode;
        }

        return action;
    }

    private TreeNode GetNextNode(TreeNode node)
    {
        DecisionNode decision = node as DecisionNode;
        if(decision != null)
        {
            if (decision.Evaluate(unit))
                return decision.trueNode;
            else
                return decision.falseNode;
        }

        return null;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Desactivate()
    {
        if(currentActionNode)
            currentActionNode.OnExit(unit);
        currentActionNode = null;
        gameObject.SetActive(false);
        currentEvaluationTime = 0f;
    }

    public bool CanLaunch(ESquadState squadState)
    {
        return squadState == squateStateNeeded;
    }
}
