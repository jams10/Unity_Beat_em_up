using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [System.Serializable]
    public struct RangeBox // 공격 범위 박스 정보를 담은 구조체.
    {
        public float offsetX;
        public Vector3 scale;
    }
    [System.Serializable]
    public struct Unit // 각 개별 공격에 대한 정보를 담은 구조체.
    {
        public string attackTrigger;  // 공격 시 발생 시킬 애니메이션 트리거 이름.
        public int hitStackCount;        // 데미지를 받을 때 증가 시킬 스택 카운트.
    }
    [SerializeField] int _damage; public int damage { get { return _damage; } }
    [SerializeField] uint _currentComboStack; public uint currentComboStack { get { return _currentComboStack; } }
    [SerializeField] Vector2 _knockBackScale; public Vector2 knockBackScale { get { return _knockBackScale; } }
    [SerializeField] uint _maxComboStack; public uint maxComboStack { get { return _maxComboStack; } }
    [SerializeField] float _cooldown; public float cooldown { get { return _cooldown; } }
    [SerializeField] bool _canAttack; public bool canAttack { get { return _canAttack; } set { _canAttack = value; } }
    [SerializeField] RangeBox _attackRangeBox; public RangeBox attackRangeBox { get { return _attackRangeBox; } }

    [SerializeField] float _rootMotionDeltaX; public float rootMotionDeltaX { get { return _rootMotionDeltaX; } }   


    [SerializeField] List<Unit> _triggers;

    public bool IncreaseComboStack()
    {
        if (_currentComboStack + 1 < _maxComboStack)
        {
            _currentComboStack++;
            return true;
        }
        return false;
    }

    public void ResetComboStack()
    {
        _currentComboStack = 0;
    }

    public Unit GetCurrentAttackUnit()
    {
        return _triggers[(int)_currentComboStack];
    }
}
