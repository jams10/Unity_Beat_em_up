using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 *  �÷��̾��� �Է¿� ���� ������ ó���ϴ� Ŭ����.
 *  ���� ���� ���� Ÿ�� �������� �޾Ƽ� �̸� �ִϸ��̼� �̺�Ʈ �Լ��� ���� ���� �ִϸ��̼ǰ� ���� ������ ���� �����ִ� ���� �����.
 */
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] ComboAttack comboAttackPrefab;

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
        comboAttack = Instantiate(comboAttackPrefab);

        currentAttack = comboAttack;

        InitAttacks();
    }

    void Update()
    {
        if (playerInputHasInit == false)
            InitPlayerInput();

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
    }

    public void Anim_ExitAttack()
    {
        playerState.RemoveState(StateMask.ATTACKING);
    }

    public void Anim_DoAttack()
    {
        currentAttack.DoAttack(transform.position, Quaternion.Euler(0, gameObject.transform.eulerAngles.y, 0) * Vector3.right);
    }
    /* Combo */
    public void Anim_EnterCombo()
    {
        comboAttack.EnterCombo();
    }

    public void Anim_ExitCombo()
    {
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
    // �Է� �̺�Ʈ�� �߰����ִ� �Լ�.
    void InitPlayerInput()
    {
        if (playerInput == null) return;
        playerInput.actions["ComboAttack"].started += Input_ComboAttack;
        playerInput.actions["ComboAttack"].performed += Input_ComboAttack;
        playerInput.actions["ComboAttack"].canceled += Input_ComboAttack;

        playerInputHasInit = true;
    }

    void DisablePlayerInput()
    {
        if (playerInput == null) return;
        playerInput.actions["ComboAttack"].started -= Input_ComboAttack;
        playerInput.actions["ComboAttack"].performed -= Input_ComboAttack;
        playerInput.actions["ComboAttack"].canceled -= Input_ComboAttack;

        playerInputHasInit = true;
    }

    void Input_ComboAttack(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            currentAttack = comboAttack;
            comboAttack.DoAnim();
        }
    }
    #endregion Input
}
