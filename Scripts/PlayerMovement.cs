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

    Rigidbody rigidBody;
    BoxCollider boxCollider;
    Animator animator;
    float horizontalInputAxis;
    float verticalInputAxis;
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

        if(!animator.GetBool("isInAir") && Input.GetButtonDown("Jump"))
        {
            animator.SetBool("isInAir", true);
            rigidBody.AddForce(Vector3.up * jumpScale, ForceMode.Impulse);     
        }

#region Animation State
        // Check run state.
        if (Vector3.Magnitude(moveDirection) > 0.0f)
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
        rigidBody.AddForce(moveDirection * movementSpeed, ForceMode.Impulse);

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
    }
}
