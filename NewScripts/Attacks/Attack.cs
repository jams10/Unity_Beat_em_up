using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� ���� Ŭ�������� ���� ���� �ν��Ͻ��� � �ν��Ͻ� ������ ��� ���� �������� ���ϴ� ������ �ϳ��� �Լ��� �����ϱ� ���� ������� Ŭ����.
// �߻� Ŭ������ ������� ���, PlayerAttack Ŭ�������� Attack �θ� Ŭ���� Ÿ������ �� �ڽ� Ŭ������ ������ ������ �� ����.
public class Attack : MonoBehaviour
{
    public virtual void DoAttack(in Vector3 characterPosition, in Vector3 lookAtVector, in LayerMask layersToDetect)
    { }

    public virtual void EnterAttack() { }
    public virtual void ExitAttack() { }

    public virtual void GetRangeBox(in Vector3 lookAtVector, in Vector3 characterPosition, out Vector3 center, out Vector3 size)
    { center = Vector3.zero; size = Vector3.zero; }
}
