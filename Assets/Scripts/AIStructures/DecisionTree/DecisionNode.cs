using UnityEngine;

public class DecisionNode : TreeNode
{
    [SerializeField] public TreeNode trueNode;
    [SerializeField] public TreeNode falseNode;

    virtual public bool Evaluate(Unit unit) { return false; } 
}
