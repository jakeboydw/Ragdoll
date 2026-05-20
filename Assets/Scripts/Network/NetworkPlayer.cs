using Fusion;
using Fusion.Addons.Physics;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public struct NetworkInputData : INetworkInput
{
    public Vector2 moveDirection;
    public NetworkBool jumpPressed;
}

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local {  get; private set; }

    public float moveSpeed = 3f;
    public float maxSpeed = 5f;
    public float rotationDegreesDelta = 300f;
    public float jumpForce = 3f;

    public Animator anim;
    public float animationMultiplier = 0.2f;

    public float groundCheckRadius = 0.1f;
    public float groundCheckDistance = 0.5f;

    private Rigidbody rb;
    private NetworkRigidbody3D networkRigidbody3D;
    private ConfigurableJoint mainJoint;
    private SyncPhysicsObject[] syncPhysicsObjects;

    private Vector2 moveInput;
    private bool isJumpPressed = false;
    private bool isGrounded = false;

    private RaycastHit[] hits = new RaycastHit[10];

    private CinemachineCamera cam;

    //网络同步参数
    [Networked]
    private float NetworkedSpeed { get; set; }

    [Networked]
    private NetworkBool NetworkGrounded { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        networkRigidbody3D = GetComponent<NetworkRigidbody3D>();
        mainJoint = GetComponent<ConfigurableJoint>();
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        isJumpPressed = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            CheckGround();
            MovePlayer(input);
            UpdateAnimation();
        }

        RespawnIfFell();
    }

    private void CheckGround()
    {
        isGrounded = false;

        int numberOfHits = Physics.SphereCastNonAlloc(rb.position, groundCheckRadius, transform.up * -1, hits, groundCheckDistance);

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

        NetworkGrounded = isGrounded;
    }

    private void MovePlayer(NetworkInputData input)
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(transform.forward, rb.linearVelocity);
        float forwardVelocityMagnitude = forwardVelocity.magnitude;

        float inputMagnitude = input.moveDirection.magnitude;
        if (inputMagnitude != 0)
        {
            Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(input.moveDirection.x, 0, input.moveDirection.y * -1), Vector3.up);

            mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Runner.DeltaTime * rotationDegreesDelta);

            if (forwardVelocityMagnitude < maxSpeed)
            {
                rb.AddForce(transform.forward * inputMagnitude * moveSpeed);
            }
        }

        if (input.jumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        NetworkedSpeed = forwardVelocityMagnitude * animationMultiplier;
    }

    private void UpdateAnimation()
    {
        anim.SetFloat("Speed", NetworkedSpeed);

        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }

    private void RespawnIfFell()
    {
        if (transform.position.y < -10)
        {
            networkRigidbody3D.Teleport(Vector3.zero, Quaternion.identity);

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData inputData = new NetworkInputData();

        inputData.moveDirection = moveInput;
        inputData.jumpPressed = isJumpPressed;

        //清空跳跃状态
        isJumpPressed = false;

        return inputData;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;

            cam = FindAnyObjectByType<CinemachineCamera>();

            if (cam != null)
            {
                cam.Follow = transform;
                cam.LookAt = transform;
            }
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        
    }
}
