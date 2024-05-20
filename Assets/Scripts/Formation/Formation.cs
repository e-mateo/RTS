using System.Collections.Generic;
using UnityEngine;

public class Formation : ScriptableObject
{
    protected List<Vector3> slots = new List<Vector3>();
    protected int numberUnits;

    public int GetSlotNumbers() { return slots.Count; }
    public virtual void UpdateFormation(int numberOfUnits)
    {
        numberUnits = numberOfUnits;
        slots.Clear();
    }

    public Vector3 GetLocalSlot(int index)
    {
        if (index < slots.Count)
            return slots[index];

        return Vector3.zero;
    }

    public Vector3 GetWorldSlot(Transform Leader, int index)
    {
        if(index >= slots.Count)
            UpdateFormation(index);

        if(Leader == null)
            return Vector3.zero;

        if (index < slots.Count)
            return Leader.position + (Leader.rotation * slots[index]);

        return Vector3.zero;
    }

    public void AddSlot()
    {
        numberUnits++;
        UpdateFormation(numberUnits);
    }

    public void RemoveSlot()
    {
        if (numberUnits > 0)
        {
            numberUnits--;
            UpdateFormation(numberUnits);
        }
    }
}
