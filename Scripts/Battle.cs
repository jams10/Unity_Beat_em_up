using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [SerializeField] protected string attackTargetTag;
    [SerializeField] protected LayerMask layersToIgnore;
    [SerializeField] private bool toggleDebug;
    [SerializeField] private float invincibleDuration;

    protected List<Attack> attackList;
    protected Attack currentAttack;

    protected Animator animator;
    protected Rigidbody rigidBody;
    protected SpriteRenderer spriteRenderer;
    protected CustomCharacter character;

    protected StateMask stopAttackMask;
    public bool targetIsInRange;
    public Vector3 lookAtVector;

    bool isSpriteFlashing; 

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        character = GetComponent<CustomCharacter>();
    }

    protected virtual void Start()
    {
        // 공격을 할 수 없는 경우를 체크하는 데 사용할 state 마스크.
        // 플레이어, AI에 따라 기본 마스크에 조건을 추가함.
        stopAttackMask = StateMask.DEAD | StateMask.STUNNED | StateMask.DAMAGED | StateMask.DEFENDING;
    }

    protected virtual void Update()
    {
        lookAtVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        // 무적 상태인 경우 스프라이트 깜빡임 효과 재생.
        if (isSpriteFlashing == false && character.HasState(StateMask.INVINCIBLE) == true)
        {
            isSpriteFlashing = true;
            StartCoroutine(FlashSprite());
        }

        // 자식 클래스에서 입력에 따라 currentAttack 값 변경. 공격 로직 수행.
    }

    #region Helper Functions
    // 공격 범위 안에 공격 대상이 있는지 체크하는 변수.
    public bool CheckTargetIsInRange()
    {
        Collider[] colliders = Physics.OverlapBox(
           transform.position + lookAtVector * currentAttack.attackRangeBox.offsetX, 
           currentAttack.attackRangeBox.scale / 2, 
           transform.rotation, ~layersToIgnore);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == attackTargetTag)
            {
                Battle target = colliders[i].GetComponent<Battle>();
                if (target != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public IEnumerator CanAttack(Attack attack)
    {
        yield return new WaitForSeconds(attack.cooldown);
        attack.canAttack = true;
    }

    IEnumerator ResetInvincible()
    {
        yield return new WaitForSeconds(invincibleDuration);
        character.RemoveState(StateMask.INVINCIBLE);
    }

    IEnumerator FlashSprite()
    {
        spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.25f);
        isSpriteFlashing = false;
    }
    #endregion Helper Functions

    #region Attack Functions
    // [애니메이션 호출 함수] : 공격 처리 함수. 공격 범위 안에 있는 대상들에 데미지를 가함.
    public void DoAttack()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position + lookAtVector * currentAttack.attackRangeBox.offsetX,
            currentAttack.attackRangeBox.scale / 2,
            transform.rotation, ~layersToIgnore);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == attackTargetTag)
            {
                Battle target = colliders[i].GetComponent<Battle>();
                if (target != null)
                {
                    // 공격 대상에 대한 EnterDamaged 함수를 호출해서 실제로 대상이 데미지를 받도록 함.
                    target.EnterDamaged(currentAttack.GetCurrentAttackUnit(), transform.position);
                }
            }
        }
    }
    // [애니메이션 호출 함수] : 콤보 공격 범위 안에 들어오는 경우 콤보 스택을 하나 증가 시킴.
    public virtual void EnterCombo()
    {
        currentAttack.IncreaseComboStack();
    }
    // [애니메이션 호출 함수] : 콤보 공격 범위 밖으로 나가면 콤보 스택을 초기화.
    public virtual void ExitCombo()
    {
        currentAttack.ResetComboStack();
    }
    // [애니메이션 호출 함수] : 공격 애니메이션이 끝나면 공격 관련 값들을 초기화.
    public virtual void ExitAttack()
    {
        character.RemoveState(StateMask.ATTACKING);
    }
    #endregion Attack Functions

    #region Damaged Functions
    // 데미지 받는 것을 처리하는 함수. 데미지를 가하는 주체가 공격 대상의 EnterDamaged 함수를 호출함에 유의.
    protected virtual void EnterDamaged(Attack.Unit attackerAttackUnit, Vector3 attackerPosition)
    {
        // 데미지를 받는 대상이 무적 상태이면 그냥 리턴.
        if (character.HasState(StateMask.INVINCIBLE) == true) return;
        // 데미지를 실제로 받았는지 먼저 체크함.
        if (character != null && character.TakeDamage(currentAttack.damage))
        {
            // 히트 애니메이션 트리거, 맞을 때 경직 효과를 위해 DAMAGED 상태 추가.
            // 경직 상태가 아닌 기절 상태의 경우 STUNNED 상태를 사용함.
            animator.SetTrigger(attackerAttackUnit.damagedTrigger);
            character.AddState(StateMask.DAMAGED);
            // 넉백을 가함.
            Vector3 force = (transform.position - attackerPosition).normalized;
            rigidBody.AddForce(new Vector3(force.x, 1, 0) * attackerAttackUnit.knockback, ForceMode.Impulse);
            // 무적 효과 발동.
            if(attackerAttackUnit.giveInvincible == true)
            {
                character.AddState(StateMask.INVINCIBLE);
                StartCoroutine(ResetInvincible());
            }
        }
    }
    // [애니메이션 호출 함수] : 데미지를 받을 때 재생되는 애니메이션 마지막에 호출되는 함수.
    public virtual void ExitDamaged()
    {
        // 공격 애니메이션 재생 중에 맞게 될 경우, 다시 공격 상태를 체크하기 위해 공격 관련 값들도 초기화 해줌.
        ExitCombo();
        ExitAttack();
        character.RemoveState(StateMask.DAMAGED);
    }
    #endregion Damaged Functions

    #region Debug Functions
    void OnDrawGizmos()
    {
        if(toggleDebug && currentAttack != null)
        {
            //Gizmos.matrix = Matrix4x4.TRS(transform.position + lookAt * attackBoxOffsetX, transform.rotation, attackBoxScale);
            if (targetIsInRange) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + lookAtVector * currentAttack.attackRangeBox.offsetX, currentAttack.attackRangeBox.scale);
        }
    }
    #endregion Debug Functions
}
