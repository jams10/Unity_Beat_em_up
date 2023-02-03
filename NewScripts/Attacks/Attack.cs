using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 전투 관련 클래스에서 현재 공격 인스턴스가 어떤 인스턴스 인지에 상관 없이 데미지를 가하는 로직을 하나의 함수로 수행하기 위해 만들어준 클래스.
// 추상 클래스로 만들어줄 경우, PlayerAttack 클래스에서 Attack 부모 클래스 타입으로 각 자식 클래스를 참조해 관리할 수 없음.
public class Attack : MonoBehaviour
{
    public virtual void DoAttack(in Vector3 characterPosition, in Vector3 lookAtVector)
    { }

    public virtual void GetRangeBox(in Vector3 lookAtVector, in Vector3 characterPosition, out Vector3 center, out Vector3 size)
    { center = Vector3.zero; size = Vector3.zero; }
}
