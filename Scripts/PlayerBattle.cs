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
    bool isCheckingCombo;      // 콤보 공격을 시도 중인지 체크하는 변수. 콤보 범위 안으로 들어올 때 공격하면 false로 바뀜.
    bool isGathering;          // 기를 모으는 중인 상태.
    bool isReadyToHeavyAttack; // 충분히 기를 모아서 공격이 가능한 상태.

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

        // 누른 경우.
        if (context.action.phase == InputActionPhase.Performed)
        {
            // 아이템을 들고 있을 때는 다르게 처리.

            // 아이템을 들고 있지 않은 경우 약 공격 수행.
            if (isCheckingCombo == false && lightAttack != null)
            {
                if (currentAttack != lightAttack) // 이전과 다른 유형의 공격일 경우 이전 공격의 콤보 스택 초기화.
                {
                    currentAttack.ResetComboStack();
                }
                character.AddState(StateMask.ATTACKING); // 공격 상태 추가.
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
        // 도중에 키를 떼었는데도 계속 돌아가는 문제 해결해야 함.
        // 도중에 키를 떼면 처음에 펀치 때 애니메이션을 통해 attacking이 true가 되었는데, 도중에 기모으기 애니메이션으로 전환되는 경우
        // 이 attacking 상태가 유지되게 됨. 따라서 도중에 키를 떼도 attacking이 사라지지 않아 움직임이 불가능함.
        // 따라서, 일단 애니메이션에서 any state에서 기모으기로 들어올 때 exit duration을 1로 설정해 1초간 애니메이션이 풀로 재생되도록 만들어
        // 펀치 애니메이션의 exit attack 함수가 호출되도록 했음.
        // 애니메이션 길이가 1이 넘어갈 수 있기 때문에 키를 뗄 떼 remove state로 attacking을 없애 주었음.
        // 애니메이션 길이를 1로 설정해주어서 exit attack 때문에 attacking이 remove 되기 때문에 위에서 add state로 attacking을 해줘도
        // 같은 시간에 처리되기 때문에 attacking이 같이 사라짐. gathering 상태 따로 만들어야 할 것 같음.
        //if(context.action.phase == InputActionPhase.Canceled)
        //{
        //    animator.SetTrigger(GatheringCancelTriggerName);
        //    isGathering = false;
        //}
    }

    public void Input_HeavyAttack(InputAction.CallbackContext context)
    {
        if (isGathering == false) return;

        // 입력 설정에서 넣어준 값 만큼 충분히 키를 홀딩한 경우.
        if (context.action.phase == InputActionPhase.Performed)
        {
            // 공격 준비가 끝났다는 애니메이션 재생 해줌.
            animator.SetTrigger(GatheringReadyTriggerName);
            isReadyToHeavyAttack = true;
        }

        // 홀딩 중에 키를 뗀 경우. 충분히 홀딩한 경우와 그렇지 못한 경우로 나뉨.
        if (context.action.phase == InputActionPhase.Canceled)
        {
            if (isReadyToHeavyAttack == true && heavyAttack != null)
            {
                // heavy attack 수행.
                isReadyToHeavyAttack = false;

                character.AddState(StateMask.ATTACKING); // 공격 상태 추가.
                currentAttack = heavyAttack;

                // 루트 모션을 적용할 애니메이션의 경우 애니메이션 시작과 끝에서 루트모션 함수 호출하는 방식으로 구현하기.
                animator.SetTrigger(currentAttack.GetCurrentAttackUnit().attackTrigger);
            }
            else
            {
                animator.SetTrigger(GatheringCancelTriggerName);
                // Exit Time을 1초로 설정 했지만, gathering 상태로 오는 과정에서 Exit Attack을 호출하지 못할 수 있으므로, ATTACKING 상태도 제거해줌.
                character.RemoveState(StateMask.ATTACKING);
                isGathering = false;
            }
            // gathering 상태 제거.
            character.RemoveState(StateMask.GATHERING);
        }
    }
}
