using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackSpell", menuName = "Spell/Attack", order = 1)]
public class AttackSO : SpellSO
{
    [Header("Attack Spell")]
    [SerializeField] public int damageAmount;

    [SerializeField] public int hitCount;

    [SerializeField] public Vector3 knockBackScale;
}
