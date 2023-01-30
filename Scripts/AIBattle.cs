using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBattle : Battle
{
    [SerializeField] private Attack lightAttackPrefab;
    [SerializeField] private Attack heavyAttackPrefab;

    Attack lightAttack;
    Attack heavyAttack;

    protected override void Start()
    {
        base.Start();

        // AI�� ��� �÷��̾�� �ٸ��� ������ ������ ������ �ִϸ��̼� �� ���� EnterCombo�� �־ �޺� ������ �̾� �������� ��.
        // �� ��, AI�� ��� Update���� ��� ������ �õ��ϱ� ������ EnterCombo�� ������ �� ATTACKING ���¸� �����ؼ� �ش� ���� ���� ������ �̾� ���� �� �ֵ��� ��.
        stopAttackMask |= StateMask.ATTACKING;

        lightAttack = Instantiate(lightAttackPrefab);
        heavyAttack = Instantiate(heavyAttackPrefab);

        if (lightAttack != null)
            currentAttack = lightAttack;
    }

    protected override void Update()
    {
        base.Update();

        targetIsInRange = CheckTargetIsInRange();

        if (character.HasState(StateMask.RUNNING))
        {
            ExitCombo();
            ExitAttack();
        }

        if (character.HasState(stopAttackMask) == false && targetIsInRange)
        {
            if (heavyAttack != null && heavyAttack.canAttack == true)
            {
                // ���� ���� �߰�.
                character.AddState(StateMask.ATTACKING);
                currentAttack = heavyAttack;
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
            // else-if�� �ƴ� if�� �� ���, currentAttack�� lightAttack���� �ٲ������� ����.
            else if (lightAttack != null && lightAttack.canAttack == true)
            {
                // ���� ���� �߰�.
                character.AddState(StateMask.ATTACKING);
                currentAttack = lightAttack;
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
            Debug.Log(currentAttack.GetCurrentAttackUnit().attackTrigger);
        }
    }

    public override void EnterCombo()
    {
        base.EnterCombo();
        character.RemoveState(StateMask.ATTACKING);
    }
    public override void ExitCombo()
    {
        base.ExitCombo();
    }

    public override void ExitAttack()
    {
        base.ExitAttack();
        // ���� ���� ��� �ð��� ��.
        currentAttack.canAttack = false;
        StartCoroutine(CanAttack(currentAttack));
    }

    protected override void EnterDamaged(in Attack.Unit attackerAttackUnit, in Vector2 knockBackScale, in Vector3 attackerPosition)
    {
        base.EnterDamaged(attackerAttackUnit, knockBackScale, attackerPosition);
    }

    public override void ExitDamaged()
    {
        ExitAttack();
        base.ExitDamaged();
    }
}
