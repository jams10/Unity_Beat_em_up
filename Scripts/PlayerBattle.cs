using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattle : Battle
{
    [SerializeField] private Attack lightAttackPrefab;
    [SerializeField] private Attack heavyAttackPrefab;

    Attack lightAttack;
    Attack heavyAttack;
    bool isCheckingCombo; // �޺� ������ �õ� ������ üũ�ϴ� ����. �޺� ���� ������ ���� �� �����ϸ� false�� �ٲ�.

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
                if(currentAttack != lightAttack) // ������ �ٸ� ������ ������ ��� ���� ������ �޺� ���� �ʱ�ȭ.
                {
                    currentAttack.ResetComboStack();
                }
                character.AddState(StateMask.ATTACKING); // ���� ���� �߰�.
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
