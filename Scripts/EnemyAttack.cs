using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
        // ���� ���� ����.
        attackMask = GameCharacter.CharacterStateMask.isAttacking
            | GameCharacter.CharacterStateMask.isRunning
            | GameCharacter.CharacterStateMask.isDamaged;
    }

    void Update()
    {
        lookAtVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

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
                PlayerAttack targetCharacter = colliders[i].GetComponent<PlayerAttack>();
                if (targetCharacter != null)
                {
                    targetIsInRange = true;
                    targetCharacter.TakeDamage(ligthAttackDamage, transform.position, 0);
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
    public void TakeDamage(int damage, Vector3 attackerPosition, int damageAnimIndex)
    {
        if(enemyCharacter.TakeDamage(damage))
        {
            enemyCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isDamaged);
            Vector3 force = new Vector3((transform.position - attackerPosition).normalized.x, 0, 0);
            if (damageAnimIndex >= 2)
            {
                force *= 2.0f;
                rigidBody.AddForce(Vector3.up, ForceMode.VelocityChange);
            }
            rigidBody.AddForce(force * knockBackScale, ForceMode.Impulse);
            EnterDamaged(damageAnimIndex);
        }
    }
    void EnterDamaged(int damageAnimIndex)
    {
        animator.SetTrigger("isDamaged_" + damageAnimIndex);
        enemyCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isStun);
        enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isDamaged);
    }

    public void ExitDamaged()
    {
        // ĳ���Ͱ� ���� �߿� �°� �Ǹ�, ó������ ���ư� �ٽ� ���� ���¸� üũ�ϱ� ���� ���� ���� ������ �ʱ�ȭ ����.
        enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isAttacking);
        canAttack = true;
        enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isStun);
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
