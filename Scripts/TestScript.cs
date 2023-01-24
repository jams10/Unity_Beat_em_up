using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestScript : MonoBehaviour
{
    [SerializeField] private List<Attack> attackPrefabs;

    List<Attack> attacks;
    Attack currentAttack;
    void Start()
    {
        attacks = new List<Attack>();    
        foreach(Attack attackPrefab in attackPrefabs)
        {
            attacks.Add(Instantiate(attackPrefab));
        }
    }

    // Update is called once per frame
    void Update()
    {


    }
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if(input != null)
        {
            Debug.Log(input);
        }
    }
    
}
