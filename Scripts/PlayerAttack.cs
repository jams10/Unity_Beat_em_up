using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    BoxCollider boxCollider;
    bool isAttacking;

    void Awake()
    {
        boxCollider= GetComponent<BoxCollider>();
    }

    void Update()
    {
        if(Input.GetButton(""))
        {

        }
    }
}
