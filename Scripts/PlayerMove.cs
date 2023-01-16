using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerMove : MonoBehaviour
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

    GameCharacter playerCharacter;
    GameCharacter.CharacterStateMask moveMask;
    GameCharacter.CharacterStateMask jumpMask;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerCharacter = GetComponent<GameCharacter>();  
    }

    void Start()
    {
        horizontalInputAxis = 0.0f;
        verticalInputAxis = 0.0f;
        moveDirection = Vector3.zero;
        pressedJump = false; 
        canJump = true;

        // 이동 제약 조건.
        moveMask = GameCharacter.CharacterStateMask.isAttacking
            | GameCharacter.CharacterStateMask.isDamaged
            | GameCharacter.CharacterStateMask.isStun;

        // 점프 제약 조건.
        jumpMask = GameCharacter.CharacterStateMask.isInAir 
            | GameCharacter.CharacterStateMask.isAttacking 
            | GameCharacter.CharacterStateMask.isDamaged
            | GameCharacter.CharacterStateMask.isStun;
    }

    void Update()
    {
        // 상하좌우 이동 입력.
        horizontalInputAxis = Input.GetAxisRaw("Horizontal");
        verticalInputAxis = Input.GetAxisRaw("Vertical");

        if(playerCharacter.HasCharacterState(moveMask) == false)
        {
            // 지면 이동 방향 계산.
            moveDirection = new Vector3(horizontalInputAxis, 0, verticalInputAxis);
            // 경사면 방향 계산.
            CalculateSlopeMovement();
            moveDirection = moveDirection.normalized;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        // 점프 입력.
        if (Input.GetButtonDown("Jump") 
            && playerCharacter.HasCharacterState(jumpMask) == false
            && canJump)
        {
            playerCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isInAir);
            pressedJump = true;
            canJump = false;
            Invoke("CanJump", jumpCoolDown);
        }

        // 캐릭터 이동 상태 업데이트
        if (Vector3.Magnitude(moveDirection) > 0.0f)
        {
            playerCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isRunning);
        }
        else
        {
            playerCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isRunning);
        }

        ProcessAnimations();
    }

    void FixedUpdate()
    {
        // Update() 함수에서 업데이트한 입력을 통해 캐릭터를 이동 시킴.
        Move();
    }

    void Move()
    {
        // 땅에 닿는지 체크.
        if (rigidBody.velocity.y < 0)
        {
            RaycastHit hitInfo = new RaycastHit();
            Physics.Raycast(rigidBody.position, Vector3.down, out hitInfo, 1);
            if (hitInfo.collider != null && hitInfo.collider.tag == "Ground" && hitInfo.distance < 0.5f)
            {
                playerCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isInAir);
            }
        }

        // 이동 방향으로 캐릭터를 회전 시킴. (좌,우 회전)
        if (Mathf.Abs(horizontalInputAxis) > 0.0f)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward * horizontalInputAxis);
        }

        // 점프 이동.
        if (pressedJump)
        {
            pressedJump = false;
            rigidBody.AddForce(Vector3.up * jumpScale, ForceMode.Impulse);
        }

        // 이동 제한.
        if (playerCharacter.HasCharacterState(moveMask)) 
        {
            moveDirection = new Vector3(moveDirection.x * groundMoveAdjust, 0, moveDirection.z * groundMoveAdjust);
        }
        if (playerCharacter.HasCharacterState(GameCharacter.CharacterStateMask.isInAir)) 
        {
            moveDirection = new Vector3(moveDirection.x * jumpMoveAdjust, 0, moveDirection.z * jumpMoveAdjust);
        }

        // X, Z축 이동 속력을 최대 속력으로 제한.
        Vector3 vXZ = new Vector3(rigidBody.velocity.x, 0.0f, rigidBody.velocity.z);
        if (Vector3.Magnitude(rigidBody.velocity) > movementSpeed * movementSpeed)
        {
            // Clamped X,Z axis velocity + Jump velocity
            rigidBody.velocity = Vector3.ClampMagnitude(vXZ, movementSpeed) + new Vector3(0, rigidBody.velocity.y, 0);
        }

        // 캐릭터 이동.
        rigidBody.AddForce(moveDirection * movementSpeed, ForceMode.Impulse);
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
            out hitInfo, 5, ~layersToIgnore)
            )
        {
            traceStart = hitInfo.point + Vector3.up * 0.01f; // 바닥에서 위로 약간의 오프셋을 더한 위치.
        }

        UnityEngine.Debug.DrawLine(Vector3.zero, traceStart, Color.blue);

        // 이동 방향 쪽으로 라인 트레이스 수행, 경사면 이동 방향을 계산함.
        if (Physics.Raycast(
            traceStart,
            new Vector3(horizontalInputAxis, 0, verticalInputAxis),
            out hitInfo, movementSpeed, ~layersToIgnore)
            )
        {
            hitPoint = hitInfo.point;
            surfaceNormal = hitInfo.normal;
            slopeDirection = Vector3.ProjectOnPlane(moveDirection, surfaceNormal);
            UnityEngine.Debug.DrawLine(traceStart, hitPoint, Color.red);
            UnityEngine.Debug.DrawLine(hitPoint, hitPoint + slopeDirection, Color.green);
            moveDirection += slopeDirection; // 경사 표면을 따르는 방향을 더해줌. (y 값).
        }
    }
    
    void CanJump()
    {
        canJump = true;
    }

    void ProcessAnimations()
    {
        if (playerCharacter.HasCharacterState(GameCharacter.CharacterStateMask.isRunning))
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        if (playerCharacter.HasCharacterState(GameCharacter.CharacterStateMask.isInAir))
        {
            animator.SetBool("isInAir", true);
        }
        else
        {
            animator.SetBool("isInAir", false);
        }
    }
}
