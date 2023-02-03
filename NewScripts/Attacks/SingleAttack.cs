using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// "������ �ִ� Ÿ�ٵ��� ������� �ϴ� ���� ����"
// "���� �ִϸ��̼��� �ʿ� ���� ��쿡 ���. ex) ����ü ��."
public class SingleAttack : Attack
{
    [SerializeField] AttackSO attack;
    [SerializeField] LayerMask layersToIgnore;

    void Awake()
    {
        if(attack.targetType!= TargetType.AREA)
        {
            Debug.Log("������ ��� Target Type�� AREA�̾�� �մϴ�.");
        }
    }

    public override void DoAttack(in Vector3 characterPosition, in Vector3 lookAtVector)
    {
        // AttackSO�� ����� �ڽ� ������, ������ ���� ���̾� ����ũ�� ����Ͽ� �ش� �ڽ� ������ ���� �ݸ��� üũ�� ������.
        // OverlapBox�� �ڽ��� ���� ũ��(halfExtent)�� �޾��ִ� �Ϳ� ����.
        Vector3 center = characterPosition + attack.areaBoxOffsetX * lookAtVector;
        Collider[] colliders = Physics.OverlapBox(center, attack.areaBoxScale / 2, Quaternion.identity, ~layersToIgnore);

        foreach (Collider collider in colliders)
        {
            // ������ ���� ������Ʈ�� ������ �ִ� ��� ��������.
            // ������ ���� ������Ʈ�� ������ �ִ� ��� ��������.
            CharacterHit target = collider.GetComponent<CharacterHit>();
            if (target != null)
            {
                Vector3 knockBackDirection = (target.transform.position - characterPosition).normalized;
                target.TakeDamage(knockBackDirection,
                    attack.knockBackScale,
                    attack.damageAmount,
                    attack.hitCount);
            }
        }
    }

    public override void GetRangeBox(in Vector3 characterPosition, in Vector3 lookAtVector, out Vector3 center, out Vector3 size)
    {
        center = characterPosition + attack.areaBoxOffsetX * lookAtVector;
        size = attack.areaBoxScale;
    }
}
