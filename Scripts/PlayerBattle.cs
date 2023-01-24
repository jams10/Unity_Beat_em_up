using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattle : Battle
{
    [SerializeField] private Attack lightAttackPrefab;
    [SerializeField] private Attack heavyAttackPrefab;

    Attack lightAttack;
    Attack heavyAttack;
    bool isCheckingCombo; // 콤보 공격을 시도 중인지 체크하는 변수. 콤보 범위 안으로 들어올 때 공격하면 false로 바뀜.

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        lightAttack = Instantiate(lightAttackPrefab);
        heavyAttack = Instantiate(heavyAttackPrefab);

        if(lightAttack != null)
            currentAttack = lightAttack;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(character.HasState(stopAttackMask) == false && isCheckingCombo == false)
        {
            if (lightAttack != null && Input.GetButtonDown("Fire1"))
            {
                if(currentAttack != lightAttack) // 이전과 다른 유형의 공격일 경우 이전 공격의 콤보 스택 초기화.
                {
                    currentAttack.ResetComboStack();
                }
                character.AddState(StateMask.ATTACKING); // 공격 상태 추가.
                currentAttack = lightAttack;
                isCheckingCombo = true;  
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
            if(heavyAttack != null && Input.GetButtonDown("Fire2"))
            {
                if (currentAttack != heavyAttack)
                {
                    currentAttack.ResetComboStack();
                }
                character.AddState(StateMask.ATTACKING);
                currentAttack = heavyAttack;
                isCheckingCombo = true;
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
        }
    }

    public override void EnterCombo()
    {
        base.EnterCombo();
        isCheckingCombo = false;
    }

    public override void ExitCombo()
    {
        base.ExitCombo();
        isCheckingCombo = false;
    }

    public override void ExitAttack()
    {
        base.ExitAttack();
    }

    protected override void EnterDamaged(Attack.Unit attackerAttackUnit, Vector3 attackerPosition)
    { 
        base.EnterDamaged(attackerAttackUnit, attackerPosition);
    }

    public override void ExitDamaged()
    { 
        base.ExitDamaged();
        isCheckingCombo = false;
    }
}
