using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// "영역에 있는 타겟들을 대상으로 하는 단일 공격"
// "공격 애니메이션이 필요 없는 경우에 사용. ex) 투사체 등."
public class SingleAttack : Attack
{
    [SerializeField] AttackSO attack;

    void Awake()
    {
        if(attack.targetType!= TargetType.AREA)
        {
            Debug.Log("공격의 경우 Target Type이 AREA이어야 합니다.");
        }
    }

    public override void DoAttack(in Vector3 characterPosition, in Vector3 lookAtVector, in LayerMask layersToDetect)
    {
        // AttackSO에 저장된 박스 오프셋, 스케일 값과 레이어 마스크를 사용하여 해당 박스 영역에 대해 콜리젼 체크를 수행함.
        // OverlapBox는 박스의 절반 크기(halfExtent)를 받아주는 것에 유의.
        Vector3 center = characterPosition + attack.areaBoxOffsetX * lookAtVector;
        Collider[] colliders = Physics.OverlapBox(center, attack.areaBoxScale / 2, Quaternion.identity, layersToDetect);

        foreach (Collider collider in colliders)
        {
            // 데미지 관련 컴포넌트를 가지고 있는 경우 수행해줌.
            // 데미지 관련 컴포넌트를 가지고 있는 경우 수행해줌.
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
