using UnityEngine;

[CreateAssetMenu(fileName = "SquareFormation", menuName = "RTS/Formations/SquareFormation", order = 2)]
public class SquareFormation : Formation
{
    [SerializeField] int unitPerLine;
    [SerializeField] float spacing;

    public override void UpdateFormation(int numberOfUnits)
    {
        base.UpdateFormation(numberOfUnits);

        int numberOfLine = (numberOfUnits / unitPerLine) + 1;
        Vector3 offSetVertical = Vector3.forward * ((spacing * (float)(numberOfLine - 1)) / 2f);
        Vector3 offSetHorizontal = Vector3.left * ((spacing * (float)(unitPerLine - 1)) / 2f);
        offSetVertical = Vector3.zero;
        int currentLine = 0;
        int currentCollum = 0;

        for (int i = 0; i < numberOfUnits; i++)
        {
            Vector3 position = (Vector3.back * currentLine * spacing) + offSetVertical;
            position += (Vector3.right * currentCollum * spacing) + offSetHorizontal;
            slots.Add(position);

            currentCollum++;
            if(currentCollum == unitPerLine)
            {
                currentCollum = 0;
                currentLine++;
            }
        }
    }
}
