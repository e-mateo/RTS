using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    float LifeTime = 0.5f;
    [SerializeField]
    float MoveForce = 2000f;

    float ShootDate = 0f;
    Unit UnitOwner;
    public void ShootToward(Vector3 direction, Unit owner)
    {
        ShootDate = Time.time;
        GetComponent<Rigidbody>().AddForce(direction.normalized * MoveForce);
        UnitOwner = owner;
    }

    #region MonoBehaviour methods
    void Update()
    {
        if ((Time.time - ShootDate) > LifeTime)
        {
            Destroy(gameObject);
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<Unit>()?.GetTeam() == UnitOwner.GetTeam())
            return;

        Destroy(gameObject);
    }
    #endregion
}
