using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    SINGLE,
    AREA,
}

public enum SpellType
{
    ATTACK,
    BUFF,
    DEBUFF,
}

public abstract class SpellSO : ScriptableObject
{
    [Header("Descriptions")]
    [SerializeField] public string spellName;
    [SerializeField] [TextArea] public string description;
    [Header("Types")]
    [SerializeField] public TargetType targetType;
    [SerializeField] public SpellType spellType;
    [Header("Target Area Box")]
    [SerializeField] public float areaBoxOffsetX;
    [SerializeField] public Vector3 areaBoxScale;
    [SerializeField] public int manaCost;
}
