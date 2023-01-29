using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpScale;
    [SerializeField] private float jumpCoolDown;
    [SerializeField] private float jumpMoveAdjust;
    [SerializeField] private float groundMoveAdjust;
    [SerializeField] private LayerMask layersToIgnore;

    Rigidbody rigidBody;
    Animator animator;
    float horizontalInputAxis;
    float verticalInputAxis;
    bool pressedJump;
    bool canJump;
    Vector3 moveDirection;

    CustomCharacter player;
    StateMask moveMask;
    StateMask jumpMask;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        player = GetComponent<CustomCharacter>();
    }

    void Start()
    {
        horizontalInputAxis = 0.0f;
        verticalInputAxis = 0.0f;
        moveDirection = Vector3.zero;
        pressedJump = false;
        canJump = true;

        // �̵� ���� ����.
        moveMask = StateMask.ATTACKING | StateMask.STUNNED | StateMask.GATHERING;

        // ���� ���� ����.
        jumpMask = StateMask.INAIR | StateMask.ATTACKING | StateMask.STUNNED;
    }

    void Update()
    {
        if (player.HasState(moveMask) == false)
        {
            // ���� �̵� ���� ���.
            moveDirection = new Vector3(horizontalInputAxis, 0, verticalInputAxis);
            // ���� ���� ���.
            CalculateSlopeMovement();
            moveDirection = moveDirection.normalized;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        // ĳ���� �̵� ���� ������Ʈ
        if (Vector3.SqrMagnitude(moveDirection) > 0.0f)
        {
            player.AddState(StateMask.RUNNING);
        }
        else
        {
            player.RemoveState(StateMask.RUNNING);
        }

        // ĳ���� ���¿� ���� �ִϸ��̼� Ʈ����.
        TriggerAnimations();
    }

    void FixedUpdate()
    {
        // ���� ����� üũ.
        if (rigidBody.velocity.y < 0)
        {
            RaycastHit hitInfo = new RaycastHit();
            Physics.Raycast(rigidBody.position, Vector3.down, out hitInfo, 1);
            if (hitInfo.collider != null && hitInfo.collider.tag == "Ground")
            {
                // ������ ������ �ʾƵ� ���߿��� ������ �� ���� �ִϸ��̼��� ����� �� �ֵ��� INAIR ���¸� �߰�.
                if (hitInfo.distance < 0.2f)
                    player.RemoveState(StateMask.INAIR);
                else if (player.HasState(StateMask.INAIR) == false)
                {
                    player.AddState(StateMask.INAIR);
                }
            }
        }

        // Update() �Լ����� ������Ʈ�� �Է��� ���� ĳ���͸� �̵� ��Ŵ.
        Move();
    }

    void Move()
    {
        // �̵� �������� ĳ���͸� ȸ�� ��Ŵ. (��,�� ȸ��)
        if (player.HasState(moveMask) == false && Mathf.Abs(horizontalInputAxis) > 0.0f)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward * horizontalInputAxis);
        }

        // ���� �̵�.
        if (pressedJump)
        {
            pressedJump = false;
            rigidBody.AddForce(Vector3.up * jumpScale, ForceMode.Impulse);
        }

        // �̵� ����.
        if (player.HasState(moveMask))
        {
            moveDirection = new Vector3(moveDirection.x * groundMoveAdjust, 0, moveDirection.z * groundMoveAdjust);
        }
        if (player.HasState(StateMask.INAIR))
        {
            moveDirection = new Vector3(moveDirection.x * jumpMoveAdjust, 0, moveDirection.z * jumpMoveAdjust);
        }

        // X, Z�� �̵� �ӷ��� �ִ� �ӷ����� ����.
        Vector3 vXZ = new Vector3(rigidBody.velocity.x, 0.0f, rigidBody.velocity.z);
        if (Vector3.Magnitude(rigidBody.velocity) > movementSpeed * movementSpeed)
        {
            // x,z �� �ӵ��� �ִ� �̵� �ӷ����� �����ϰ� ���⿡ ���� �ӵ��� ������.
            rigidBody.velocity = Vector3.ClampMagnitude(vXZ, movementSpeed) + new Vector3(0, rigidBody.velocity.y, 0);
        }

        // ĳ���� �̵�.
        rigidBody.AddForce(moveDirection * movementSpeed, ForceMode.Impulse);
    }

    IEnumerator CanJump(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canJump = true;
    }

    void CalculateSlopeMovement()
    {
        Vector3 traceStart = Vector3.zero;

        // �÷��̾ �� �ִ� �ٴ� ��ġ�� ���.
        RaycastHit hitInfo;
        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out hitInfo, 5, ~layersToIgnore)
            )
        {
            traceStart = hitInfo.point + Vector3.up * 0.01f; // �ٴڿ��� ���� �ణ�� �������� ���� ��ġ.
        }

        //UnityEngine.Debug.DrawLine(Vector3.zero, traceStart, Color.blue);

        Vector3 hitPoint = Vector3.zero, surfaceNormal = Vector3.zero, slopeDirection = Vector3.zero;

        // �̵� ���� ������ ���� Ʈ���̽� ����, ���� �̵� ������ �����.
        if (Physics.Raycast(
            traceStart,
            new Vector3(horizontalInputAxis, 0, verticalInputAxis),
            out hitInfo, movementSpeed, ~layersToIgnore)
            )
        {
            hitPoint = hitInfo.point;
            surfaceNormal = hitInfo.normal;
            slopeDirection = Vector3.ProjectOnPlane(moveDirection, surfaceNormal);
            //UnityEngine.Debug.DrawLine(traceStart, hitPoint, Color.red);
            //UnityEngine.Debug.DrawLine(hitPoint, hitPoint + slopeDirection, Color.green);
            moveDirection += slopeDirection; // ��� ǥ���� ������ ������ ������. (y ��).
        }
    }

    void TriggerAnimations()
    {
        if (player.HasState(StateMask.RUNNING))
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        if (player.HasState(StateMask.INAIR))
        {
            animator.SetBool("isInAir", true);
        }
        else
        {
            animator.SetBool("isInAir", false);
        }
    }

    public void Input_Movement(InputAction.CallbackContext context)
    {
        Vector2 axisValue = context.ReadValue<Vector2>();
        if(axisValue!=null)
        {
            // �����¿� �̵� �Է�.
            horizontalInputAxis = axisValue.x;
            verticalInputAxis = axisValue.y;
        }
    }

    public void Input_Jump(InputAction.CallbackContext context)
    {
        if(context.action.phase == InputActionPhase.Performed)
        {
            if(player.HasState(jumpMask) == false && canJump)
            {
                player.AddState(StateMask.INAIR);
                pressedJump = true;
                canJump = false;
                StartCoroutine(CanJump(jumpCoolDown));
            }
        }
    }
}
