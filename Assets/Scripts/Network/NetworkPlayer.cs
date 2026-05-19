using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayer : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float maxSpeed = 5f;
    public float rotationDegreesDelta = 300f;
    public float jumpForce = 3f;

    public Animator anim;
    public float animationMultiplier = 0.2f;

    private Rigidbody rb;
    private ConfigurableJoint mainJoint;
    private SyncPhysicsObject[] syncPhysicsObjects;

    private Vector2 moveInput;
    private bool isGrounded = false;

    private RaycastHit[] hits = new RaycastHit[10];

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainJoint = GetComponent<ConfigurableJoint>();
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce,  ForceMode.Impulse);
        }
    }

    private void Update()
    {
        CheckGround();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        UpdateAnimation();
    }

    private void CheckGround()
    {
        isGrounded = false;

        int numberOfHits = Physics.SphereCastNonAlloc(rb.position, 0.1f, transform.up * -1, hits, 0.5f);
        
        for (int i = 0; i < numberOfHits; i++)
        {
            //忽视自身碰撞体
            if (hits[i].transform.root == transform)
            {
                continue;
            }

            isGrounded = true;
            break;
        }
    }

    private void MovePlayer()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(transform.forward, rb.linearVelocity);
        float forwardVelocityMagnitude = forwardVelocity.magnitude;

        float inputMagnitude = moveInput.magnitude;
        if (inputMagnitude != 0)
        {
            Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(moveInput.x, 0, moveInput.y * -1), transform.up);

            mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.fixedDeltaTime * rotationDegreesDelta);

            if (forwardVelocityMagnitude < maxSpeed)
            {
                rb.AddForce(transform.forward * inputMagnitude * moveSpeed);
            }
        }

        anim.SetFloat("Speed", forwardVelocityMagnitude * animationMultiplier);
    }

    private void UpdateAnimation()
    {
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }
}
