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
    bool enemyIsInRange;

    void Awake()
    {
        instance = this;
        animator= GetComponent<Animator>();
        rigidBody= GetComponent<Rigidbody>();
        comboStack = 0;
        enemyIsInRange = false;
    }

    void Update()
    {
        // transform�� rotation ���� quaternion ���̹Ƿ�, ���Ϸ� ������ ������ �־���.
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

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Enemy")
            {
                CustomCharacter character = colliders[i].GetComponent<CustomCharacter>();
                if (character != null)
                {
                    // �޺� ���� �־ �� �������� �и��� ȿ�� �־ ������ �� ����.
                    character.TakeDamage(10);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        //Gizmos.matrix = Matrix4x4.TRS(transform.position + lookAt * attackBoxOffsetX, transform.rotation, attackBoxScale);
        if(enemyIsInRange) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + lookAt * attackBoxOffsetX, attackBoxScale);
    }
}
