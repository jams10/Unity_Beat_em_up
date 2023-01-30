using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowableItem : InteractableItem
{
    [SerializeField] private float grabHeight;
    [SerializeField] private float dropHeight;
    [SerializeField] private Vector2 throwScaleXY;

    protected Rigidbody rigidBody;
    bool isGrabbed;
    bool canAttack;

    protected override void Awake()
    {
        base.Awake();
        rigidBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        itemType = ItemType.THROWABLE;
        isGrabbed = false;
    }

    protected override void Update()
    {
        base.Update();
        if(canAttack)
        {
            
        }
    }

    public override void Interact(ref CustomCharacter player)
    {
        // 들어올리지 않았다면, 들어올리기.
        if (player != null && isGrabbed == false)
        {
            Grab(ref player);
            // GRABBING 상태는 물건을 던지거나 총을 발사하는 버튼이 공격 버튼과 같은 버튼이기 때문에,
            // 상태를 체크해서 상태에 따라 물건을 던지거나 총을 발사하거나 물리 공격을 할 수 있도록 하기 위해 추가함.
            player.AddState(StateMask.GRABBING);
        }
        // 이미 들어 올린 상태에서 다시 Interact를 호출했다면 제자리에 떨어트리기.
        else if(player != null && isGrabbed == true)
        {
            Drop();
            player.RemoveState(StateMask.GRABBING);
        }
    }

    protected void Grab(ref CustomCharacter player)
    {
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
        boxCollider.enabled = false;
        // 플레이어 트랜스폼을 부모 트랜스폼으로 설정하고, 플레이어 머리 위에 위치 시킴.
        gameObject.transform.SetParent(player.gameObject.transform, false);
        gameObject.transform.localPosition = Vector3.up * grabHeight;

        isGrabbed = true;
    }

    public void Drop()
    {
        DetachFromCharacter();
        rigidBody.AddForce(new Vector3(0, dropHeight, 0), ForceMode.Impulse);

        isGrabbed = false;
    }

    public void Throw(in Vector2 lookAtVector, in bool IsRunning)
    {
        if (isGrabbed)
        {
            Debug.Log(lookAtVector);
            DetachFromCharacter();
            // 아이템을 공격 가능한 상태로 만들어줌.
            canAttack = true;
            isGrabbed = false;
            // 달리고 있는 경우 더 빠른 속도로 던지게 함.
            Vector2 actualScale = throwScaleXY;
            actualScale.x = IsRunning ? throwScaleXY.x * 1.5f : throwScaleXY.x;
            rigidBody.AddForce(new Vector3(lookAtVector.x * actualScale.x, lookAtVector.y * actualScale.y, 0), ForceMode.Impulse);
        }
    }

    void DetachFromCharacter()
    {
        gameObject.transform.SetParent(null);
        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
        boxCollider.enabled = true;
    }


    //protected bool IsCollided()
    //{

    //}
}
