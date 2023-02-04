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
    CHARGING = 256,
    GRABBING = 512,
}

public class CharacterState : MonoBehaviour
{
    int state;

    public bool HasState(in StateMask mask)
    {
        return (state & (int)mask) > 0;
    }
    public void AddState(in StateMask mask)
    {
        state |= (int)mask;
    }
    public void RemoveState(in StateMask mask)
    {
        state &= (int)~mask;
    }
}
