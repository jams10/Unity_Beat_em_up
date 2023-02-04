using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputProcessor : MonoBehaviour
{
    PlayerInput playerInput;
    bool playerInputHasInit;

    PlayerAttack playerAttack;
    CastDebuff castDebuff;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerAttack = GetComponent<PlayerAttack>();
        castDebuff = GetComponent<CastDebuff>();
    }

    void Update()
    {
        if (playerInputHasInit == false)
            InitPlayerInput();
    }

    // 입력 이벤트를 추가해주는 함수.
    void InitPlayerInput()
    {
        if (playerInput == null) return;
        playerInput.actions["ComboAttack"].started += Input_ComboAttack;
        playerInput.actions["ComboAttack"].performed += Input_ComboAttack;
        playerInput.actions["ComboAttack"].canceled += Input_ComboAttack;

        playerInput.actions["CastDebuff"].performed += Input_CastDebuff;

        playerInputHasInit = true;
    }

    void DisablePlayerInput()
    {
        if (playerInput == null) return;
        playerInput.actions["ComboAttack"].started -= Input_ComboAttack;
        playerInput.actions["ComboAttack"].performed -= Input_ComboAttack;
        playerInput.actions["ComboAttack"].canceled -= Input_ComboAttack;

        playerInput.actions["CastDebuff"].performed -= Input_CastDebuff;

        playerInputHasInit = true;
    }

    void Input_ComboAttack(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            playerAttack.ComboAttack();
        }
    }

    void Input_CastDebuff(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            castDebuff.DoAnim();
        }
    }
}
