
public interface IRepairable
{
    bool NeedsRepairing();
    void Repair(int amount);
    void FullRepair();
}
