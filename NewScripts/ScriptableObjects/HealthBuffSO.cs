using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthBuffSpell", menuName = "Spell/HealthBuff", order = 1)]
public class HealthBuffSO : SpellSO
{ 
    [Header("Buff Spell")]
    [SerializeField] public int healAmount;
}
