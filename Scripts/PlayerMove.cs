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

        // �̵� ���� ����.
        moveMask = GameCharacter.CharacterStateMask.isAttacking
            | GameCharacter.CharacterStateMask.isDamaged
            | GameCharacter.CharacterStateMask.isStun;

        // ���� ���� ����.
        jumpMask = GameCharacter.CharacterStateMask.isInAir 
            | GameCharacter.CharacterStateMask.isAttacking 
            | GameCharacter.CharacterStateMask.isDamaged
            | GameCharacter.CharacterStateMask.isStun;
    }

    void Update()
    {
        // �����¿� �̵� �Է�.
        horizontalInputAxis = Input.GetAxisRaw("Horizontal");
        verticalInputAxis = Input.GetAxisRaw("Vertical");

        if(playerCharacter.HasCharacterState(moveMask) == false)
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

        // ���� �Է�.
        if (Input.GetButtonDown("Jump") 
            && playerCharacter.HasCharacterState(jumpMask) == false
            && canJump)
        {
            playerCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isInAir);
            pressedJump = true;
            canJump = false;
            StartCoroutine(CanJump(jumpCoolDown));
        }

        // ĳ���� �̵� ���� ������Ʈ
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
        // Update() �Լ����� ������Ʈ�� �Է��� ���� ĳ���͸� �̵� ��Ŵ.
        Move();
    }

    void Move()
    {
        // ���� ����� üũ.
        if (rigidBody.velocity.y < 0)
        {
            RaycastHit hitInfo = new RaycastHit();
            Physics.Raycast(rigidBody.position, Vector3.down, out hitInfo, 1);
            if (hitInfo.collider != null && hitInfo.collider.tag == "Ground" && hitInfo.distance < 0.5f)
            {
                playerCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isInAir);
            }
        }

        // �̵� �������� ĳ���͸� ȸ�� ��Ŵ. (��,�� ȸ��)
        if (Mathf.Abs(horizontalInputAxis) > 0.0f)
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
        if (playerCharacter.HasCharacterState(moveMask)) 
        {
            moveDirection = new Vector3(moveDirection.x * groundMoveAdjust, 0, moveDirection.z * groundMoveAdjust);
        }
        if (playerCharacter.HasCharacterState(GameCharacter.CharacterStateMask.isInAir)) 
        {
            moveDirection = new Vector3(moveDirection.x * jumpMoveAdjust, 0, moveDirection.z * jumpMoveAdjust);
        }

        // X, Z�� �̵� �ӷ��� �ִ� �ӷ����� ����.
        Vector3 vXZ = new Vector3(rigidBody.velocity.x, 0.0f, rigidBody.velocity.z);
        if (Vector3.Magnitude(rigidBody.velocity) > movementSpeed * movementSpeed)
        {
            // Clamped X,Z axis velocity + Jump velocity
            rigidBody.velocity = Vector3.ClampMagnitude(vXZ, movementSpeed) + new Vector3(0, rigidBody.velocity.y, 0);
        }

        // ĳ���� �̵�.
        rigidBody.AddForce(moveDirection * movementSpeed, ForceMode.Impulse);
    }

    void CalculateSlopeMovement()
    {
        Vector3 traceStart = Vector3.zero;
        Vector3 hitPoint = Vector3.zero, surfaceNormal = Vector3.zero, slopeDirection = Vector3.zero;

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

        UnityEngine.Debug.DrawLine(Vector3.zero, traceStart, Color.blue);

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
            UnityEngine.Debug.DrawLine(traceStart, hitPoint, Color.red);
            UnityEngine.Debug.DrawLine(hitPoint, hitPoint + slopeDirection, Color.green);
            moveDirection += slopeDirection; // ��� ǥ���� ������ ������ ������. (y ��).
        }
    }
    
    IEnumerator CanJump(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
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
