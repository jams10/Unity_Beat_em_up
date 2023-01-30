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

        // AI의 경우 플레이어와 다르게 마지막 공격을 제외한 애니메이션 맨 끝에 EnterCombo를 넣어서 콤보 공격을 이어 나가도록 함.
        // 이 때, AI의 경우 Update에서 계속 공격을 시도하기 때문에 EnterCombo를 만났을 때 ATTACKING 상태를 제거해서 해당 시점 부터 공격을 이어 나갈 수 있도록 함.
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
                // 공격 상태 추가.
                character.AddState(StateMask.ATTACKING);
                currentAttack = heavyAttack;
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
            // else-if가 아닌 if로 할 경우, currentAttack이 lightAttack으로 바뀌어버림에 유의.
            else if (lightAttack != null && lightAttack.canAttack == true)
            {
                // 공격 상태 추가.
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
        // 공격 재사용 대기 시간에 들어감.
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
