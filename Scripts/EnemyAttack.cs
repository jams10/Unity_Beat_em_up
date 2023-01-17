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
    [SerializeField] private AudioClip hitSound;

    Animator animator;
    Rigidbody rigidBody;
    AudioSource audioSource;
    List<Component> attackTargets;
    GameCharacter enemyCharacter;
    GameCharacter.CharacterStateMask attackMask;
    Vector3 lookAtVector;
    bool targetIsInRange;
    bool canAttack;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();   
        enemyCharacter = GetComponent<GameCharacter>();
    }

    void Start()
    {
        targetIsInRange = false;
        canAttack = true;
        attackTargets= new List<Component>();
        // 공격 제약 조건.
        attackMask = GameCharacter.CharacterStateMask.isAttacking
            | GameCharacter.CharacterStateMask.isDamaged;
            //| GameCharacter.CharacterStateMask.isRunning
    }

    void Update()
    {
        lookAtVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        targetIsInRange = false;

        IsTargetInAttackRange();

        if (targetIsInRange == true && canAttack == true && enemyCharacter.HasCharacterState(attackMask) == false)
        {
            EnterAttack();
        }
    }

    #region Helper Functions
    void IsTargetInAttackRange()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + lookAtVector * attackBoxOffsetX, attackBoxScale / 2, transform.rotation, ~layersToIgnore);
        attackTargets.Clear();

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == attackTargetTag)
            {
                PlayerAttack targetCharacter = colliders[i].GetComponent<PlayerAttack>();
                if (targetCharacter != null)
                {
                    targetIsInRange = true;
                    attackTargets.Add(targetCharacter);
                }
            }
        }
    }
    #endregion Helper Functions

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
        StartCoroutine(CanAttack(attackCoolDown));
    }

    public void ProcessAttack()
    {
        if(targetIsInRange)
        {
            foreach(PlayerAttack target in attackTargets)
            {
                target.TakeDamage(ligthAttackDamage, transform.position, 0);
            }
        }
    }

    IEnumerator CanAttack(float coolDown)
    {
        yield return new WaitForSeconds(coolDown);  
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
                rigidBody.AddForce(Vector3.up * 1.2f, ForceMode.VelocityChange);
            }
            rigidBody.AddForce(force * knockBackScale, ForceMode.Impulse);
            EnterDamaged(damageAnimIndex);
        }
    }
    void EnterDamaged(int damageAnimIndex)
    {
        animator.SetTrigger("isDamaged_" + damageAnimIndex);
        audioSource.clip = hitSound;
        audioSource.volume = 0.5f;
        audioSource.Play();
        enemyCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isStun);
        enemyCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isDamaged);
    }

    public void ExitDamaged()
    {
        // 캐릭터가 공격 중에 맞게 되면, 처음으로 돌아가 다시 공격 상태를 체크하기 위해 공격 관련 값들을 초기화 해줌.
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
