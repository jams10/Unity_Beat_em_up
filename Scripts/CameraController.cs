using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform targetToFollow;
    public Vector3 offsetFromTarget;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(targetToFollow != null)
        {
            transform.position = new Vector3(targetToFollow.position.x, 0, 0) + offsetFromTarget;
        }
    }
}
