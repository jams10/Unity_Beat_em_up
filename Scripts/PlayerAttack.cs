using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack instance;
    public Animator animator;
    public bool isAttacking;

    BoxCollider boxCollider;
    Rigidbody rigidBody;

    void Awake()
    {
        instance = this;
        boxCollider= GetComponent<BoxCollider>();
        animator= GetComponent<Animator>();
        rigidBody= GetComponent<Rigidbody>();   
    }

    void Update()
    {
        ProcessAttack();
    }

    void ProcessAttack()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            isAttacking = true;
        }
    }
}
