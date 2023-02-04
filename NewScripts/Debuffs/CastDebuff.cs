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

    // AI�� ��� ����� ��ų�� ��ٿ ���� �ٸ� ���� Ȥ�� ��ų�� ������ �� �ֵ��� ��ٿ� ������ ���θ� �ܺο��� ������ �� �ֵ��� ��.
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

    // ������� ������ Ÿ���� �ٷ� �Է����� �Ѱ��ִ� ���.
    public void DoDebuff(GameObject targetToApply)
    {
        // �ǰ� ���� ������Ʈ�� ������ �ִ� ��� ��������.
        CharacterHit target = targetToApply.GetComponent<CharacterHit>();
        if (target != null)
        {
            target.TakeDebuff(debuff.debuffType, debuff.effectDuration, debuff.effectTick);
        }
    }
    // ����� ���� ĳ���� ��ġ ���� ����� ������ ������ �ݸ��� üũ�� ���� Ÿ�ٿ� ������� ������.
    public void DoDebuff(in Vector3 characterPosition, in Vector3 lookAtVector, in LayerMask layersToDetect)
    {
        Vector3 center = characterPosition + debuff.areaBoxOffsetX * lookAtVector;
        Collider[] colliders = Physics.OverlapBox(center, debuff.areaBoxScale / 2, Quaternion.identity, layersToDetect);

        foreach (Collider collider in colliders)
        {
            // �ǰ� ���� ������Ʈ�� ������ �ִ� ��� ��������.
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
