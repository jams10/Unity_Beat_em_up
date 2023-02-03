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

    [SerializeField] List<AttackUnit> attackUnits;

    [SerializeField] LayerMask layersToIgnore;

    Animator animator;

    int currentComboCount;
    int maxComboCount;
    bool isCheckingCombo;

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
        if (isCheckingCombo == true) return;

        if (animator != null && attackUnits[currentComboCount].animTrigger != "")
            animator.SetTrigger(attackUnits[currentComboCount].animTrigger);

        isCheckingCombo = true;
    }

    public override void DoAttack(in Vector3 characterPosition, in Vector3 lookAtVector)
    {
        // AttackSO�� ����� �ڽ� ������, ������ ���� ���̾� ����ũ�� ����Ͽ� �ش� �ڽ� ������ ���� �ݸ��� üũ�� ������.
        // OverlapBox�� �ڽ��� ���� ũ��(halfExtent)�� �޾��ִ� �Ϳ� ����.
        Vector3 center = characterPosition + attackUnits[currentComboCount].attack.areaBoxOffsetX * lookAtVector;
        Collider[] colliders = Physics.OverlapBox(center, attackUnits[currentComboCount].attack.areaBoxScale / 2, Quaternion.identity, ~layersToIgnore);

        foreach (Collider collider in colliders)
        {
            // ������ ���� ������Ʈ�� ������ �ִ� ��� ��������.
            CharacterHit target = collider.GetComponent<CharacterHit>();
            if (target != null)
            {
                Debug.Log(currentComboCount);
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

    public override void GetRangeBox(in Vector3 characterPosition, in Vector3 lookAtVector, out Vector3 center, out Vector3 size)
    {
        center = characterPosition + attackUnits[currentComboCount].attack.areaBoxOffsetX * lookAtVector;
        size = attackUnits[currentComboCount].attack.areaBoxScale;
    }
}
