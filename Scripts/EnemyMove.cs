using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{
    [SerializeField] private Transform target;

    Animator animator;
    NavMeshAgent navMeshAgent;
    GameCharacter enemyCharacter;
    GameCharacter.CharacterStateMask moveMask;
    Vector3 lookAtVector;

    void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyCharacter = GetComponent<GameCharacter>(); 
    }

    void Start()
    {
        lookAtVector = Vector3.zero;
        // 이동 제약 조건.
        moveMask = GameCharacter.CharacterStateMask.isAttacking
            | GameCharacter.CharacterStateMask.isDamaged
            | GameCharacter.CharacterStateMask.isStun;
    }

    void Update()
    {
        lookAtVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        if(enemyCharacter.HasCharacterState(moveMask))
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.updatePosition = false;
        }
        else
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.updatePosition = true;
            MoveToTarget();

            if (Vector3.SqrMagnitude(transform.position - target.position) > navMeshAgent.stoppingDistance * navMeshAgent.stoppingDistance)
            {
                enemyCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isRunning);
            }
            else
            {
                enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isRunning);
            }
        }

        ProcessAnimations();
    }

    void MoveToTarget()
    {
        Vector3 direction = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward * direction.x);

        navMeshAgent.destination = target.position;
    }

    void ProcessAnimations()
    {
        if (enemyCharacter.HasCharacterState(GameCharacter.CharacterStateMask.isRunning))
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }
}
