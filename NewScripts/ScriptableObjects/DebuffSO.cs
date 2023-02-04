using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DebuffType
{
    STUN,
    DAMAGE,
    SLOW,
}

[CreateAssetMenu(fileName = "DebuffSpell", menuName = "Spell/Debuff", order = 1)]
public class DebuffSO : SpellSO
{
    [Header("DeBuff Spell")]
    [SerializeField] public DebuffType debuffType;

    [SerializeField] public float effectDuration;

    [SerializeField] public float effectTick;
}
