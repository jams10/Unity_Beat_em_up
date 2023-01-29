using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThrowableItem : InteractableItem
{
    [SerializeField] private float grabHeight;
    [SerializeField] private float dropHeight;
    [SerializeField] private Vector2 throwScaleXY;

    protected Rigidbody rigidBody;
    Vector2 throwDirection;
    bool isGrabbed;
    bool canAttack;

    protected override void Awake()
    {
        base.Awake();
        rigidBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        itemType = (int)ItemType.THROWABLE;
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
        // ���ø��� �ʾҴٸ�, ���ø���.
        if (player != null && isGrabbed == false)
        {
            Grab(ref player);
            isGrabbed = true;
            // GRABBING ���´� ������ �����ų� ���� �߻��ϴ� ��ư�� ���� ��ư�� ���� ��ư�̱� ������,
            // ���¸� üũ�ؼ� ���¿� ���� ������ �����ų� ���� �߻��ϰų� ���� ������ �� �� �ֵ��� �ϱ� ���� �߰���.
            player.AddState(StateMask.GRABBING);
        }
        // �̹� ��� �ø� ���¿��� �ٽ� Interact�� ȣ���ߴٸ� ���ڸ��� ����Ʈ����.
        else if(player != null && isGrabbed == true)
        {
            Drop();
            isGrabbed = false;
            player.RemoveState(StateMask.GRABBING);
        }
    }

    protected void Grab(ref CustomCharacter player)
    {
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
        boxCollider.enabled = false;
        // �÷��̾� Ʈ�������� �θ� Ʈ���������� �����ϰ�, �÷��̾� �Ӹ� ���� ��ġ ��Ŵ.
        gameObject.transform.SetParent(player.gameObject.transform, false);
        gameObject.transform.localPosition = Vector3.up * grabHeight;
    }

    protected void Drop()
    {
        gameObject.transform.SetParent(null);
        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
        boxCollider.enabled = true;
        rigidBody.AddForce(new Vector3(0, dropHeight, 0), ForceMode.Impulse);
    }

    protected void Throw(in Vector2 lookAtVector)
    {
        if (isGrabbed)
        {
            throwDirection = lookAtVector;
            // �������� ���� ������ ���·� �������.
            canAttack = true;
            rigidBody.AddForce(new Vector3(throwDirection.x * throwScaleXY.x, throwDirection.y * throwScaleXY.y, 0), ForceMode.Impulse);
        }
    }

    //protected bool IsCollided()
    //{

    //}
}
