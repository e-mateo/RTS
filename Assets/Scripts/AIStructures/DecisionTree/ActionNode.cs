
public class ActionNode : TreeNode
{
    virtual public void OnEnter(Unit unit) { }
    virtual public void OnUpdate(float frequency, Unit unit) { }
    virtual public void OnExit(Unit unit) { }
}
