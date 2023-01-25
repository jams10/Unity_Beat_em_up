using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ItemType
{
    NONE = 0,
    FOOD = 1,
    THROWABLE = 2,
    GUN = 4,
    GRENADE = 8,
    END = 16,
}

public class InteractableItem : MonoBehaviour
{

    bool isTimeToDestroy;
    protected int itemType;
    protected Rigidbody rigidBody;
    protected BoxCollider boxCollider;

    protected virtual void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();  
    }

    void Start()
    {
        itemType = (int)ItemType.NONE;
    }

    protected virtual void Update()
    {
        if(isTimeToDestroy == true)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Interact(GameObject player)
    {
        Debug.Log("Interactable Item!");
    }
}
