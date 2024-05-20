using UnityEngine;

[CreateAssetMenu(fileName = "CircleFormation", menuName = "FormationRules/CircleFormation", order = 2)]
public class CircleFormation : Formation
{
    [SerializeField] float spacingBetweenEachCircle = 10f;
    [SerializeField] float baseAngle = 45f;
    public override void UpdateFormation(int numberOfUnits)
    {
        base.UpdateFormation(numberOfUnits);

        slots.Add(Vector3.zero);
        int InterdemediateCircleIndex = 1;
        float AngleBetweenUnit = baseAngle;
        float CurrentAngle = 0;

        for(int i = 1; i < numberOfUnits; i++)
        {
            Vector3 position = Vector3.forward * InterdemediateCircleIndex * spacingBetweenEachCircle;
            position = Quaternion.AngleAxis(CurrentAngle, Vector3.up) * position;
            slots.Add(position);

            CurrentAngle += AngleBetweenUnit;
            if(CurrentAngle >= 360)
            {
                InterdemediateCircleIndex++;
                AngleBetweenUnit /= 2f;
                CurrentAngle = 0;
            }
        }
    }
}
