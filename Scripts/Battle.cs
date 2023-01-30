using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;

public class Battle : MonoBehaviour
{
    [SerializeField] protected string attackTargetTag;
    [SerializeField] protected LayerMask layersToIgnore;
    [SerializeField] private bool toggleDebug;
    [SerializeField] private float invincibleDuration;
    [SerializeField] private float hitDuration;
    [SerializeField] private int maxHitCount;
    [SerializeField] private string hitAnimTriggerPrefix;
    [SerializeField] private Vector2 hitKnockbackScale;

    protected List<Attack> attackList;
    protected Attack currentAttack;

    protected Animator animator;
    protected Rigidbody rigidBody;
    protected SpriteRenderer spriteRenderer;
    protected CustomCharacter character;
    protected ItemInteraction itemInteraction;

    protected StateMask stopAttackMask;
    protected bool isRootMotionPlaying;
    public bool targetIsInRange;

    bool isSpriteFlashing;
    bool hitResetCoroutineRunning;
    int currentHitCount;

    Coroutine hitCountResetCoroutine;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        character = GetComponent<CustomCharacter>();
        itemInteraction = GetComponent<ItemInteraction>();   
    }

    protected virtual void Start()
    {
        // ������ �� �� ���� ��츦 üũ�ϴ� �� ����� state ����ũ.
        // �÷��̾�, AI�� ���� �⺻ ����ũ�� ������ �߰���.
        stopAttackMask = StateMask.DEAD | StateMask.STUNNED | StateMask.DEFENDING | StateMask.GRABBING;
    }

    protected virtual void Update()
    {

        // ���� ������ ��� ��������Ʈ ������ ȿ�� ���.
        if (isSpriteFlashing == false && character.HasState(StateMask.ATTACKING) == false && character.HasState(StateMask.INVINCIBLE) == true)
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
           transform.position + character.GetLookAtVector() * currentAttack.attackRangeBox.offsetX, 
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

    IEnumerator ResetHitCount()
    {
        hitResetCoroutineRunning = true;
        yield return new WaitForSeconds(hitDuration);
        character.RemoveState(StateMask.DAMAGED);
        currentHitCount = 0; // �ǰ� ���� �ʱ�ȭ.
        hitResetCoroutineRunning = false;
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
            transform.position + character.GetLookAtVector() * currentAttack.attackRangeBox.offsetX,
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
                    target.EnterDamaged(currentAttack.GetCurrentAttackUnit(), currentAttack.knockBackScale, transform.position);
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
    // [�ִϸ��̼� ȣ�� �Լ�] : ��Ʈ ��� ����.
    public virtual void EnterRootMotion()
    {
        isRootMotionPlaying = true;
        character.AddState(StateMask.INVINCIBLE);
    }
    // [�ִϸ��̼� ȣ�� �Լ�] : ��Ʈ ��� ��.
    public virtual void ExitRootMotion()
    {
        isRootMotionPlaying = false;
        character.RemoveState(StateMask.INVINCIBLE);
    }
    #endregion Attack Functions

    #region Damaged Functions
    // ������ �޴� ���� ó���ϴ� �Լ�. �������� ���ϴ� ��ü�� ���� ����� EnterDamaged �Լ��� ȣ���Կ� ����.
    protected virtual void EnterDamaged(in Attack.Unit attackerAttackUnit, in Vector2 knockBackScale, in Vector3 attackerPosition)
    {
        // �������� �޴� ����� ���� �����̸� �׳� ����.
        if (character.HasState(StateMask.INVINCIBLE) == true)
            return;
        // �������� ������ �޾Ҵ��� ���� üũ��.
        if (character != null && character.TakeDamage(currentAttack.damage))
        {
            // �ǰ� ������ ����������. �ڷ�ƾ�� ���� �ǰ� ī��Ʈ�� �ʱ�ȭ �Ǳ� �� �����ؼ� �ǰ��� ���� ��� ������ ���̰� ��.
            currentHitCount += attackerAttackUnit.hitStackCount;

            // ��Ʈ �ִϸ��̼� Ʈ����, ���� �� ���� ȿ���� ���� STUN ���� �߰�.
            // DAMAGED�� ��� �ǰ� ������ ���� ���. ������ �ϴ� �߿� ���� ��쿡�� ���� ���¸� ��������.
            character.AddState(StateMask.STUNNED);
            character.AddState(StateMask.DAMAGED);
            character.RemoveState(StateMask.ATTACKING);

            // �ִ� �ǰ� ���ÿ� �����ϴ� ���.
            if (currentHitCount >= maxHitCount)
            {
                currentHitCount = maxHitCount;

                // �˹��� ����.
                Vector3 force = (transform.position - attackerPosition).normalized;
                rigidBody.AddForce(new Vector3(force.x, 1, 0) * knockBackScale, ForceMode.Impulse);

                // ���� ȿ�� �ߵ�.
                character.AddState(StateMask.INVINCIBLE);
                StartCoroutine(ResetInvincible());
            }

            // �ǰ� ī��Ʈ ���ÿ� �´� �ִϸ��̼� Ʈ���� ���.
            animator.SetTrigger(hitAnimTriggerPrefix + currentHitCount);
        }
    }
    // [�ִϸ��̼� ȣ�� �Լ�] : �������� ���� �� ����Ǵ� �ִϸ��̼� �������� ȣ��Ǵ� �Լ�.
    public virtual void ExitDamaged()
    {
        // ���� �ִϸ��̼� ��� �߿� �°� �� ���, �ٽ� ���� ���¸� üũ�ϱ� ���� ���� ���� ���鵵 �ʱ�ȭ ����.
        ExitCombo();
        //ExitAttack();
        character.RemoveState(StateMask.STUNNED);
        
        // �ǰ� �ִϸ��̼��� ���� �ڿ� ª�� �ð� �Ŀ� DAMAGED ���¸� ��������.(ª�� �ð� �ȿ� �� �� �� ���� ��� �ǰ� ������ ���� ��Ŵ.)
        // �ڷ�ƾ ���� �� �� �� �� ���� ���, ���� �ڷ�ƾ�� ������Ű�� ���ο� �ǰ� �ð��� �������� �ϴ� �ڷ�ƾ ����.
        if(hitResetCoroutineRunning == false)
            hitCountResetCoroutine = StartCoroutine(ResetHitCount());
        else
        {
            StopCoroutine(hitCountResetCoroutine);
            hitCountResetCoroutine = StartCoroutine(ResetHitCount());
        }
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
            Gizmos.DrawWireCube(transform.position + character.GetLookAtVector() * currentAttack.attackRangeBox.offsetX, currentAttack.attackRangeBox.scale);
        }
    }
    #endregion Debug Functions
}
