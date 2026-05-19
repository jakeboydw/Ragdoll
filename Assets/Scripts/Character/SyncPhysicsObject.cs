using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    public bool syncAnimation = false;
    public Rigidbody animatedRigidbody;

    private Rigidbody rb;
    private ConfigurableJoint joint;
    private Quaternion startLocalRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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