using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowableItem : InteractableItem
{
    [SerializeField] private float grabHeight;
    [SerializeField] private float dropHeight;
    [SerializeField] private Vector2 throwScaleXY;

    protected Rigidbody rigidBody;
    bool isGrabbed;
    bool canAttack;

    protected override void Awake()
    {
        base.Awake();
        rigidBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        itemType = ItemType.THROWABLE;
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
            // GRABBING ���´� ������ �����ų� ���� �߻��ϴ� ��ư�� ���� ��ư�� ���� ��ư�̱� ������,
            // ���¸� üũ�ؼ� ���¿� ���� ������ �����ų� ���� �߻��ϰų� ���� ������ �� �� �ֵ��� �ϱ� ���� �߰���.
            player.AddState(StateMask.GRABBING);
        }
        // �̹� ��� �ø� ���¿��� �ٽ� Interact�� ȣ���ߴٸ� ���ڸ��� ����Ʈ����.
        else if(player != null && isGrabbed == true)
        {
            Drop();
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

        isGrabbed = true;
    }

    public void Drop()
    {
        DetachFromCharacter();
        rigidBody.AddForce(new Vector3(0, dropHeight, 0), ForceMode.Impulse);

        isGrabbed = false;
    }

    public void Throw(in Vector2 lookAtVector, in bool IsRunning)
    {
        if (isGrabbed)
        {
            Debug.Log(lookAtVector);
            DetachFromCharacter();
            // �������� ���� ������ ���·� �������.
            canAttack = true;
            isGrabbed = false;
            // �޸��� �ִ� ��� �� ���� �ӵ��� ������ ��.
            Vector2 actualScale = throwScaleXY;
            actualScale.x = IsRunning ? throwScaleXY.x * 1.5f : throwScaleXY.x;
            rigidBody.AddForce(new Vector3(lookAtVector.x * actualScale.x, lookAtVector.y * actualScale.y, 0), ForceMode.Impulse);
        }
    }

    void DetachFromCharacter()
    {
        gameObject.transform.SetParent(null);
        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
        boxCollider.enabled = true;
    }


    //protected bool IsCollided()
    //{

    //}
}
