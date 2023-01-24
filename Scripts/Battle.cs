using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [SerializeField] protected string attackTargetTag;
    [SerializeField] protected LayerMask layersToIgnore;
    [SerializeField] private bool toggleDebug;
    [SerializeField] private float invincibleDuration;

    protected List<Attack> attackList;
    protected Attack currentAttack;

    protected Animator animator;
    protected Rigidbody rigidBody;
    protected SpriteRenderer spriteRenderer;
    protected CustomCharacter character;

    protected StateMask stopAttackMask;
    public bool targetIsInRange;
    public Vector3 lookAtVector;

    bool isSpriteFlashing; 

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        character = GetComponent<CustomCharacter>();
    }

    protected virtual void Start()
    {
        // ������ �� �� ���� ��츦 üũ�ϴ� �� ����� state ����ũ.
        // �÷��̾�, AI�� ���� �⺻ ����ũ�� ������ �߰���.
        stopAttackMask = StateMask.DEAD | StateMask.STUNNED | StateMask.DAMAGED | StateMask.DEFENDING;
    }

    protected virtual void Update()
    {
        lookAtVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        // ���� ������ ��� ��������Ʈ ������ ȿ�� ���.
        if (isSpriteFlashing == false && character.HasState(StateMask.INVINCIBLE) == true)
        {
            isSpriteFlashing = true;
            StartCoroutine(FlashSprite());
        }

        // �ڽ� Ŭ�������� �Է¿� ���� currentAttack �� ����. ���� ���� ����.
    }

    #region Helper Functions
    // ���� ���� �ȿ� ���� ����� �ִ��� üũ�ϴ� ����.
    public bool CheckTargetIsInRange()
    {
        Collider[] colliders = Physics.OverlapBox(
           transform.position + lookAtVector * currentAttack.attackRangeBox.offsetX, 
           currentAttack.attackRangeBox.scale / 2, 
           transform.rotation, ~layersToIgnore);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == attackTargetTag)
            {
                Battle target = colliders[i].GetComponent<Battle>();
                if (target != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public IEnumerator CanAttack(Attack attack)
    {
        yield return new WaitForSeconds(attack.cooldown);
        attack.canAttack = true;
    }

    IEnumerator ResetInvincible()
    {
        yield return new WaitForSeconds(invincibleDuration);
        character.RemoveState(StateMask.INVINCIBLE);
    }

    IEnumerator FlashSprite()
    {
        spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        yield return new WaitForSeconds(0.25f);
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.25f);
        isSpriteFlashing = false;
    }
    #endregion Helper Functions

    #region Attack Functions
    // [�ִϸ��̼� ȣ�� �Լ�] : ���� ó�� �Լ�. ���� ���� �ȿ� �ִ� ���鿡 �������� ����.
    public void DoAttack()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position + lookAtVector * currentAttack.attackRangeBox.offsetX,
            currentAttack.attackRangeBox.scale / 2,
            transform.rotation, ~layersToIgnore);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == attackTargetTag)
            {
                Battle target = colliders[i].GetComponent<Battle>();
                if (target != null)
                {
                    // ���� ��� ���� EnterDamaged �Լ��� ȣ���ؼ� ������ ����� �������� �޵��� ��.
                    target.EnterDamaged(currentAttack.GetCurrentAttackUnit(), transform.position);
                }
            }
        }
    }
    // [�ִϸ��̼� ȣ�� �Լ�] : �޺� ���� ���� �ȿ� ������ ��� �޺� ������ �ϳ� ���� ��Ŵ.
    public virtual void EnterCombo()
    {
        currentAttack.IncreaseComboStack();
    }
    // [�ִϸ��̼� ȣ�� �Լ�] : �޺� ���� ���� ������ ������ �޺� ������ �ʱ�ȭ.
    public virtual void ExitCombo()
    {
        currentAttack.ResetComboStack();
    }
    // [�ִϸ��̼� ȣ�� �Լ�] : ���� �ִϸ��̼��� ������ ���� ���� ������ �ʱ�ȭ.
    public virtual void ExitAttack()
    {
        character.RemoveState(StateMask.ATTACKING);
    }
    #endregion Attack Functions

    #region Damaged Functions
    // ������ �޴� ���� ó���ϴ� �Լ�. �������� ���ϴ� ��ü�� ���� ����� EnterDamaged �Լ��� ȣ���Կ� ����.
    protected virtual void EnterDamaged(Attack.Unit attackerAttackUnit, Vector3 attackerPosition)
    {
        // �������� �޴� ����� ���� �����̸� �׳� ����.
        if (character.HasState(StateMask.INVINCIBLE) == true) return;
        // �������� ������ �޾Ҵ��� ���� üũ��.
        if (character != null && character.TakeDamage(currentAttack.damage))
        {
            // ��Ʈ �ִϸ��̼� Ʈ����, ���� �� ���� ȿ���� ���� DAMAGED ���� �߰�.
            // ���� ���°� �ƴ� ���� ������ ��� STUNNED ���¸� �����.
            animator.SetTrigger(attackerAttackUnit.damagedTrigger);
            character.AddState(StateMask.DAMAGED);
            // �˹��� ����.
            Vector3 force = (transform.position - attackerPosition).normalized;
            rigidBody.AddForce(new Vector3(force.x, 1, 0) * attackerAttackUnit.knockback, ForceMode.Impulse);
            // ���� ȿ�� �ߵ�.
            if(attackerAttackUnit.giveInvincible == true)
            {
                character.AddState(StateMask.INVINCIBLE);
                StartCoroutine(ResetInvincible());
            }
        }
    }
    // [�ִϸ��̼� ȣ�� �Լ�] : �������� ���� �� ����Ǵ� �ִϸ��̼� �������� ȣ��Ǵ� �Լ�.
    public virtual void ExitDamaged()
    {
        // ���� �ִϸ��̼� ��� �߿� �°� �� ���, �ٽ� ���� ���¸� üũ�ϱ� ���� ���� ���� ���鵵 �ʱ�ȭ ����.
        ExitCombo();
        ExitAttack();
        character.RemoveState(StateMask.DAMAGED);
    }
    #endregion Damaged Functions

    #region Debug Functions
    void OnDrawGizmos()
    {
        if(toggleDebug && currentAttack != null)
        {
            //Gizmos.matrix = Matrix4x4.TRS(transform.position + lookAt * attackBoxOffsetX, transform.rotation, attackBoxScale);
            if (targetIsInRange) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + lookAtVector * currentAttack.attackRangeBox.offsetX, currentAttack.attackRangeBox.scale);
        }
    }
    #endregion Debug Functions
}
