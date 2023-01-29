using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBattle : Battle
{
    [SerializeField] private Attack lightAttackPrefab;
    [SerializeField] private Attack heavyAttackPrefab;
    [SerializeField] private string GatheringTriggerName;
    [SerializeField] private string GatheringReadyTriggerName;
    [SerializeField] private string GatheringCancelTriggerName;

    Attack lightAttack;
    Attack heavyAttack;
    bool isCheckingCombo;      // �޺� ������ �õ� ������ üũ�ϴ� ����. �޺� ���� ������ ���� �� �����ϸ� false�� �ٲ�.
    bool isGathering;          // �⸦ ������ ���� ����.
    bool isReadyToHeavyAttack; // ����� �⸦ ��Ƽ� ������ ������ ����.

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
    }

    void FixedUpdate()
    {
        if(isRootMotionPlaying)
        {
            rigidBody.AddForce(lookAtVector * currentAttack.rootMotionDeltaX, ForceMode.Impulse);
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

    public override void EnterRootMotion()
    {
        base.EnterRootMotion();
    }

    public override void ExitRootMotion()
    {
        base.ExitRootMotion();
    }

    protected override void EnterDamaged(in Attack.Unit attackerAttackUnit, in Vector2 knockBackScale, in Vector3 attackerPosition)
    {
        base.EnterDamaged(attackerAttackUnit, knockBackScale, attackerPosition);
        isGathering = false;
        isReadyToHeavyAttack = false;
    }

    public override void ExitDamaged()
    { 
        base.ExitDamaged();
        isCheckingCombo = false;
    }

    public void Input_LightAttack(InputAction.CallbackContext context)
    {
        if (character.HasState(stopAttackMask) == true) return;

        // ���� ���.
        if (context.action.phase == InputActionPhase.Performed)
        {
            // �������� ��� ���� ���� �ٸ��� ó��.

            // �������� ��� ���� ���� ��� �� ���� ����.
            if (isCheckingCombo == false && lightAttack != null)
            {
                if (currentAttack != lightAttack) // ������ �ٸ� ������ ������ ��� ���� ������ �޺� ���� �ʱ�ȭ.
                {
                    currentAttack.ResetComboStack();
                }
                character.AddState(StateMask.ATTACKING); // ���� ���� �߰�.
                currentAttack = lightAttack;
                isCheckingCombo = true;
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
        }
    }

    public void Input_GatheringEnergy(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            animator.SetTrigger(GatheringTriggerName);
            character.AddState(StateMask.GATHERING);
            isGathering = true;
        }
        // ���߿� Ű�� �����µ��� ��� ���ư��� ���� �ذ��ؾ� ��.
        // ���߿� Ű�� ���� ó���� ��ġ �� �ִϸ��̼��� ���� attacking�� true�� �Ǿ��µ�, ���߿� ������� �ִϸ��̼����� ��ȯ�Ǵ� ���
        // �� attacking ���°� �����ǰ� ��. ���� ���߿� Ű�� ���� attacking�� ������� �ʾ� �������� �Ұ�����.
        // ����, �ϴ� �ִϸ��̼ǿ��� any state���� �������� ���� �� exit duration�� 1�� ������ 1�ʰ� �ִϸ��̼��� Ǯ�� ����ǵ��� �����
        // ��ġ �ִϸ��̼��� exit attack �Լ��� ȣ��ǵ��� ����.
        // �ִϸ��̼� ���̰� 1�� �Ѿ �� �ֱ� ������ Ű�� �� �� remove state�� attacking�� ���� �־���.
        // �ִϸ��̼� ���̸� 1�� �������־ exit attack ������ attacking�� remove �Ǳ� ������ ������ add state�� attacking�� ���൵
        // ���� �ð��� ó���Ǳ� ������ attacking�� ���� �����. gathering ���� ���� ������ �� �� ����.
        //if(context.action.phase == InputActionPhase.Canceled)
        //{
        //    animator.SetTrigger(GatheringCancelTriggerName);
        //    isGathering = false;
        //}
    }

    public void Input_HeavyAttack(InputAction.CallbackContext context)
    {
        if (isGathering == false) return;

        // �Է� �������� �־��� �� ��ŭ ����� Ű�� Ȧ���� ���.
        if (context.action.phase == InputActionPhase.Performed)
        {
            // ���� �غ� �����ٴ� �ִϸ��̼� ��� ����.
            animator.SetTrigger(GatheringReadyTriggerName);
            isReadyToHeavyAttack = true;
        }

        // Ȧ�� �߿� Ű�� �� ���. ����� Ȧ���� ���� �׷��� ���� ���� ����.
        if (context.action.phase == InputActionPhase.Canceled)
        {
            if (isReadyToHeavyAttack == true && heavyAttack != null)
            {
                // heavy attack ����.
                isReadyToHeavyAttack = false;

                character.AddState(StateMask.ATTACKING); // ���� ���� �߰�.
                currentAttack = heavyAttack;

                // ��Ʈ ����� ������ �ִϸ��̼��� ��� �ִϸ��̼� ���۰� ������ ��Ʈ��� �Լ� ȣ���ϴ� ������� �����ϱ�.
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
            else
            {
                animator.SetTrigger(GatheringCancelTriggerName);
                // Exit Time�� 1�ʷ� ���� ������, gathering ���·� ���� �������� Exit Attack�� ȣ������ ���� �� �����Ƿ�, ATTACKING ���µ� ��������.
                character.RemoveState(StateMask.ATTACKING);
                isGathering = false;
            }
            // gathering ���� ����.
            character.RemoveState(StateMask.GATHERING);
        }
    }
}
