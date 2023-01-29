using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
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

    protected bool isTimeToDestroy;
    protected int itemType;
    protected BoxCollider boxCollider;

    protected virtual void Awake()
    {
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

    public virtual void Interact(ref CustomCharacter player)
    {
        
    }

    public bool IsTypeOf(ItemType type)
    {
        return (itemType & (int)type) > 0;
    }
}
