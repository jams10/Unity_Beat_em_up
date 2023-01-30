using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [SerializeField] protected string attackTargetTag;
    [SerializeField] protected LayerMask layersToIgnore;
    [SerializeField] private bool toggleDebug;
    [SerializeField] private float invincibleDuration;
    [SerializeField] private float hitDuration;
    [SerializeField] private int maxHitCount;
    [SerializeField] private string hitAnimTriggerPrefix;
    [SerializeField] private Vector2 hitKnockbackScale;

    protected List<Attack> attackList;
    protected Attack currentAttack;

    protected Animator animator;
    protected Rigidbody rigidBody;
    protected SpriteRenderer spriteRenderer;
    protected CustomCharacter character;
    protected ItemInteraction itemInteraction;

    protected StateMask stopAttackMask;
    protected bool isRootMotionPlaying;
    public bool targetIsInRange;

    bool isSpriteFlashing;
    bool hitResetCoroutineRunning;
    int currentHitCount;

    Coroutine hitCountResetCoroutine;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        character = GetComponent<CustomCharacter>();
        itemInteraction = GetComponent<ItemInteraction>();   
    }

    protected virtual void Start()
    {
        // 공격을 할 수 없는 경우를 체크하는 데 사용할 state 마스크.
        // 플레이어, AI에 따라 기본 마스크에 조건을 추가함.
        stopAttackMask = StateMask.DEAD | StateMask.STUNNED | StateMask.DEFENDING | StateMask.GRABBING;
    }

    protected virtual void Update()
    {

        // 무적 상태인 경우 스프라이트 깜빡임 효과 재생.
        if (isSpriteFlashing == false && character.HasState(StateMask.ATTACKING) == false && character.HasState(StateMask.INVINCIBLE) == true)
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
           transform.position + character.GetLookAtVector() * currentAttack.attackRangeBox.offsetX, 
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

    IEnumerator ResetHitCount()
    {
        hitResetCoroutineRunning = true;
        yield return new WaitForSeconds(hitDuration);
        character.RemoveState(StateMask.DAMAGED);
        currentHitCount = 0; // 피격 스택 초기화.
        hitResetCoroutineRunning = false;
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
            transform.position + character.GetLookAtVector() * currentAttack.attackRangeBox.offsetX,
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
                    target.EnterDamaged(currentAttack.GetCurrentAttackUnit(), currentAttack.knockBackScale, transform.position);
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
    // [애니메이션 호출 함수] : 루트 모션 시작.
    public virtual void EnterRootMotion()
    {
        isRootMotionPlaying = true;
        character.AddState(StateMask.INVINCIBLE);
    }
    // [애니메이션 호출 함수] : 루트 모션 끝.
    public virtual void ExitRootMotion()
    {
        isRootMotionPlaying = false;
        character.RemoveState(StateMask.INVINCIBLE);
    }
    #endregion Attack Functions

    #region Damaged Functions
    // 데미지 받는 것을 처리하는 함수. 데미지를 가하는 주체가 공격 대상의 EnterDamaged 함수를 호출함에 유의.
    protected virtual void EnterDamaged(in Attack.Unit attackerAttackUnit, in Vector2 knockBackScale, in Vector3 attackerPosition)
    {
        // 데미지를 받는 대상이 무적 상태이면 그냥 리턴.
        if (character.HasState(StateMask.INVINCIBLE) == true)
            return;
        // 데미지를 실제로 받았는지 먼저 체크함.
        if (character != null && character.TakeDamage(currentAttack.damage))
        {
            // 피격 스택을 증가시켜줌. 코루틴을 통해 피격 카운트가 초기화 되기 전 연속해서 피격을 맞을 경우 스택이 쌓이게 됨.
            currentHitCount += attackerAttackUnit.hitStackCount;

            // 히트 애니메이션 트리거, 맞을 때 경직 효과를 위해 STUN 상태 추가.
            // DAMAGED의 경우 피격 스택을 위해 사용. 공격을 하는 중에 맞을 경우에도 공격 상태를 제거해줌.
            character.AddState(StateMask.STUNNED);
            character.AddState(StateMask.DAMAGED);
            character.RemoveState(StateMask.ATTACKING);

            // 최대 피격 스택에 도달하는 경우.
            if (currentHitCount >= maxHitCount)
            {
                currentHitCount = maxHitCount;

                // 넉백을 가함.
                Vector3 force = (transform.position - attackerPosition).normalized;
                rigidBody.AddForce(new Vector3(force.x, 1, 0) * knockBackScale, ForceMode.Impulse);

                // 무적 효과 발동.
                character.AddState(StateMask.INVINCIBLE);
                StartCoroutine(ResetInvincible());
            }

            // 피격 카운트 스택에 맞는 애니메이션 트리거 재생.
            animator.SetTrigger(hitAnimTriggerPrefix + currentHitCount);
        }
    }
    // [애니메이션 호출 함수] : 데미지를 받을 때 재생되는 애니메이션 마지막에 호출되는 함수.
    public virtual void ExitDamaged()
    {
        // 공격 애니메이션 재생 중에 맞게 될 경우, 다시 공격 상태를 체크하기 위해 공격 관련 값들도 초기화 해줌.
        ExitCombo();
        //ExitAttack();
        character.RemoveState(StateMask.STUNNED);
        
        // 피격 애니메이션이 끝난 뒤에 짧은 시간 후에 DAMAGED 상태를 제거해줌.(짧은 시간 안에 한 번 더 맞을 경우 피격 스택을 증가 시킴.)
        // 코루틴 진행 중 한 번 더 맞을 경우, 기존 코루틴을 중지시키고 새로운 피격 시간을 기준으로 하는 코루틴 시작.
        if(hitResetCoroutineRunning == false)
            hitCountResetCoroutine = StartCoroutine(ResetHitCount());
        else
        {
            StopCoroutine(hitCountResetCoroutine);
            hitCountResetCoroutine = StartCoroutine(ResetHitCount());
        }
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
            Gizmos.DrawWireCube(transform.position + character.GetLookAtVector() * currentAttack.attackRangeBox.offsetX, currentAttack.attackRangeBox.scale);
        }
    }
    #endregion Debug Functions
}
