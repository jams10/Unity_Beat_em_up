using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// "������ �ִ� Ÿ�ٵ��� ������� �ϴ� �޺� ����"
// "�ִϸ��̼� Ʈ���Ÿ� �����ϰ� �ֱ� ������ �ִϸ��̼��� �ʿ��� ���� ���ݵ� Attack�� �ϳ��� �߰��ؼ� �޺� ������ ����ϸ� �ȴ�."
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

    // AI�� ��� ������ ��ٿ ���� �ٸ� ������ ������ �� �ֵ��� ��ٿ� ������ ���θ� �ܺο��� ������ �� �ֵ��� ��.
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
         * ���� �ִϸ��̼��� ���� �Ǹ� EnterCombo, ExitCombo ȣ�� ������ �޺� üũ ���� �ۿ� �ִ� ��쿡�� isCheckingCombo�� true ���� ��.
         * ���� �޺� ���� �ۿ� �ִ� ��쿡�� �ٽ� ���� ��ư�� ������ ���� �޺� �������� �Ѿ�� ������, �޺� ���� üũ ���� �ȿ� ���� �� ��μ� isCheckingCombo ����
         * false�� �Ǳ� ������ ���� ��ư�� ������ ���� �޺� ���� ����� ���� �ִϸ��̼� Ʈ���Ű� �ߵ���.
         */
        if (isCheckingCombo == true || _inCooldown == true) return;

        if (animator != null && attackUnits[currentComboCount].animTrigger != "")
            animator.SetTrigger(attackUnits[currentComboCount].animTrigger);

        isCheckingCombo = true;
    }

    public override void DoAttack(in Vector3 characterPosition, in Vector3 lookAtVector, in LayerMask layersToDetect)
    {
        // AttackSO�� ����� �ڽ� ������, ������ ���� ���̾� ����ũ�� ����Ͽ� �ش� �ڽ� ������ ���� �ݸ��� üũ�� ������.
        // OverlapBox�� �ڽ��� ���� ũ��(halfExtent)�� �޾��ִ� �Ϳ� ����.
        Vector3 center = characterPosition + attackUnits[currentComboCount].attack.areaBoxOffsetX * lookAtVector;
        Collider[] colliders = Physics.OverlapBox(center, attackUnits[currentComboCount].attack.areaBoxScale / 2, Quaternion.identity, layersToDetect);

        foreach (Collider collider in colliders)
        {
            // ������ ���� ������Ʈ�� ������ �ִ� ��� ��������.
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
        // �޺� ����.
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
