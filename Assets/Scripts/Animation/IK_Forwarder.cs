using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IK_Forwarder : MonoBehaviour
{
    public UnityAction onAnimatorIK; 
    // Start is called before the first frame update
    void OnAnimatorIK()
    {
        onAnimatorIK.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
