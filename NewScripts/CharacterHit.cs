using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  캐릭터의 피격을 처리하는 클래스.
 *  캐릭터가 피해를 입거나 디버프에 걸리는 상태에 대한 로직과 함께 관련 애니메이션 재생도 처리함.
 *  공격의 경우 여러 타입과 구성이 있기 때문에 실질적인 공격 로직을 담당하는 클래스와 이를 애니메이션에 연결 시켜주는 클래스를 따로 만들었으나(PlayerAttack 등),
 *  피격이나 디버프에 걸리는 상태의 경우 각 개별 공격에 대한 피격, 디버프만 처리해주면 되기 때문에 간단하게 관련 로직과 애니메이션 재생을 이 클래스에 한 번에 처리해주며,
 *  플레이어와 AI 모두 피격, 디버프에 걸릴 수 있기 때문에 플레이어와 AI 클래스를 따로 나누지 않았음.
 */
public class CharacterHit : MonoBehaviour
{
    [Header("Hit Settings")]
    [SerializeField] int maxHitCount;
    [SerializeField] string hitAnimTriggerPrefix;
    [SerializeField] float hitCountTime;
    [Header("Invincible Settings")]
    [SerializeField] bool turnOnInvincible; // 마지막 최대 피격 횟수에 다다를 경우에 무적 효과를 켜줄지 결정함.
    [SerializeField] float invincibleDuration;

    // 피격 애니메이션 트리거 번호가 1번 부터 시작하기 때문에 피격 카운트는 1부터 시작.
    int currentHitCount = 1;

    Animator animator;
    Rigidbody rigidBody;
    CharacterState characterState;
    CharacterEnergy characterEnergy;
    SpriteRenderer spriteRenderer;

    Coroutine hitTimerCoroutine;
    Coroutine debuffTimerCoroutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        characterState = GetComponent<CharacterState>();
        characterEnergy = GetComponent<CharacterEnergy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Attack 유형 Spell에 대한 피격을 처리하는 함수.
    public void TakeDamage(in Vector3 knockBackDirecton, in Vector3 knockBackScale, in int damageAmount, in int hitCount)
    {
        // 피격 대상이 되는 캐릭터가 무적 상태이거나, 실제로 체력이 낮아지지 않았으면 그냥 리턴해줌.
        if (characterState.HasState(StateMask.INVINCIBLE)) return;
        if (characterEnergy.DecreaseHealth(damageAmount) == false) return;

        // 데미지를 입은 경우에 또 다시 피격을 입으면 피격 카운트를 증가 시켜 줌.
        if(characterState.HasState(StateMask.DAMAGED))
            currentHitCount += hitCount;

        // 넉백 적용.
        rigidBody.AddForce(Vector3.Scale(new Vector3(knockBackDirecton.x, 1, knockBackDirecton.z), knockBackScale), ForceMode.Impulse);

        // 최대 피격 횟수에 다다를 경우에 무적 상태를 적용함.
        if (currentHitCount == maxHitCount)
        {
            if(turnOnInvincible)
            {
                characterState.AddState(StateMask.INVINCIBLE);
                StartCoroutine(ResetInvincible());
            }
        }
        if (currentHitCount > maxHitCount)
            currentHitCount = 1;

        // 피격 횟수에 맞는 애니메이션 트리거 재생.
        animator.SetTrigger(hitAnimTriggerPrefix + currentHitCount);
    }

    public void TakeDebuff()
    {

    }

    #region Animation Event Functions
    /* Attack */
    public void Anim_EnterHit()
    {
        // DAMAGED 상태가 활성화 된 동안에 공격을 맞을 경우 피격 횟수가 증가함.
        characterState.AddState(StateMask.DAMAGED);

        // 기존에 피격 코루틴이 돌아가고 있는 경우 중단해줌.
        if(hitTimerCoroutine != null)
            StopCoroutine(hitTimerCoroutine);
    }

    public void Anim_ExitHit()
    {
        hitTimerCoroutine = StartCoroutine(ResetHitCount());
    }
    #endregion Animation Event Functions

    #region Coroutines
    IEnumerator ResetHitCount()
    {
        yield return new WaitForSeconds(hitCountTime);
        characterState.RemoveState(StateMask.DAMAGED);
        currentHitCount = 1;
    }
    IEnumerator ResetInvincible()
    {
        spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        yield return new WaitForSeconds(invincibleDuration);
        spriteRenderer.color = Color.white;
        characterState.RemoveState(StateMask.INVINCIBLE);
    }
    #endregion Coroutines
}
