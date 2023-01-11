using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.UIElements;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack instance;
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
        // transform의 rotation 값은 quaternion 값이므로, 오일러 각으로 변경해 넣어줌.
        lookAt = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;
;
        ProcessAttack();
    }

    #region Combo
    public void EnterCombo()
    {
        isAttacking = false;
        if (!enemyIsInRange) return;
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
    void ProcessAttack()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + lookAt * attackBoxOffsetX, attackBoxScale / 2, rigidBody.transform.rotation, ~layerMasks);
        if (colliders.Length > 0)
        {
            enemyIsInRange = true;
            for (int i = 0; i < colliders.Length; i++) { Debug.Log(colliders[i].name); }
        }
        else enemyIsInRange = false;

        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Punch_" + comboStack);
        }
    }
    #endregion Combo

    void OnDrawGizmos()
    {
        //Gizmos.matrix = Matrix4x4.TRS(transform.position + lookAt * attackBoxOffsetX, transform.rotation, attackBoxScale);
        if(enemyIsInRange) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + lookAt * attackBoxOffsetX, attackBoxScale);
    }
}
