using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthFoodItem : InteractableItem
{
    [SerializeField] private int healAmount;

    void Start()
    {
        itemType = (int)ItemType.FOOD;
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
        // �̹� ü���� �� ���ִ� ��쿡�� �������� ���� �ʵ��� ��.
        if(player != null && player.IsHealthFull() == false)
        {
            player.IncreaseHealth(healAmount);
            isTimeToDestroy = true;
        }
    }
}
