using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [System.Serializable]
    public struct RangeBox // ���� ���� �ڽ� ������ ���� ����ü.
    {
        public float offsetX;
        public Vector3 scale;
    }
    [System.Serializable]
    public struct Unit // �� ���� ���ݿ� ���� ������ ���� ����ü.
    {
        public string attackTrigger;  // ���� �� �߻� ��ų �ִϸ��̼� Ʈ���� �̸�.
        public string damagedTrigger; // �������� ���� �� �߻� ��ų �ִϸ��̼� Ʈ���� �̸�.
        public Vector2 knockback;     // �������� ���� �� ������ �˹� ������ ��.
        public bool giveInvincible;   // �� ������ �´� ��� ���� �ð� ���� ������ ���� ���°� �Ǵ��� ����.
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
