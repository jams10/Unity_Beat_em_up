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

        // 대상에서 stopping distance 오프셋 만큼 떨어진 위치로 이동.(공격 가능 범위 까지 이동.)
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

        // 공격 가능 범위 까지 이동 했으면 공격. (공격 쿨다운 충족 & 이미 공격 중이지 않은 추가 조건.)
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
