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

        // AI의 경우 각 개별 공격들을 하나의 애니메이션에 합쳐서 넣을 것이기 때문에 한 번 공격 애니메이션을 재생 한 뒤에 다시 재생되지 않도록 함.
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
                // 공격 상태 추가.
                character.AddState(StateMask.ATTACKING);
                currentAttack = heavyAttack;
                currentAttack.canAttack = false;
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
            // else-if가 아닌 if로 할 경우, currentAttack이 lightAttack으로 바뀌어버림에 유의.
            else if (lightAttack != null && lightAttack.canAttack == true)
            {
                // 공격 상태 추가.
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
