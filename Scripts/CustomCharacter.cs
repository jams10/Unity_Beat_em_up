using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateMask
{
    NONE = 0,
    DEAD = 1,
    RUNNING = 2,
    INAIR = 4,
    DAMAGED = 8,
    STUNNED = 16,
    ATTACKING = 32,
    DEFENDING = 64,
    INVINCIBLE = 128,
    END = 256,
}

public class CustomCharacter : MonoBehaviour
{
    [SerializeField] private int maxHealth;

    int currentHealth;
    int stateFlag;

    void Start()
    {
        stateFlag = 0;
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
    }
    #region State Functions
    public bool HasState(StateMask mask)
    {
        return (stateFlag & (int)mask) > 0;
    }
    public void AddState(StateMask mask)
    {
        stateFlag |= (int)mask;
    }
    public void RemoveState(StateMask mask)
    {
        stateFlag &= (int)~mask;
    }
    #endregion State Functions

    public bool TakeDamage(int damage)
    {
        int changedHealth = Math.Max(0, currentHealth - damage);
        if (currentHealth != changedHealth)
        {
            currentHealth = changedHealth;
            return true;
        }
        return false;
    }
}
