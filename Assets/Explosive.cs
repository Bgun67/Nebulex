using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class Explosive : MonoBehaviour
{
    [SerializeField] VisualEffect vfx;
    // Start is called before the first frame update
    public void Activate(Player_Controller player)
    {
        vfx.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
