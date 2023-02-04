using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastDebuff : MonoBehaviour
{
    [SerializeField] DebuffSO debuff;
    [SerializeField] string animTrigger;

    [SerializeField] float debuffCooldown;
    [SerializeField] LayerMask layersToAttack;

    Animator animator;

    Coroutine cooldownCoroutine;

    // AI의 경우 디버프 스킬이 쿨다운에 들어가면 다른 공격 혹은 스킬을 선택할 수 있도록 쿨다운 중인지 여부를 외부에서 접근할 수 있도록 함.
    bool _inCooldown; public bool inCoolDown { get { return _inCooldown; } }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAnimator(Animator animatorInCharacter)
    {
        animator = animatorInCharacter;
    }

    public void DoAnim()
    {
        if (animator != null && animTrigger != "")
            animator.SetTrigger(animTrigger);
    }

    // 디버프를 적용할 타겟을 바로 입력으로 넘겨주는 경우.
    public void DoDebuff(GameObject targetToApply)
    {
        // 피격 관련 컴포넌트를 가지고 있는 경우 수행해줌.
        CharacterHit target = targetToApply.GetComponent<CharacterHit>();
        if (target != null)
        {
            target.TakeDebuff(debuff.debuffType, debuff.effectDuration, debuff.effectTick);
        }
    }
    // 디버프 시전 캐릭터 위치 기준 디버프 스펠의 영역을 콜리젼 체크를 통해 타겟에 디버프를 적용함.
    public void DoDebuff(in Vector3 characterPosition, in Vector3 lookAtVector, in LayerMask layersToDetect)
    {
        Vector3 center = characterPosition + debuff.areaBoxOffsetX * lookAtVector;
        Collider[] colliders = Physics.OverlapBox(center, debuff.areaBoxScale / 2, Quaternion.identity, layersToDetect);

        foreach (Collider collider in colliders)
        {
            // 피격 관련 컴포넌트를 가지고 있는 경우 수행해줌.
            CharacterHit target = collider.GetComponent<CharacterHit>();
            if (target != null)
            {
                target.TakeDebuff(debuff.debuffType, debuff.effectDuration, debuff.effectTick);
            }
        }
    }

    public virtual void GetRangeBox(in Vector3 lookAtVector, in Vector3 characterPosition, out Vector3 center, out Vector3 size)
    {
        center = characterPosition + debuff.areaBoxOffsetX * lookAtVector;
        size = debuff.areaBoxScale;
    }
    #region Animation Event Functions
    public void Anim_EnterDebuff()
    {
        
    }

    public void Anim_ExitDebuff()
    {
        _inCooldown = true;
        cooldownCoroutine = StartCoroutine(ResetCooldown());
    }

    public void Anim_DoDebuff()
    {
        DoDebuff(transform.position, Quaternion.Euler(0, gameObject.transform.eulerAngles.y, 0) * Vector3.right, layersToAttack);
    }
    #endregion Animation Event Functions

    #region Coroutine
    IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(debuffCooldown);
        _inCooldown = false;
    }
    #endregion Coroutine
}
