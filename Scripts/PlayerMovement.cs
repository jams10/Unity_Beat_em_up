using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed;
    public float jumpScale;
    public float jumpSpamTime;
    public string[] interruptAnimNames;

    Rigidbody rigidBody;
    BoxCollider boxCollider;
    Animator animator;
    float horizontalInputAxis;
    float verticalInputAxis;
    float moveDelta;
    Vector3 moveDirection;
    
    // Initialize any variables or game state before the game starts.
    // Awake is always called before any Start functions. This allows you to order initialization of scripts.
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        boxCollider= GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();

        horizontalInputAxis = 0.0f;
        verticalInputAxis = 0.0f;
        moveDirection = Vector3.zero;
        moveDelta = movementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Update input axis values.
        horizontalInputAxis = Input.GetAxisRaw("Horizontal");
        verticalInputAxis = Input.GetAxisRaw("Vertical");

        // Prevent the faster movement in diagonal.
        moveDirection = new Vector3(horizontalInputAxis, 0, verticalInputAxis);
        moveDirection = moveDirection.normalized;

        if(!animator.GetBool("isInAir") && Input.GetButtonDown("Jump") && PlayerAttack.instance.isAttacking == false)
        {
            animator.SetBool("isInAir", true);
            rigidBody.AddForce(Vector3.up * jumpScale, ForceMode.Impulse);     
        }

#region Animation State
        // Check run state.
        if (Vector3.Magnitude(moveDirection) > 0.0f && PlayerAttack.instance.isAttacking == false)
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

        // 공격 애니메이션 재생 중인 경우 이동 제한.
        if (IsPlayingAnyInterruptAnims())
        {
            moveDelta = movementSpeed / 5.0f;
        }
        else
        {
            moveDelta = movementSpeed;
        }


        rigidBody.AddForce(moveDirection * moveDelta, ForceMode.Impulse);

        // Rotate to movement direction
        if(Mathf.Abs(horizontalInputAxis) > 0.0f)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward * horizontalInputAxis);
        }

        // Limit X,Z axis speed.
        Vector3 vXZ = new Vector3(rigidBody.velocity.x, 0.0f, rigidBody.velocity.z);
        if (Vector3.Magnitude(vXZ) > movementSpeed * movementSpeed)
        {
            // Clamped X,Z axis velocity + Jump velocity
            rigidBody.velocity = Vector3.ClampMagnitude(vXZ, movementSpeed) + new Vector3(0, rigidBody.velocity.y, 0);
        }
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
}
