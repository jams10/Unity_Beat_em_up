using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour
{
    [SerializeField] private Transform target;

    Animator animator;
    NavMeshAgent navMeshAgent;
    CustomCharacter ai;
    StateMask moveMask;
    AIBattle battleComp;

    void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        ai = GetComponent<CustomCharacter>();
        battleComp = GetComponent<AIBattle>();
    }

    void Start()
    {
        // 이동 제약 조건.
        moveMask = StateMask.ATTACKING | StateMask.DAMAGED | StateMask.STUNNED;
    }

    // Update is called once per frame
    void Update()
    {
        if (ai.HasState(moveMask) == true || battleComp.targetIsInRange)
        {
            navMeshAgent.enabled = false;
            ai.RemoveState(StateMask.RUNNING);
        }
        else
        {
            navMeshAgent.enabled = true;
            ai.AddState(StateMask.RUNNING);
            MoveToTarget();
        }

        TriggerAnimations();
    }

    void MoveToTarget()
    {
        Vector3 direction = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward * direction.x);

        navMeshAgent.destination = target.position;
    }

    void TriggerAnimations()
    {
        if (ai.HasState(StateMask.RUNNING))
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }
}
