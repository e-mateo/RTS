using UnityEngine;

[CreateAssetMenu(fileName = "TriangleFormation", menuName = "FormationRules/Triangle", order = 2)]
public class TriangleFormation : Formation
{
    [SerializeField] float spacing;

    public override void UpdateFormation(int numberOfUnits)
    {
        base.UpdateFormation(numberOfUnits);

        int line = 1;
        int unitOnThisLine = 0;

        for (int i = 0; i < numberOfUnits; i++)
        {
            Vector3 startLeft = ((float)(line - 1) / 2f) * spacing * Vector3.left;
            slots.Add((Vector3.back * (line - 1) * spacing) + startLeft + (Vector3.right * unitOnThisLine * spacing));

            unitOnThisLine++;
            if (unitOnThisLine == line)
            {
                unitOnThisLine = 0;
                line++;
            }
        }

        Vector3 offset = Vector3.forward * ((float)(line * spacing) / 2f);
        for (int i = 0; i < numberOfUnits; i++)
        {
            slots[i] += offset;
        }
    }
}
