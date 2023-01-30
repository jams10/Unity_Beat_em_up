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

        // ���� �ȿ� �����۵��� �ִ��� Ȯ����.
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

        // ������ �ִ� ������ �� ĳ���Ϳ� ���� ����� �������� ��ȣ�ۿ��� ���������� ������.
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
            // ���� ��ȣ�ۿ� �ϰ� �ִ� �������� ���� ��� ã����.
            if(interactingItem == null)
            {
                if (CheckItemIsInRange(out interactingItem))
                {
                    interactingItem.Interact(ref player);
                }
            }
            else
            {
                // ü�� �������� ��� ��ȣ �ۿ�� ���ÿ� �����.
                // Throwable �������� ��� �ٽ� �� �� ��ȣ �ۿ� ��ư�� ������ ��� �ٴڿ� ����Ʈ���� ��.
                // Gun �������� ��쿡�� �ٽ� �� �� ��ȣ �ۿ� ��ư�� ������ �ٴڿ� ����Ʈ���� ��.
                // ����, ���⼭�� Interact�� �� �� �� ȣ�����ְ� ��ȣ �ۿ� ���� �������� �����.
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
                    // Throwable ������ : ���� = ������.
                    case ItemType.THROWABLE:
                        ThrowableItem item = interactingItem as ThrowableItem;
                        item.Throw(player.GetLookAtVector(), player.HasState(StateMask.RUNNING));
                        interactingItem = null;
                        break;
                    // Gun ������ : ���� = �� �߻�.
                }
            }
        }
    }
}
