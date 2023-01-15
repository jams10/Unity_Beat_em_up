using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private int maxLightAttacks;
    [SerializeField] private int ligthAttackDamage;
    [SerializeField] private float knockBackScale;
    [SerializeField] private float attackCoolDown;
    [SerializeField] private float attackBoxOffsetX;
    [SerializeField] private Vector3 attackBoxScale;
    [SerializeField] private string attackTargetTag;
    [SerializeField] private LayerMask layersToIgnore;

    Animator animator;
    Rigidbody rigidBody;
    GameCharacter enemyCharacter;
    GameCharacter.CharacterStateMask attackMask;
    Vector3 lookAtVector;
    bool targetIsInRange;
    bool canAttack;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        enemyCharacter = GetComponent<GameCharacter>();
    }

    void Start()
    {
        targetIsInRange = false;
        canAttack = true;
        // 공격 제약 조건.
        attackMask = GameCharacter.CharacterStateMask.isAttacking
            | GameCharacter.CharacterStateMask.isRunning
            | GameCharacter.CharacterStateMask.isDamaged;
    }

    void Update()
    {
        lookAtVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        if (enemyCharacter.HasCharacterState(GameCharacter.CharacterStateMask.isDamaged))
        {
            rigidBody.AddForce(-lookAtVector * knockBackScale, ForceMode.Impulse);
            EnterDamaged();
        }

        targetIsInRange = false;
        if (canAttack == true && enemyCharacter.HasCharacterState(attackMask) == false)
        {
            EnterAttack();
        }
    }
    #region Attack Process Functions
    void EnterAttack()
    {
        canAttack = false;
        enemyCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isAttacking);
        animator.SetTrigger("attack");
    }

    public void ExitAttack()
    {
        enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isAttacking);
        Invoke("CanAttack", attackCoolDown);
    }

    public void ProcessAttack()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + lookAtVector * attackBoxOffsetX, attackBoxScale / 2, transform.rotation, ~layersToIgnore);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == attackTargetTag)
            {
                GameCharacter targetCharacter = colliders[i].GetComponent<GameCharacter>();
                if (targetCharacter != null)
                {
                    targetIsInRange = true;
                    if (targetCharacter.TakeDamage(ligthAttackDamage))
                    {
                        targetCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isDamaged);
                    }
                }
            }
        }
    }

    void CanAttack()
    {
        canAttack = true;
    }
    #endregion Attack Process Functions

    #region Damaged Process Functions
    void EnterDamaged()
    {
        animator.SetTrigger("isDamaged");
        enemyCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isStun);
        enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isDamaged);
    }

    public void ExitDamaged()
    {
        // 캐릭터가 공격 중에 맞게 되면, 처음으로 돌아가 다시 공격 상태를 체크하기 위해 공격 관련 값들을 초기화 해줌.
        enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isAttacking);
        enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isStun);
        canAttack = true;
    }
    #endregion Damaged Process Functions

    void OnDrawGizmos()
    {
        //Gizmos.matrix = Matrix4x4.TRS(transform.position + lookAt * attackBoxOffsetX, transform.rotation, attackBoxScale);
        if (targetIsInRange) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + lookAtVector * attackBoxOffsetX, attackBoxScale);
    }
}
