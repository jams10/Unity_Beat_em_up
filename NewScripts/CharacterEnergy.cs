using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEnergy : MonoBehaviour
{
    [SerializeField] int maxHealth;

    int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public bool IncreaseHealth(int delta)
    {
        int newHealth = currentHealth + delta > maxHealth ? maxHealth : currentHealth + delta;
        if(newHealth != currentHealth)
        {
            currentHealth = newHealth;
            return true;
        }
        return false;
    }

    public bool DecreaseHealth(int delta)
    {
        int newHealth = currentHealth - delta < 0 ? 0 : currentHealth - delta;
        if (newHealth != currentHealth)
        {
            currentHealth = newHealth;
            return true;
        }
        return false;
    }
}
