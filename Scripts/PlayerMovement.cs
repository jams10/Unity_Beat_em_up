using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed;
    public float attackMoveAdjust;
    public float jumpScale;
    public float jumpSpamTime;
    public float jumpMoveAdjust;
    public string[] interruptAnimNames;
    [SerializeField] private LayerMask layerMasks;

    Rigidbody rigidBody;
    Animator animator;
    float horizontalInputAxis;
    float verticalInputAxis;
    float moveDelta;
    bool pressedJump;
    Vector3 moveDirectionXZ;
    
    // Initialize any variables or game state before the game starts.
    // Awake is always called before any Start functions. This allows you to order initialization of scripts.
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        horizontalInputAxis = 0.0f;
        verticalInputAxis = 0.0f;
        moveDirectionXZ = Vector3.zero;
        moveDelta = movementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Update input axis values.
        horizontalInputAxis = Input.GetAxisRaw("Horizontal");
        verticalInputAxis = Input.GetAxisRaw("Vertical");

        // 이동 방향.
        moveDirectionXZ = new Vector3(horizontalInputAxis, 0, verticalInputAxis);
        // 경사면 방향 계산.
        CalculateSlopeMovement();

        moveDirectionXZ = moveDirectionXZ.normalized;

        UnityEngine.Debug.Log(animator.GetBool("isInAir"));

        // Jump Input
        if (!animator.GetBool("isInAir") && Input.GetButtonDown("Jump") && PlayerMeleeAttack.instance.isAttacking == false)
        {
            animator.SetBool("isInAir", true);
            pressedJump = true;
        }

        #region Animation State
        // Check run state.
        if (Vector3.Magnitude(moveDirectionXZ) > 0.0f)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
#endregion Animation State
    }

    // FixedUpdate is called once per fixed time step.
    void FixedUpdate()
    {
        // Process movement by input data which is updated from Update().
        Move();
 
    }

    void Move()
    {
        // Check ground state.
        if (rigidBody.velocity.y < 0)
        {
            RaycastHit hitInfo = new RaycastHit();
            Physics.Raycast(rigidBody.position, Vector3.down, out hitInfo, 1);
            if (hitInfo.collider != null && hitInfo.collider.tag == "Ground" && hitInfo.distance < 0.5f)
            {
                animator.SetBool("isInAir", false);
            }
        }

        // Rotate to movement direction
        if (Mathf.Abs(horizontalInputAxis) > 0.0f)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward * horizontalInputAxis);
        }

        if (pressedJump)
        {
            pressedJump = false;
            rigidBody.AddForce(Vector3.up * jumpScale, ForceMode.Impulse);
        }

        // 공격 애니메이션 재생 중인 경우 이동 제한.
        if (IsPlayingAnyInterruptAnims())
        {
            moveDirectionXZ = new Vector3(horizontalInputAxis / attackMoveAdjust, 0, verticalInputAxis / attackMoveAdjust);
        }

        if(animator.GetBool("isInAir"))
        {
            moveDirectionXZ = new Vector3(horizontalInputAxis / jumpMoveAdjust, 0, verticalInputAxis / jumpMoveAdjust);
        }

        // Limit X,Z axis speed.
        Vector3 vXZ = new Vector3(rigidBody.velocity.x, 0.0f, rigidBody.velocity.z);
        if (Vector3.Magnitude(rigidBody.velocity) > movementSpeed * movementSpeed)
        {
            // Clamped X,Z axis velocity + Jump velocity
            rigidBody.velocity = Vector3.ClampMagnitude(vXZ, movementSpeed) + new Vector3(0, rigidBody.velocity.y, 0);
        }

        rigidBody.AddForce(moveDirectionXZ * movementSpeed, ForceMode.Impulse);
    }

    bool IsPlayingAnyInterruptAnims()
    {
        // 움직임에 제약을 주는 애니메이션이 재생 중인지 여부를 체크함.
        foreach(string s in interruptAnimNames)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(s) == true)
                return true;
        }
        return false;
    }

    void CalculateSlopeMovement()
    {
        Vector3 traceStart = Vector3.zero;
        Vector3 hitPoint = Vector3.zero, surfaceNormal = Vector3.zero, slopeDirection = Vector3.zero;

        // 플레이어가 서 있는 바닥 위치를 계산.
        RaycastHit hitInfo;
        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out hitInfo, 5, ~layerMasks)
            )
        {
            traceStart = hitInfo.point + Vector3.up * 0.01f; // 바닥에서 위로 약간의 오프셋을 더한 위치.
        }

        UnityEngine.Debug.DrawLine(Vector3.zero, traceStart, Color.blue);

        // 이동 방향 쪽으로 라인 트레이스 수행, 경사면 이동 방향을 계산함.
        if (Physics.Raycast(
            traceStart,
            new Vector3(horizontalInputAxis, 0, verticalInputAxis),
            out hitInfo, movementSpeed, ~layerMasks)
            )
        {
            hitPoint = hitInfo.point;
            surfaceNormal = hitInfo.normal;
            slopeDirection = Vector3.ProjectOnPlane(moveDirectionXZ, surfaceNormal);
            UnityEngine.Debug.DrawLine(traceStart, hitPoint, Color.red);
            UnityEngine.Debug.DrawLine(hitPoint, hitPoint + slopeDirection, Color.green);
            moveDirectionXZ += slopeDirection; // 경사 표면을 따르는 방향을 더해줌. (y 값).
        }
    }
}
