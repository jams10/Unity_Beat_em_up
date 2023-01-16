using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacter : MonoBehaviour
{
    public enum CharacterStateMask
    {
        isAlive =       1,
        isRunning =     2,
        isInAir =       4,
        isAttacking =   8,
        isDamaged =     16,
        isStun =        32,
        isInvincible =  48,
    }

    [SerializeField] private int maxHealth;
    int characterStateFlag;
    int currentHealth;

    void Start()
    {
        characterStateFlag = 1;
        currentHealth = maxHealth;
    }

    void Update()
    {
         
    }

    public bool TakeDamage(int damage)
    {
        int changedHealth = Math.Max(0, currentHealth - damage);
        if(currentHealth != changedHealth)
        {
            currentHealth = changedHealth;
            return true;
        }
        return false;
    }

    #region Character State
    public bool HasCharacterState(CharacterStateMask mask)
    {
        return (characterStateFlag & (int)mask) > 0;
    }

    public void AddCharacterState(CharacterStateMask mask)
    {
        characterStateFlag |= (int)mask;
    }

    public void RemoveCharacterState(CharacterStateMask mask)
    {
        characterStateFlag &= (int)~mask;
    }
    #endregion Character State
}
