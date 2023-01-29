using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if (interactingItem != null)
            {
                interactingItem.Interact(ref player);
                interactingItem = null;
            }
            else if(CheckItemIsInRange(out interactingItem))
            {
                interactingItem.Interact(ref player);
            }
        }
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
}
