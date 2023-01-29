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

        // AI�� ��� �� ���� ���ݵ��� �ϳ��� �ִϸ��̼ǿ� ���ļ� ���� ���̱� ������ �� �� ���� �ִϸ��̼��� ��� �� �ڿ� �ٽ� ������� �ʵ��� ��.
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

        if (character.HasState(stopAttackMask) == false && targetIsInRange)
        {
            if (heavyAttack != null && heavyAttack.canAttack == true)
            {
                // ���� ���� �߰�.
                character.AddState(StateMask.ATTACKING);
                currentAttack = heavyAttack;
                currentAttack.canAttack = false;
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
            // else-if�� �ƴ� if�� �� ���, currentAttack�� lightAttack���� �ٲ������� ����.
            else if (lightAttack != null && lightAttack.canAttack == true)
            {
                // ���� ���� �߰�.
                character.AddState(StateMask.ATTACKING);
                currentAttack = lightAttack;
                currentAttack.canAttack = false;
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
        }
    }

    public override void EnterCombo()
    {
        base.EnterCombo();
    }
    public override void ExitCombo()
    {
        base.ExitCombo();
    }

    public override void ExitAttack()
    {
        base.ExitAttack();
        StartCoroutine(CanAttack(currentAttack));
    }

    protected override void EnterDamaged(in Attack.Unit attackerAttackUnit, in Vector2 knockBackScale, in Vector3 attackerPosition)
    {
        base.EnterDamaged(attackerAttackUnit, knockBackScale, attackerPosition);
    }

    public override void ExitDamaged()
    {
        base.ExitDamaged();
    }
}
