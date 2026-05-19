using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    public Collider thisColllider;
    public Collider[] collidersToIgnore;

    private void Start()
    {
        foreach (Collider col in collidersToIgnore)
        {
            Physics.IgnoreCollision(thisColllider, col, true);
        }
    }
}
