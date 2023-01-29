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
}