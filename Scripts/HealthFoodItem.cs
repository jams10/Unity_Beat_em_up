using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthFoodItem : InteractableItem
{
    [SerializeField] private int healAmount;

    void Start()
    {
        itemType = ItemType.FOOD;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Interact(ref CustomCharacter player)
    {
        // 이미 체력이 꽉 차있는 경우에는 아이템을 먹지 않도록 함.
        if(player != null && player.IsHealthFull() == false)
        {
            player.IncreaseHealth(healAmount);
            isTimeToDestroy = true;
        }
    }
}
