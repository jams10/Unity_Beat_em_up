using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 *  플레이어의 입력에 따른 공격을 처리하는 클래스.
 *  여러 가지 공격 타입 프리팹을 받아서 이를 애니메이션 이벤트 함수를 통해 공격 애니메이션과 공격 로직을 연결 시켜주는 것을 담당함.
 */
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] ComboAttack comboAttackPrefab;
    [SerializeField] LayerMask layersToAttack;

    Animator animator;

    PlayerInput playerInput;
    bool playerInputHasInit;

    Attack currentAttack;
    ComboAttack comboAttack;

    CharacterState playerState;

    void Awake()
    {
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        playerState = GetComponent<CharacterState>();
        comboAttack = Instantiate(comboAttackPrefab);

        currentAttack = comboAttack;

        InitAttacks();
    }

    void Update()
    {
        //if (playerInputHasInit == false)
        //    InitPlayerInput();

    }

    private void InitAttacks()
    {
        comboAttack.SetAnimator(animator);
    }

    #region Animation Event Functions
    /* Attack */
    public void Anim_EnterAttack()
    {
        playerState.AddState(StateMask.ATTACKING);
        currentAttack.EnterAttack();
    }

    public void Anim_ExitAttack()
    {
        playerState.RemoveState(StateMask.ATTACKING);
        currentAttack.ExitAttack();
    }

    public void Anim_DoAttack()
    {
        Debug.Log("attack");
        currentAttack.DoAttack(transform.position, Quaternion.Euler(0, gameObject.transform.eulerAngles.y, 0) * Vector3.right, layersToAttack);
    }
    /* Combo */
    // 마지막으로 사용할 콤보 애니메이션의 경우에는 EnterCombo를 넣어주면 안됨.
    public void Anim_EnterCombo()
    {
        Debug.Log("enter combo");
        comboAttack.EnterCombo();
    }

    public void Anim_ExitCombo()
    {
        Debug.Log("exit combo");
        comboAttack.ExitCombo();
    }
    #endregion Animation Event Functions

    void OnDrawGizmos()
    {
        if (currentAttack == null) return;

        Vector3 center = Vector3.zero;
        Vector3 size = Vector3.zero;
        currentAttack.GetRangeBox(transform.position, Quaternion.Euler(0, gameObject.transform.eulerAngles.y, 0) * Vector3.right, out center, out size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }

    #region Input
    // 입력 이벤트를 추가해주는 함수.
    //void InitPlayerInput()
    //{
    //    if (playerInput == null) return;
    //    playerInput.actions["ComboAttack"].started += Input_ComboAttack;
    //    playerInput.actions["ComboAttack"].performed += Input_ComboAttack;
    //    playerInput.actions["ComboAttack"].canceled += Input_ComboAttack;

    //    playerInputHasInit = true;
    //}

    //void DisablePlayerInput()
    //{
    //    if (playerInput == null) return;
    //    playerInput.actions["ComboAttack"].started -= Input_ComboAttack;
    //    playerInput.actions["ComboAttack"].performed -= Input_ComboAttack;
    //    playerInput.actions["ComboAttack"].canceled -= Input_ComboAttack;

    //    playerInputHasInit = true;
    //}

    //void Input_ComboAttack(InputAction.CallbackContext context)
    //{
    //    if (context.action.phase == InputActionPhase.Performed)
    //    {
    //        currentAttack = comboAttack;
    //        comboAttack.DoAnim();
    //    }
    //}
    #endregion Input

    public void ComboAttack()
    {
        currentAttack = comboAttack;
        comboAttack.DoAnim();
    }

}
