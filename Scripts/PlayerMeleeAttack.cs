using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.UIElements;

public class PlayerMeleeAttack : MonoBehaviour
{
    public static PlayerMeleeAttack instance;
    public bool isAttacking;
    public int maxLightAttack;
    public float attackBoxOffsetX;
    public Vector3 attackBoxScale;
    [SerializeField] private LayerMask layerMasks;

    Animator animator;
    Rigidbody rigidBody;
    Vector3 lookAt;
    int comboStack;
    bool targetIsInRange;

    void Awake()
    {
        instance = this;
        animator= GetComponent<Animator>();
        rigidBody= GetComponent<Rigidbody>();
        comboStack = 0;
        targetIsInRange = false;
    }

    void Update()
    {
        // transform의 rotation 값은 quaternion 값이므로, 오일러 각으로 변경해 넣어줌.
        lookAt = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;
;
        ProcessAttack();
    }

    #region Combo
    public void EnterCombo()
    {
        isAttacking = false;
        if(comboStack < maxLightAttack)
        {
            comboStack++;   
        }
    }
    public void ExitCombo()
    {
        isAttacking = false;
        comboStack = 0;
    }
    #endregion Combo
    void ProcessAttack()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Punch_" + comboStack);
        }
    }

    public void AttackEnemy()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + lookAt * attackBoxOffsetX, attackBoxScale / 2, rigidBody.transform.rotation, ~layerMasks);

        targetIsInRange = false;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Enemy")
            {
                targetIsInRange = true;
                CustomCharacter character = colliders[i].GetComponent<CustomCharacter>();
                if (character != null)
                {
                    // 콤보 스택 넣어서 맨 마지막에 밀리는 효과 넣어도 괜찮을 것 같음.
                    character.TakeDamage(10);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        //Gizmos.matrix = Matrix4x4.TRS(transform.position + lookAt * attackBoxOffsetX, transform.rotation, attackBoxScale);
        if(targetIsInRange) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + lookAt * attackBoxOffsetX, attackBoxScale);
    }
}
