using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// "영역에 있는 타겟들을 대상으로 하는 콤보 공격"
// "애니메이션 트리거를 포함하고 있기 때문에 애니메이션이 필요한 단일 공격도 Attack을 하나만 추가해서 콤보 공격을 사용하면 된다."
public class ComboAttack : Attack
{
    [System.Serializable]
    public struct AttackUnit
    {
        public AttackSO attack;
        public string animTrigger;
    }

    [SerializeField] float attackCooldown;
    [SerializeField] List<AttackUnit> attackUnits;

    Animator animator;

    int currentComboCount;
    int maxComboCount;
    bool isCheckingCombo;

    // AI의 경우 공격이 쿨다운에 들어가면 다른 공격을 선택할 수 있도록 쿨다운 중인지 여부를 외부에서 접근할 수 있도록 함.
    bool _inCooldown; public bool inCoolDown { get { return _inCooldown; } }

    Coroutine cooldownCoroutine;

    void Start()
    {
        maxComboCount = attackUnits.Count;
    }

    public void SetAnimator(Animator animatorInCharacter)
    {
        animator = animatorInCharacter;
    }

    public void DoAnim()
    {
        /*
         * 공격 애니메이션이 시작 되면 EnterCombo, ExitCombo 호출 사이의 콤보 체크 영역 밖에 있는 경우에는 isCheckingCombo가 true 값이 됨.
         * 따라서 콤보 영역 밖에 있는 경우에는 다시 공격 버튼을 눌러도 다음 콤보 공격으로 넘어가지 않으며, 콤보 공격 체크 영역 안에 들어올 때 비로소 isCheckingCombo 값이
         * false가 되기 때문에 공격 버튼을 누르면 다음 콤보 공격 재생을 위한 애니메이션 트리거가 발동됨.
         */
        if (isCheckingCombo == true || _inCooldown == true) return;

        if (animator != null && attackUnits[currentComboCount].animTrigger != "")
            animator.SetTrigger(attackUnits[currentComboCount].animTrigger);

        isCheckingCombo = true;
    }

    public override void DoAttack(in Vector3 characterPosition, in Vector3 lookAtVector, in LayerMask layersToDetect)
    {
        // AttackSO에 저장된 박스 오프셋, 스케일 값과 레이어 마스크를 사용하여 해당 박스 영역에 대해 콜리젼 체크를 수행함.
        // OverlapBox는 박스의 절반 크기(halfExtent)를 받아주는 것에 유의.
        Vector3 center = characterPosition + attackUnits[currentComboCount].attack.areaBoxOffsetX * lookAtVector;
        Collider[] colliders = Physics.OverlapBox(center, attackUnits[currentComboCount].attack.areaBoxScale / 2, Quaternion.identity, layersToDetect);

        foreach (Collider collider in colliders)
        {
            // 데미지 관련 컴포넌트를 가지고 있는 경우 수행해줌.
            CharacterHit target = collider.GetComponent<CharacterHit>();
            if (target != null)
            {
                Vector3 knockBackDirection = (target.transform.position - characterPosition).normalized;
                target.TakeDamage(knockBackDirection,
                    attackUnits[currentComboCount].attack.knockBackScale,
                    attackUnits[currentComboCount].attack.damageAmount,
                    attackUnits[currentComboCount].attack.hitCount);
            }
        }
    }

    public void EnterCombo()
    {
        isCheckingCombo = false;
        // 콤보 증가.
        if (currentComboCount + 1 < maxComboCount)
            currentComboCount++;
        else
            currentComboCount = 0;
    }

    public void ExitCombo()
    {
        isCheckingCombo = false;

        currentComboCount = 0;
    }

    public override void ExitAttack()
    {
        _inCooldown = true;
        cooldownCoroutine = StartCoroutine(ResetCooldown());
    }

    IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        _inCooldown = false;
    }

    public override void GetRangeBox(in Vector3 characterPosition, in Vector3 lookAtVector, out Vector3 center, out Vector3 size)
    {
        center = characterPosition + attackUnits[currentComboCount].attack.areaBoxOffsetX * lookAtVector;
        size = attackUnits[currentComboCount].attack.areaBoxScale;
    }
}
