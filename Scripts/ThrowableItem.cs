using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThrowableItem : InteractableItem
{
    [SerializeField] private float height;

    bool isGrabbed;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        itemType = (int)ItemType.THROWABLE;
        isGrabbed = false;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Interact(GameObject player)
    {
        if(isGrabbed == true)
        {
            gameObject.transform.SetParent(null);
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
            boxCollider.enabled = true;
            rigidBody.AddForce(new Vector3(3, 0, 1), ForceMode.Impulse);
            isGrabbed = false;
        }
        else
        {
            Debug.Log("Throwable Item!");
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            boxCollider.enabled = false;
            // 플레이어 트랜스폼을 부모 트랜스폼으로 설정하고, 플레이어 머리 위에 위치 시킴.
            gameObject.transform.SetParent(player.transform, false);
            gameObject.transform.localPosition = Vector3.zero + Vector3.up * height;
            isGrabbed = true;
        }
    }
}
