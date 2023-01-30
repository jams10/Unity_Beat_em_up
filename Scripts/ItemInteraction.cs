using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemInteraction : MonoBehaviour
{
    [SerializeField] private string ItemTag;
    [SerializeField] protected LayerMask layersToIgnore;

    CustomCharacter player;
    BoxCollider boxCollider;
    InteractableItem interactingItem;

    void Awake()
    {
        player = GetComponent<CustomCharacter>(); 
        boxCollider = GetComponent<BoxCollider>();
    }

    void Start()
    {
        interactingItem = null;
    }

    bool CheckItemIsInRange(out InteractableItem nearestItem)
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position,
            boxCollider.bounds.extents,
            transform.rotation, ~layersToIgnore);

        List<InteractableItem> itemList = new List<InteractableItem>(); 

        // 범위 안에 아이템들이 있는지 확인함.
        bool itemIsInRange = false;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == ItemTag)
            {
                InteractableItem item = colliders[i].GetComponent<InteractableItem>();
                if(item!= null)
                {
                    itemList.Add(item);
                    itemIsInRange = true;
                }
            }
        }

        // 범위에 있는 아이템 중 캐릭터와 가장 가까운 아이템을 상호작용할 아이템으로 선정함.
        if(itemIsInRange)
        {
            float minDist = 1000.0f;
            int minIdx = 0;
            for (int i = 0; i < itemList.Count; ++i)
            {
                float dist = Vector3.SqrMagnitude(transform.position - itemList[i].transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIdx = i;
                }
            }
            nearestItem = itemList[minIdx];
        }
        else
        {
            nearestItem = null;
        }

        return itemIsInRange;
    }

    public void Drop()
    {
        if (interactingItem != null && player.HasState(StateMask.STUNNED) == true)
        {
            ThrowableItem item = interactingItem as ThrowableItem;
            if (item != null)
            {
                item.Drop();
                interactingItem = null;
            }
        }
    }

    public void Input_ItemInteraction(InputAction.CallbackContext context)
    {
        if(context.action.phase == InputActionPhase.Performed)
        {
            // 현재 상호작용 하고 있는 아이템이 없는 경우 찾아줌.
            if(interactingItem == null)
            {
                if (CheckItemIsInRange(out interactingItem))
                {
                    interactingItem.Interact(ref player);
                }
            }
            else
            {
                // 체력 아이템의 경우 상호 작용과 동시에 사라짐.
                // Throwable 아이템의 경우 다시 한 번 상호 작용 버튼을 누르는 경우 바닥에 떨어트리게 됨.
                // Gun 아이템의 경우에도 다시 한 번 상호 작용 버튼을 누르면 바닥에 떨어트리게 됨.
                // 따라서, 여기서는 Interact를 한 번 더 호출해주고 상호 작용 중인 아이템을 비워줌.
                interactingItem.Interact(ref player);
                interactingItem = null;
            }
        }
    }

    public void Input_ItemAttack(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            if(interactingItem != null && player.HasState(StateMask.GRABBING) == true)
            {
                ItemType itemType = interactingItem.GetItemType();
                switch (itemType)
                {
                    // Throwable 아이템 : 공격 = 던지기.
                    case ItemType.THROWABLE:
                        ThrowableItem item = interactingItem as ThrowableItem;
                        item.Throw(player.GetLookAtVector(), player.HasState(StateMask.RUNNING));
                        interactingItem = null;
                        break;
                    // Gun 아이템 : 공격 = 총 발사.
                }
            }
        }
    }
}
