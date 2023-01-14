using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask layerMasks;
    public float attackBoxOffsetX;
    public float attackCoolDown;
    public Vector3 attackBoxScale;

    Animator animator;
    NavMeshAgent navMeshAgent;
    Rigidbody rigidBody;
    bool targetIsInRange;
    bool isAttacking;
    bool canAttack;
    Vector3 lookAt;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        canAttack = true;
    }

    void Update()
    {
        lookAt = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;
        bool canMove = (Vector3.Distance(transform.position, target.position) > navMeshAgent.stoppingDistance);

        // ��󿡼� stopping distance ������ ��ŭ ������ ��ġ�� �̵�.(���� ���� ���� ���� �̵�.)
        if (isAttacking == false)
        {
            MoveToTarget();
        }

        if(canMove)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        // ���� ���� ���� ���� �̵� ������ ����. (���� ��ٿ� ���� & �̹� ���� ������ ���� �߰� ����.)
        targetIsInRange = false;
        if (canAttack == true && isAttacking == false && !canMove)
        {
            EnterAttack();
        }
    }

    void MoveToTarget()
    {
        Vector3 direction = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward * direction.x);

        navMeshAgent.destination = target.position;
    }

    public void EnterAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.SetTrigger("attack");
        Invoke("ResetAttackCoolDown", attackCoolDown);
    }

    public void ExitAttack()
    {
        isAttacking = false;
    }

    void ResetAttackCoolDown()
    {
        canAttack = true;
    }

    public void AttackTarget()
    {
        Debug.Log("is in attack target");
        Collider[] colliders = Physics.OverlapBox(transform.position + lookAt * attackBoxOffsetX, attackBoxScale / 2, rigidBody.transform.rotation, ~layerMasks);

        targetIsInRange = false;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Player")
            {
                targetIsInRange = true;
                
            }
        }
    }

    void OnDrawGizmos()
    {
        if (targetIsInRange) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + lookAt * attackBoxOffsetX, attackBoxScale);
    }
}
