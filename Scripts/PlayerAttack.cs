using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private int maxLightAttacks;
    [SerializeField] private int ligthAttackDamage;
    [SerializeField] private float knockBackScale;
    [SerializeField] private float attackBoxOffsetX;
    [SerializeField] private Vector3 attackBoxScale;
    [SerializeField] private string attackTargetTag;
    [SerializeField] private LayerMask layersToIgnore;

    Animator animator;
    Rigidbody rigidBody;
    GameCharacter playerCharacter;
    int lightAttackComboStack;
    bool isAttacking;
    Vector3 lookAtVector;

    bool targetIsInRange;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        playerCharacter = GetComponent<GameCharacter>();  
    }

    void Start()
    {
        lightAttackComboStack = 0;
        isAttacking = false;
        lookAtVector = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // �÷��̾ �ٶ󺸴� ����. transform�� rotation ���� quaternion ���̹Ƿ�, ���Ϸ� ������ ������ �־���.
        lookAtVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        if (playerCharacter.HasCharacterState(GameCharacter.CharacterStateMask.isDamaged))
        {
            rigidBody.AddForce(-lookAtVector * knockBackScale, ForceMode.Impulse);
            EnterDamaged();
        }

        ProcessAttackInput();
    }

    #region Attack Functions
    void ProcessAttackInput()
    {
        if (Input.GetButtonDown("Fire1") 
            && isAttacking == false
            && playerCharacter.HasCharacterState(GameCharacter.CharacterStateMask.isDamaged) == false)
        {
            {
                targetIsInRange = false;
                isAttacking = true;
                playerCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isAttacking);
                animator.SetTrigger("Punch_" + lightAttackComboStack);
            }
        }
    }


    public void ProcessAttack()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + lookAtVector * attackBoxOffsetX, attackBoxScale / 2, transform.rotation, ~layersToIgnore);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == attackTargetTag)
            {
                GameCharacter targetcCharacter = colliders[i].GetComponent<GameCharacter>();
                if (targetcCharacter != null)
                {
                    targetIsInRange = true;
                    // �޺� ���� �־ �� �������� �и��� ȿ�� �־ ������ �� ����.
                    if (targetcCharacter.TakeDamage(ligthAttackDamage))
                    {
                        targetcCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isDamaged);
                    }
                }
            }
        }
    }

    public void EnterCombo()
    {
        isAttacking = false;
        if (lightAttackComboStack < maxLightAttacks)
        {
            lightAttackComboStack++;
        }
    }

    public void ExitCombo()
    {
        isAttacking = false;
        lightAttackComboStack = 0;
    }

    public void ExitAttack()
    {
        isAttacking = false;
        lightAttackComboStack = 0;
        playerCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isAttacking);
    }
    #endregion Attack Functions

    void EnterDamaged()
    {
        animator.SetTrigger("isDamaged");
        playerCharacter.AddCharacterState(GameCharacter.CharacterStateMask.isStun);
        playerCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isDamaged);
    }

    public void ExitDamaged()
    {
        // ĳ���Ͱ� ���� �߿� �°� �Ǹ�, ó������ ���ư� �ٽ� ���� ���¸� üũ�ϱ� ���� ���� ���� ������ �ʱ�ȭ ����.
        isAttacking = false;
        playerCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isAttacking);
        playerCharacter.RemoveCharacterState(GameCharacter.CharacterStateMask.isStun);
    }

    void OnDrawGizmos()
    {
        //Gizmos.matrix = Matrix4x4.TRS(transform.position + lookAt * attackBoxOffsetX, transform.rotation, attackBoxScale);
        if (targetIsInRange) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + lookAtVector * attackBoxOffsetX, attackBoxScale);
    }
}
