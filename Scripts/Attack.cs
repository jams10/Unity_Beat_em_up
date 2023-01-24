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
        public string damagedTrigger; // 데미지를 받을 때 발생 시킬 애니메이션 트리거 이름.
        public Vector2 knockback;     // 데미지를 받을 때 적용할 넉백 스케일 값.
        public bool giveInvincible;   // 이 공격을 맞는 경우 일정 시간 동안 상대방이 무적 상태가 되는지 여부.
    }
    [SerializeField] int _damage; public int damage { get { return _damage; } }
    [SerializeField] uint _currentComboStack; public uint currentComboStack { get { return _currentComboStack; } }
    [SerializeField] uint _maxComboStack; public uint maxComboStack { get { return _maxComboStack; } }
    [SerializeField] float _cooldown; public float cooldown { get { return _cooldown; } }
    [SerializeField] bool _canAttack; public bool canAttack { get { return _canAttack; } set { _canAttack = value; } }
    [SerializeField] RangeBox _attackRangeBox; public RangeBox attackRangeBox { get { return _attackRangeBox; } }


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
