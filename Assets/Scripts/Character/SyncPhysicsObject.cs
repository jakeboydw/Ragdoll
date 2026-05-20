using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    public bool syncAnimation = false;
    public Rigidbody animatedRigidbody;

    private ConfigurableJoint joint;
    private Quaternion startLocalRotation;

    private void Awake()
    {
        joint = GetComponent<ConfigurableJoint>();

        startLocalRotation = transform.localRotation;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation)
        {
            return;
        }

        ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRigidbody.transform.localRotation, startLocalRotation);
    }
}